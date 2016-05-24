using System;
using System.Linq;
using System.Threading;
using Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;
using BotEngine.Client;
using BotEngine;

namespace Sanderling.Sample.Read
{
	/// <summary>
	/// This is just a minimal example of how to read from the eve online client process' memory.
	/// For more information, visit the project page at https://github.com/Arcitectus/Sanderling
	/// </summary>
	class Program
	{
		const int MeasurementTimeDistance = 1000;

		static void SampleRun()
		{
			Console.WriteLine("this program reads the memory of the eve online client process.");
			Console.WriteLine("start an eve online client and login to your account. Then press any key to continue.\n");
			Console.ReadKey();

			var Config = Extension.ConfigReadFromConsole();

			if (null == Config)
			{
				Console.WriteLine("reading config failed.");
				return;
			}

			var licenseClientConfig = new LicenseClientConfig
			{
				ApiVersionAddress = Config.LicenseServerAddress,
				Request = new AuthRequest
				{
					ServiceId = Config.ServiceId,
					LicenseKey = Config.LicenseKey,
					Consume = true,
				},
			};

			Console.WriteLine();
			Console.WriteLine("connecting to " + (licenseClientConfig?.ApiOverviewAddress ?? "") + " using Key \"" + (licenseClientConfig?.Request?.LicenseKey ?? "") + "\" ....");

			var sensorServerDispatcher = new SimpleInterfaceServerDispatcher
			{
				LicenseClientConfig = licenseClientConfig,
			};

			sensorServerDispatcher.CyclicExchangeStart();

			var exchangeAuth = sensorServerDispatcher?.LicenseClient?.ExchangeAuthLast?.Value;

			Console.WriteLine("Auth exchange completed ");

			if (exchangeAuth.AuthSuccess() ?? false)
				Console.WriteLine("successful");
			else
			{
				Console.WriteLine("with error: " + Environment.NewLine + exchangeAuth?.SerializeToString(Newtonsoft.Json.Formatting.Indented));
				return;
			}

			Console.WriteLine("\nstarting to set up the sensor and read from memory.\nthe initial measurement takes longer.");

			for (;;)
			{
				var response = sensorServerDispatcher?.InterfaceAppManager?.MeasurementTakeRequest(
					Config.EveOnlineClientProcessId,
					Bib3.Glob.StopwatchZaitMiliSictInt());

				if (null == response)
					Console.WriteLine("Sensor Interface not yet ready.");
				else
					MeasurementReceived(response?.MemoryMeasurement);

				Thread.Sleep(MeasurementTimeDistance);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="measurement">contains the structures read from the eve online client process memory.</param>
		static public void MeasurementReceived(BotEngine.Interface.FromProcessMeasurement<IMemoryMeasurement> measurement)
		{
			Console.WriteLine("\nMeasurement received");
			Console.WriteLine("measurement time: " + ((measurement?.End)?.ToString("### ### ### ### ###")?.Trim() ?? "null"));

			var ListUIElement =
				measurement?.Value?.EnumerateReferencedUIElementTransitive()
				?.GroupBy(uiElement => uiElement.Id)
				?.Select(group => group?.FirstOrDefault())
				?.ToArray();

			Console.WriteLine("number of UI elements in measurement: " + (ListUIElement?.Length.ToString() ?? "null"));

			var ContextMenu = measurement?.Value?.Menu?.FirstOrDefault();

			var ContextMenuFirstEntry = ContextMenu?.Entry?.FirstOrDefault();

			if (null == ContextMenuFirstEntry)
			{
				Console.WriteLine("no contextmenu open");
			}
			else
			{
				var Center = ContextMenuFirstEntry.Region.Center();

				Console.WriteLine("contextmenu first entry : label: \"" + (ContextMenuFirstEntry?.Text ?? "null") + "\", center location : " +
					Center.A.ToString() + ", " + Center.B.ToString());
			}
		}

		static void Main(string[] args)
		{
			SampleRun();

			Console.ReadKey();
		}
	}
}
