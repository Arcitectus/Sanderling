using Bib3;
using NUnit.Framework;
using Sanderling.Parse;
using System;
using System.Linq;

namespace Sanderling.MemoryReading.Test
{
	public class MemoryReadingDemo
	{
		[Test]
		public void Demo_memory_reading_from_process_sample()
		{
			var sampleFilePath =
				Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
				.PathToFilesysChild("Sanderling.Process.Sample")
				.PathToFilesysChild("my-eve-online-client-process-sample.zip");

			var snapshotZipArchiv = Bib3.Glob.InhaltAusDataiMitPfaad(sampleFilePath);

			var snapshot = BotEngine.Interface.Process.Snapshot.Extension.SnapshotFromZipArchive(snapshotZipArchiv);

			var memoryReader = new BotEngine.Interface.Process.Snapshot.SnapshotReader(snapshot?.ProcessSnapshot?.MemoryBaseAddressAndListOctet);

			var memoryMeasurement = memoryReader.MemoryMeasurement().Parse();

			//	At this point, you will find in the "memoryMeasurement" variable the contents read from the process sample as they would appear in the Sanderling API Explorer.

			Console.WriteLine("Overview window read: " + (memoryMeasurement.WindowOverview?.Any() ?? false));
		}
	}
}
