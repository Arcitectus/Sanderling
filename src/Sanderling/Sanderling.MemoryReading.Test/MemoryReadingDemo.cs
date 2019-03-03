using Bib3;
using NUnit.Framework;
using Sanderling.ExploreProcessMeasurement;
using Sanderling.Parse;
using System;
using System.Linq;

namespace Sanderling.MemoryReading.Test
{
	public class MemoryReadingDemo
	{
		[Test]
		[Explicit("Do not include this method when running all tests. The only reason this is marked as a `Test` is to simplify execution from Visual Studio UI.")]
		public void Demo_memory_reading_from_process_sample()
		{
			//	To get a process sample of the EVE Online client to use with this reading approach,
			//	see the guide at https://forum.botengine.org/t/how-to-collect-samples-for-memory-reading-development/50

			var windowsProcessMeasurementFilePath =
				Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
				.PathToFilesysChild("Sanderling.Process.Sample")
				.PathToFilesysChild("my-eve-online-client-process-sample.zip");

			var windowsProcessMeasurementZipArchive = System.IO.File.ReadAllBytes(windowsProcessMeasurementFilePath);

			var windowsProcessMeasurement = BotEngine.Interface.Process.Snapshot.Extension.SnapshotFromZipArchive(windowsProcessMeasurementZipArchive);

			var memoryReader = new BotEngine.Interface.Process.Snapshot.SnapshotReader(windowsProcessMeasurement?.ProcessSnapshot?.MemoryBaseAddressAndListOctet);

			//	The address of the root of the UI tree usually does not change in an EVE Online client process.
			//	Therefore the UI tree root search result is reused when reading the UI tree from the same process later.
			var searchForUITreeRoot = memoryReader.SearchForUITreeRoot();

			var memoryMeasurementPartialPythonModel = memoryReader?.ReadUITreeFromRoot(searchForUITreeRoot);

			var sanderlingMemoryMeasurement = Optimat.EveOnline.AuswertGbs.Extension.SensorikScnapscusKonstrukt(memoryMeasurementPartialPythonModel, null);

			var parsedMemoryMeasurement = sanderlingMemoryMeasurement.Parse();

			//	At this point, you will find in the "parsedMemoryMeasurement" variable the contents read from the process measurement as they would appear in the Sanderling API Explorer.

			Console.WriteLine("Overview window read: " + (parsedMemoryMeasurement.WindowOverview?.Any() ?? false));
		}
	}
}
