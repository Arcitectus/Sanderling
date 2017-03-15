using Bib3;
using BotEngine;
using BotEngine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Optimat.EveOnline
{
	public abstract class SictProcessAuswertWurzelSuuce : Optimat.EveOnline.SictProzesAuswertZuusctand
	{
		virtual public void Berecne()
		{
		}

		static public SictProcessAuswertWurzelSuuce Berecne(
			SictProcessAuswertWurzelSuuce suuce)
		{
			if (null == suuce)
				return null;

			suuce.Berecne();

			return suuce;
		}
	}

	public class MemoryAuswertWurzelSuuce : SictProcessAuswertWurzelSuuce
	{
		readonly IMemoryReader MemoryReader;

		public MemoryAuswertWurzelSuuce(
			IMemoryReader memoryReader)
		{
			this.MemoryReader = memoryReader;
		}

		static Int64 DebugAdrese = -1;

		static public KeyValuePair<Int64, Int64>[] ListeScpaicerberaicGültigeZiil(IMemoryReader memoryReader)
		{
			var ProcessReader = memoryReader as BotEngine.Interface.ProcessMemoryReader;
			var SnapshotReader = memoryReader as BotEngine.Interface.Process.Snapshot.SnapshotReader;

			if (null != ProcessReader)
			{
				var ListRangeOfPages = BotEngine.Windows.Extension.ListRangeOfPagesFromProcessWithId((uint)ProcessReader.ProcessId, false);

				return
					ListRangeOfPages
					.Where(rangeOfPages => BotEngine.Windows.Extension.GültigeZaigerZiil(rangeOfPages.BasicInfo))
					?.Select(rangeOfPages => new KeyValuePair<Int64, Int64>(
						rangeOfPages.BasicInfo.BaseAddress.ToInt64(), rangeOfPages.BasicInfo.RegionSize.ToInt64()))
						?.ToArray();
			}

			if (null != SnapshotReader)
			{
				return
					SnapshotReader?.MemoryBaseAddressAndListOctet
					?.Select(memoryBaseAddressAndListOctet => new KeyValuePair<Int64, Int64>(
						memoryBaseAddressAndListOctet.Key, memoryBaseAddressAndListOctet.Value?.Length ?? 0))
					?.ToArray();
			}

			return null;
		}

		override public void Berecne()
		{
			InternDauer.BeginSezeJezt();

			try
			{
				var ListeScpaicerberaicGültigeZiil = SictProcessMitIdAuswertWurzelSuuce.ListeScpaicerberaicGültigeZiil(MemoryReader);

				if (ListeScpaicerberaicGültigeZiil.IsNullOrEmpty())
					return;

				var ListeAdresraumTailZuDurcsuuceBeginAdreseUndOktetAnzaal = new List<KeyValuePair<Int64, Int64>>();

				var TempProfileMengeMemBlokReserviirt = new List<Int64>();

				ListeAdresraumTailZuDurcsuuceBeginAdreseUndOktetAnzaal.AddRange(ListeScpaicerberaicGültigeZiil);

				var ProcessLeeser = MemoryReader;

				if (null == ProcessLeeser)
				{
					return;
				}

				var ListeKandidaatZaigerZiilUndKwele = new List<KeyValuePair<Int64, Int64>>();

				var ListeObjTypeKandidaatVorFilterSizeAdrese = new List<Int64>();

				var ObjTypeTypeMengeKandidaat = new List<Optimat.EveOnline.SictAuswertPythonObjType>();

				foreach (var AdresraumTailZuDurcsuuceBeginAdreseUndOktetAnzaal in ListeAdresraumTailZuDurcsuuceBeginAdreseUndOktetAnzaal)
				{
					var AdresraumTailZuDurcsuuceBeginAdrese = AdresraumTailZuDurcsuuceBeginAdreseUndOktetAnzaal.Key;
					var AdresraumTailZuDurcsuuceOktetAnzaal = AdresraumTailZuDurcsuuceBeginAdreseUndOktetAnzaal.Value;

					var AdresraumTailZuDurcsuuceListeUInt32 =
							ProcessLeeser.ReadArray<UInt32>(
							AdresraumTailZuDurcsuuceBeginAdrese,
							(int)AdresraumTailZuDurcsuuceOktetAnzaal);

					if (null == AdresraumTailZuDurcsuuceListeUInt32)
						continue;

					for (Int64 InAdresraumTailKandidaatIndex = 1,
							AdreseViirVorher = AdresraumTailZuDurcsuuceBeginAdrese;
							InAdresraumTailKandidaatIndex < AdresraumTailZuDurcsuuceListeUInt32.Length;
							++InAdresraumTailKandidaatIndex,
							AdreseViirVorher += 4)
					{
						var KandidaatZaigerZiilAdrese = AdresraumTailZuDurcsuuceListeUInt32[InAdresraumTailKandidaatIndex];

						//	Typ Type zaict uf sic selbsct
						var BedingungZaigtAufSelbsctErfült = AdreseViirVorher == KandidaatZaigerZiilAdrese;

						if (!BedingungZaigtAufSelbsctErfült)
							continue;

						var KandidaatPyObjBeginAdrese = KandidaatZaigerZiilAdrese;

						ListeObjTypeKandidaatVorFilterSizeAdrese.Add(KandidaatPyObjBeginAdrese);

						var KandidaatPyObjEndeAdrese = Math.Min(KandidaatPyObjBeginAdrese + 0x100, AdresraumTailZuDurcsuuceBeginAdrese + AdresraumTailZuDurcsuuceOktetAnzaal);

						var KandidaatPyObjListeOktetAnzaal = KandidaatPyObjEndeAdrese - KandidaatPyObjBeginAdrese;

						if (KandidaatPyObjListeOktetAnzaal < 0x10)
							continue;

						var KandidaatPyObjListeOktet = new byte[KandidaatPyObjListeOktetAnzaal];

						Buffer.BlockCopy(
							AdresraumTailZuDurcsuuceListeUInt32,
							(int)(KandidaatPyObjBeginAdrese - AdresraumTailZuDurcsuuceBeginAdrese),
							KandidaatPyObjListeOktet,
							0,
							KandidaatPyObjListeOktet.Length);

						var KandidaatPyObj = new Optimat.EveOnline.SictAuswertPythonObjType(KandidaatPyObjBeginAdrese, KandidaatPyObjListeOktet);

						if (0x10000 < KandidaatPyObj.tp_basicsize)
							continue;

						if (0x1000 < KandidaatPyObj.tp_itemsize)
							continue;

						KandidaatPyObj.LaadeReferenziirte(ProcessLeeser);

						if (!string.Equals("type", KandidaatPyObj.tp_name))
							continue;

						ObjTypeTypeMengeKandidaat.Add(KandidaatPyObj);
					}
				}

				var TempProfileMengeMemBlokReserviirtAggr0 = TempProfileMengeMemBlokReserviirt.Sum();

				if (!(1 == ObjTypeTypeMengeKandidaat.Count))
				{
					//	meer oder wenicer als ain Kandidaat für Type Type gefunde.
				}

				PyObjTypType = ObjTypeTypeMengeKandidaat.FirstOrDefault();

				if (null == PyObjTypType)
					return;

				var PyObjTypTypeAdrese = PyObjTypType.HerkunftAdrese;

				var BaumZaigerTiifeScrankeMax = Math.Max(1, (int)Math.Log(ListeKandidaatZaigerZiilUndKwele.Count, 2) - 4);

				var BaumZaigerVonZiilNaacKwele = SictAstBinär13<Int64>.ErscteleBaumAusListe(ListeKandidaatZaigerZiilUndKwele, BaumZaigerTiifeScrankeMax);

				{
					var MengePyObjTyp = new List<Optimat.EveOnline.SictAuswertPythonObjType>();

					var MengeKandidaatPyObjTypeAdrese =
						ListeAdresraumTailZuDurcsuuceBeginAdreseUndOktetAnzaal
						?.Select(region => ProcessLeeser.AddressesHoldingValue32Aligned32(
						(uint)PyObjTypTypeAdrese,
						region.Key, region.Value + region.Key))
						?.ConcatNullable()
						?.ToArray();

					for (int KandidaatIndex = 0; KandidaatIndex < MengeKandidaatPyObjTypeAdrese.Length; KandidaatIndex++)
					{
						var KandidaatAdrese = MengeKandidaatPyObjTypeAdrese[KandidaatIndex] - 4;

						var PyObj = new Optimat.EveOnline.SictAuswertPythonObjType(KandidaatAdrese, null, ProcessLeeser);

						PyObj.LaadeReferenziirte(ProcessLeeser);

						if (null == PyObj.tp_name)
							continue;

						if (PyObj.tp_name.Length < 1)
							continue;

						MengePyObjTyp.Add(PyObj);
					}

					foreach (var PyObjType in MengePyObjTyp)
						PyObjSezeNaacScpaicer(PyObjType);

					MengePyObjTypSezeAusMengeFürHerkunftAdrPyObj();
				}

				FüleRefTypScpezVonMengePyObjType();

				if (null == PyObjTypUIRoot)
					return;

				{
					/*
					 * 2013.07.13
					 * Suuce GBS Wurzel per Typ UIRoot
					 * */

					var GbsMengeWurzelObj = new List<Optimat.EveOnline.SictAuswertPyObjGbsAst>();

					var ListeKandidaatPyObjUIRootZaigerAufTypKweleAdrese =
						ListeAdresraumTailZuDurcsuuceBeginAdreseUndOktetAnzaal
						?.Select(region => ProcessLeeser.AddressesHoldingValue32Aligned32(
						(uint)PyObjTypUIRoot.HerkunftAdrese,
						region.Key, region.Value + region.Key))
						?.ConcatNullable()
						?.ToArray();

					for (int KandidaatIndex = 0; KandidaatIndex < ListeKandidaatPyObjUIRootZaigerAufTypKweleAdrese.Length; KandidaatIndex++)
					{
						var KandidaatAdrese = ListeKandidaatPyObjUIRootZaigerAufTypKweleAdrese[KandidaatIndex] - 4;

						var Kandidaat = new Optimat.EveOnline.SictAuswertPyObjGbsAst(KandidaatAdrese, null, ProcessLeeser);

						LaadeReferenziirte(Kandidaat, ProcessLeeser, true, true);

						if (null == Kandidaat.Dict)
							continue;

						var AusChildrenListRef = Kandidaat.AusChildrenListRef;

						if (null == AusChildrenListRef)
							continue;

						if (AusChildrenListRef.Length < 1)
							continue;

						GbsMengeWurzelObj.Add(Kandidaat);
					}

					this.GbsMengeWurzelObj = GbsMengeWurzelObj.ToArray();
				}
			}
			catch
			{
				//	Report Exception
			}
			finally
			{
				InternDauer.EndeSezeJezt();
			}
		}
	}

	public class SictProcessMitIdAuswertWurzelSuuce : MemoryAuswertWurzelSuuce
	{
		readonly public int ProcessId;

		public SictProcessMitIdAuswertWurzelSuuce(int processId)
			: base(new ProcessMemoryReader(processId))
		{
			ProcessId = processId;
		}
	}
}