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
		static string StringIdentifierFromValue(byte[] value)
		{
			using (var sha = new System.Security.Cryptography.SHA256Managed())
			{
				return BitConverter.ToString(sha.ComputeHash(value)).Replace("-", "");
			}
		}

		[Test]
		[Explicit("Do not include this method when running all tests. The only reason this is marked as a `Test` is to simplify execution from Visual Studio UI.")]
		public void Demo_memory_reading_from_process_sample()
		{
			//	To get a process sample of the EVE Online client to use with this reading approach,
			//	see the guide at https://forum.botengine.org/t/how-to-collect-samples-for-memory-reading-development/50

			var windowsProcessMeasurementFilePath =
				System.IO.Path.Combine(
					Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
					"BotEngine", "Sanderling.Process.Sample", "my-eve-online-client-process-sample.zip");

			var windowsProcessMeasurementZipArchive = System.IO.File.ReadAllBytes(windowsProcessMeasurementFilePath);

			var measurementId = StringIdentifierFromValue(windowsProcessMeasurementZipArchive);

			Console.WriteLine("Loaded sample " + measurementId + " from '" + windowsProcessMeasurementFilePath + "'");

			var windowsProcessMeasurement = BotEngine.Interface.Process.Snapshot.Extension.SnapshotFromZipArchive(windowsProcessMeasurementZipArchive);

			var memoryReader = new BotEngine.Interface.Process.Snapshot.SnapshotReader(windowsProcessMeasurement?.ProcessSnapshot?.MemoryBaseAddressAndListOctet);

			Console.WriteLine("I begin to search for the root of the UI tree...");

			//	The address of the root of the UI tree usually does not change in an EVE Online client process.
			//	Therefore the UI tree root search result is reused when reading the UI tree from the same process later.
			var searchForUITreeRoot = memoryReader.SearchForUITreeRoot();

			Console.WriteLine("I read the partial python model of the UI tree...");

			var memoryMeasurementPartialPythonModel = memoryReader?.ReadUITreeFromRoot(searchForUITreeRoot);

			var allNodesFromMemoryMeasurementPartialPythonModel =
				memoryMeasurementPartialPythonModel.EnumerateNodeFromTreeDFirst(node => node.GetListChild())
				.ToList();

			Console.WriteLine($"The tree in memoryMeasurementPartialPythonModel contains { allNodesFromMemoryMeasurementPartialPythonModel.Count } nodes");

			var memoryMeasurementReducedWithNamedNodes =
				Optimat.EveOnline.AuswertGbs.Extension.SensorikScnapscusKonstrukt(memoryMeasurementPartialPythonModel, null);

			var memoryMeasurementReducedWithNamedNodesParsedFurther =
				memoryMeasurementReducedWithNamedNodes.Parse();

			//	At this point, you will find in the "parsedMemoryMeasurement" variable the contents read from the process measurement as they would appear in the Sanderling API Explorer.

			Console.WriteLine("Overview window read: " + (memoryMeasurementReducedWithNamedNodesParsedFurther.WindowOverview?.Any() ?? false));

			{
				//	TODO: Improve accessibility: Move this derivation section into an artifact which can be easily referenced and executed from the Windows Console App.

				var destinationDirectoryPath =
					System.IO.Path.Combine(
						System.IO.Path.GetDirectoryName(windowsProcessMeasurementFilePath),
						"derivation",
						"from-" + measurementId);

				Console.WriteLine("I serialize the values from the different stages of memory reading and write those into files.");

				var stagesNamedValues = new[]
				{
					("partial-python", (object)memoryMeasurementPartialPythonModel),
					("reduced-with-named-nodes", (object)memoryMeasurementReducedWithNamedNodes),
					("reduced-with-named-nodes-parsed-further", (object)memoryMeasurementReducedWithNamedNodesParsedFurther),
				};

				foreach (var (stageName, stageValue) in stagesNamedValues)
				{
					var stageSerializedValue = System.Text.Encoding.UTF8.GetBytes(
							Newtonsoft.Json.JsonConvert.SerializeObject(
								stageValue,
								new Newtonsoft.Json.JsonSerializerSettings
								{
									NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,

									//	See https://stackoverflow.com/questions/7397207/json-net-error-self-referencing-loop-detected-for-type/18223985#18223985
									ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore,
								}));

					Console.WriteLine(stageName + " serialized to " + StringIdentifierFromValue(stageSerializedValue));

					var destinationFilePath = System.IO.Path.Combine(destinationDirectoryPath, stageName + ".json");

					System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(destinationFilePath));
					System.IO.File.WriteAllBytes(destinationFilePath, stageSerializedValue);

					Console.WriteLine(StringIdentifierFromValue(stageSerializedValue) + " written to '" + destinationFilePath + "'.");
				}
			}
		}
	}
}
