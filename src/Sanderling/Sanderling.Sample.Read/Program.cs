using Bib3.Geometrik;
using Sanderling.Interface.MemoryStruct;
using System;
using System.Linq;
using System.Threading;

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

			var eveOnlineClientProcessId = Extension.ReadEveOnlineClientProcessIdFromConsole();

			if (null == eveOnlineClientProcessId)
			{
				Console.WriteLine("reading eve online client process id failed.");
				return;
			}

			Console.WriteLine("\nstarting to set up the sensor and read from memory.\nthe initial measurement takes longer.");

			var sensor = new Sensor();

			for (;;)
			{
				var response = sensor?.MeasurementTakeNewRequest(eveOnlineClientProcessId.Value);

				if (null == response)
					Console.WriteLine("Sensor Interface not yet ready.");
				else
					MeasurementReceived(response?.MemoryMeasurement);

				Thread.Sleep(MeasurementTimeDistance);
			}
		}

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
