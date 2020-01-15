using Bib3;
using System.Linq;

namespace Sanderling.ExploreProcessMeasurement
{
	/// <summary>
	/// Copied from the Sanderling.MemoryReading.Test project.
	/// </summary>
	static public class Extension
	{
		static public Interface.MemoryStruct.IMemoryMeasurement MemoryMeasurement(
			this BotEngine.Interface.IMemoryReader MemoryReader)
		{
			var GbsWurzelHaupt = MemoryReader?.GbsWurzelHaupt();

			return Optimat.EveOnline.AuswertGbs.Extension.SensorikScnapscusKonstrukt(GbsWurzelHaupt, null);
		}

		static public Optimat.EveOnline.GbsAstInfo GbsWurzelHaupt(
			this BotEngine.Interface.IMemoryReader memoryReader)
		{
			if (null == memoryReader)
				return null;

			return ReadUITreeFromRoot(memoryReader, memoryReader.SearchForUITreeRoot());
		}

		static public Optimat.EveOnline.GbsAstInfo ReadUITreeFromRoot(
			this BotEngine.Interface.IMemoryReader memoryReader,
			Optimat.EveOnline.MemoryAuswertWurzelSuuce searchForRoot)
		{
			var memoryMeasurementTask = new Optimat.EveOnline.SictProzesAuswertZuusctandScpezGbsBaum(
				memoryReader,
				searchForRoot,
				0x400,
				0x4000,
				0x40,
				searchForRoot?.GbsMengeWurzelObj?.Select(Wurzel => Wurzel?.HerkunftAdrese)?.WhereNotNullSelectValue()?.ToArray());

			memoryMeasurementTask.BerecneScrit();

			return memoryMeasurementTask.GbsWurzelHauptInfo;
		}

		static public Optimat.EveOnline.MemoryAuswertWurzelSuuce SearchForUITreeRoot(
			this BotEngine.Interface.IMemoryReader memoryReader)
		{
			var searchForRoot = new Optimat.EveOnline.MemoryAuswertWurzelSuuce(memoryReader);

			searchForRoot.Berecne();

			return searchForRoot;
		}
	}
}
