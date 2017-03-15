using Bib3;
using System.Linq;

namespace Sanderling.MemoryReading.Test
{
	static public class Extension
	{
		static public Interface.MemoryStruct.IMemoryMeasurement MemoryMeasurement(
			this BotEngine.Interface.IMemoryReader MemoryReader)
		{
			var GbsWurzelHaupt = MemoryReader?.GbsWurzelHaupt();

			return Optimat.EveOnline.AuswertGbs.Extension.SensorikScnapscusKonstrukt(GbsWurzelHaupt, null);
		}

		static public Optimat.EveOnline.GbsAstInfo GbsWurzelHaupt(
			this BotEngine.Interface.IMemoryReader MemoryReader)
		{
			if (null == MemoryReader)
			{
				return null;
			}

			var WurzelSuuce = new Optimat.EveOnline.MemoryAuswertWurzelSuuce(MemoryReader);

			WurzelSuuce.Berecne();

			var MemoryMeasurementTask = new Optimat.EveOnline.SictProzesAuswertZuusctandScpezGbsBaum(
				MemoryReader,
				WurzelSuuce,
				0x400,
				0x4000,
				0x40,
				WurzelSuuce?.GbsMengeWurzelObj?.Select(Wurzel => Wurzel?.HerkunftAdrese)?.WhereNotNullSelectValue()?.ToArray());

			MemoryMeasurementTask.BerecneScrit();

			return MemoryMeasurementTask.GbsWurzelHauptInfo;
		}
	}
}
