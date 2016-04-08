using System;
using System.Linq;
using System.Threading;
using Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;
using BotEngine.Interface;

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
			Console.WriteLine("this Programm reads the memory of the eve online client process.");
			Console.WriteLine("start an eve online client and login to your account. Then press any key to continue.\n");
			Console.ReadKey();

			var Config = Extension.ConfigReadFromConsole();

			if (null == Config)
			{
				Console.WriteLine("reading config failed.");
				return;
			}

			var licenseClientConfig = new BotEngine.Client.LicenseClientConfig
			{
				ApiVersionAddress = Config.LicenseServerAddress,
				Request = new BotEngine.Client.AuthRequest
				{
					ServiceId = Config.ServiceId,
					LicenseKey = Config.LicenseKey,
					Consume = true,
				},
			};

			Console.WriteLine();
			Console.WriteLine("connecting to " + (licenseClientConfig?.ApiOverviewAddress ?? "") + " using Key \"" + (licenseClientConfig?.Request?.LicenseKey ?? "") + "\" ....");

			var sensorServerDispatcher = new SimpleInterfaceServerDispatcher();

			var licenseClient = new Func<LicenseClient>(() => sensorServerDispatcher.LicenseClient);

			while (!(licenseClient()?.AuthCompleted ?? false))
			{
				sensorServerDispatcher.Exchange(licenseClientConfig);

				Thread.Sleep(1111);
			}

			var AuthResult = licenseClient()?.ExchangeAuthLast?.Value?.Response;

			var LicenseServerSessionId = AuthResult?.SessionId;

			Console.WriteLine("Auth completed, SessionId = " + (LicenseServerSessionId ?? "null"));

			Console.WriteLine("\nstarting to set up the sensor and read from memory.\nthe initial measurement takes longer.");

			for (;;)
			{
				sensorServerDispatcher?.Exchange();

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
		/// <param name="Measurement">contains the structures read from the eve online client process memory.</param>
		static public void MeasurementReceived(BotEngine.Interface.FromProcessMeasurement<IMemoryMeasurement> Measurement)
		{
			Console.WriteLine("\nMeasurement received");
			Console.WriteLine("measurement time: " + ((Measurement?.End)?.ToString("### ### ### ### ###")?.Trim() ?? "null"));

			var ListUIElement =
				Measurement?.Value?.EnumerateReferencedUIElementTransitive()
				?.GroupBy(UIElement => UIElement.Id)
				?.Select(Group => Group?.FirstOrDefault())
				?.ToArray();

			Console.WriteLine("number of UI elements in measurement: " + (ListUIElement?.Length.ToString() ?? "null"));

			var ContextMenu = Measurement?.Value?.Menu?.FirstOrDefault();

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
