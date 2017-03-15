using Bib3;
using BotEngine.EveOnline.Sensor.Option.MemoryMeasurement.SictGbs;
using BotEngine.Interface;
using System;
using System.Linq;

namespace Optimat.EveOnline
{
	public partial class SictProzesAuswertZuusctandScpezGbsBaum
	{
		void _16_Erwaiterung_Manuel(
			Int64 zait,
			IMemoryReader ausProzesLeeser,
			SictProzesAuswertZuusctand prozesAuswertZuusctand,
			SictAuswertPyObj32GbsAstZuusctand gbsAst,
			GbsAstInfo ziilAstInfo)
		{
			if (null == ziilAstInfo)
				return;

			var listeDictEntry = gbsAst.DictObj?.ListeDictEntry;

			var dictEntryValueObjFromKey = new Func<string, SictAuswertPyObj32Zuusctand>(keyString =>
			{
				foreach (var dictEntry in listeDictEntry.EmptyIfNull())
				{
					var candidateKeyString = (ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer(dictEntry.ReferenzKey, ausProzesLeeser, prozesAuswertZuusctand, Zait) as SictAuswertPyObj32StrZuusctand)?.WertString;

					if (!(keyString == candidateKeyString))
						continue;

					var entryValueObj = ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer(dictEntry.ReferenzValue, ausProzesLeeser, prozesAuswertZuusctand, Zait);

					if (null == entryValueObj)
						continue;

					bool t;

					entryValueObj?.Aktualisiire(ausProzesLeeser, out t, zait);

					return entryValueObj;
				}

				return null;
			});

			var dictListKeyStringValueNotEmptyLazy = new Lazy<string[]>(() =>
				listeDictEntry?.Select(dictEntry =>
				{
					var keyString = (ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer(dictEntry.ReferenzKey, ausProzesLeeser, prozesAuswertZuusctand, Zait) as SictAuswertPyObj32StrZuusctand)?.WertString;

					if (null == keyString)
						return null;

					var value = ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer(dictEntry.ReferenzValue, ausProzesLeeser, prozesAuswertZuusctand, Zait);

					if (null == value)
						return null;

					bool t;

					value.Aktualisiire(ausProzesLeeser, out t, Zait);

					var Type = prozesAuswertZuusctand.MengeFürHerkunftAdrPyObj?.TryGetValueOrDefault(value.RefType);

					var tp_name = (Type as SictAuswertPythonObjType)?.tp_name;

					if (!(0 < tp_name?.Length))
						return null;

					if ("NoneType" == tp_name)
						return null;

					return keyString;
				})
				?.WhereNotDefault()
				?.ToArrayIfNotEmpty());

			if (AuswertGbs.SictAuswertGbsWindowOverviewZaile.MainIconPyTypeName == ziilAstInfo.PyObjTypName)
				ziilAstInfo.DictListKeyStringValueNotEmpty = dictListKeyStringValueNotEmptyLazy.Value;

			if (SquadronUIExtension.FightersHealthGaugePyTypeName == ziilAstInfo.PyObjTypName)
			{
				var squadronSizeValue = dictEntryValueObjFromKey("squadronSize");
				var squadronMaxSizeValue = dictEntryValueObjFromKey("squadronMaxSize");

				ziilAstInfo.SquadronSize = (squadronSizeValue as SictAuswertPyObj32Int32Zuusctand)?.WertInt32;
				ziilAstInfo.SquadronMaxSize = (squadronMaxSizeValue as SictAuswertPyObj32Int32Zuusctand)?.WertInt32;
			}

			if (AuswertGbs.SictAuswertGbsShipUiSlotsSlot.ModuleButtonPyTypeName == ziilAstInfo.PyObjTypName ||
				SquadronUIExtension.AbilityIconPyTypeName == ziilAstInfo.PyObjTypName)
			{
				var rampActiveValue = dictEntryValueObjFromKey("ramp_active");

				ziilAstInfo.RampActive = (rampActiveValue as SictAuswertPyObj32BoolZuusctand)?.WertBool;
			}
		}
	}
}
