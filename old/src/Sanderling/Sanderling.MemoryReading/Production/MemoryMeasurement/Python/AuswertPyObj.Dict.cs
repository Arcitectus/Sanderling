using BotEngine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Optimat.EveOnline
{
	public class SictAuswertPyObj32DictZuusctand : SictAuswertPyObj32Zuusctand
	{
		public int ListeEntryAnzaalScrankeMax;

		public SictPyDictEntry32[] ListeDictEntry
		{
			private set;
			get;
		}

		SictAuswertPyObj32Zuusctand MaTableZwiscenscpaicer;

		public Int32 ma_fill
		{
			get
			{
				return ObjektBegin.BaiPlus8Int32;
			}
		}

		/* # Active */
		public Int32 ma_used
		{
			get
			{
				return ObjektBegin.BaiPlus12Int32;
			}
		}

		/* The table contains ma_mask + 1 slots, and that's a power of 2.
		 * We store the mask instead of the size because the mask is more
		 * frequently needed.
		 */
		public Int32 ma_mask
		{
			get
			{
				return ObjektBegin.BaiPlus16Int32;
			}
		}

		/* ma_table points to ma_smalltable for small tables, else to
		 * additional malloc'ed memory.  ma_table is never NULL!  This rule
		 * saves repeated runtime null-tests in the workhorse getitem and
		 * setitem calls.
		 */
		public Int64 Ref_ma_table
		{
			get
			{
				return ObjektBegin.BaiPlus20UInt32;
			}
		}

		public SictAuswertPyObj32DictZuusctand(
			Int64 HerkunftAdrese,
			Int64 BeginZait)
			:
			base(HerkunftAdrese, BeginZait)
		{
			ObjektListeOktetAnzaal = 0x20;
		}

		override public void Aktualisiire(
			IMemoryReader AusProzesLeeser,
			out	bool Geändert,
			Int64 Zait,
			int? ZuLeeseListeOktetAnzaal = null)
		{
			Geändert = false;

			bool BaseGeändert;

			var ListeEntryAnzaalScrankeMax = this.ListeEntryAnzaalScrankeMax;

			var VorherObjektBegin = this.ObjektBegin;

			var VorherRef_ma_table = this.Ref_ma_table;

			base.Aktualisiire(AusProzesLeeser, out	BaseGeändert, Zait, ZuLeeseListeOktetAnzaal);

			var NaacherObjektBegin = this.ObjektBegin;

			var Ref_ma_table = this.Ref_ma_table;

			var ma_mask = this.ma_mask;
			var ma_used = this.ma_used;
			var ma_fill = this.ma_fill;

			SictPyDictEntry32[] ListeDictEntry = null;

			try
			{
				if (null == AusProzesLeeser)
				{
					return;
				}

				if (ma_mask < ma_fill)
				{
					return;
				}

				if (0 != Ref_ma_table)
				{
					var ListeEntryAnzaal = ma_mask + 1;

					if (ListeEntryAnzaal < 0)
					{
						return;
					}

					if (ListeEntryAnzaalScrankeMax < ListeEntryAnzaal)
					{
						return;
					}

					var MaTableZwiscenscpaicer = this.MaTableZwiscenscpaicer;

					if (null != MaTableZwiscenscpaicer)
					{
						if (MaTableZwiscenscpaicer.HerkunftAdrese != Ref_ma_table)
						{
							MaTableZwiscenscpaicer = null;
						}
					}

					if (null == MaTableZwiscenscpaicer)
					{
						MaTableZwiscenscpaicer = new SictAuswertPyObj32Zuusctand(Ref_ma_table, Zait);
					}

					this.MaTableZwiscenscpaicer = MaTableZwiscenscpaicer;

					var EntryListeOktetAnzaal = SictPyDictEntry32.EntryListeOktetAnzaal;

					//	var ListeEntryListeOktet = AusProzesLeeser.ListeOktetLeeseVonAdrese(Ref_ma_table, ListeEntryAnzaal * EntryListeOktetAnzaal, false);

					bool MaTableGeändert;

					MaTableZwiscenscpaicer.Aktualisiire(AusProzesLeeser, out	MaTableGeändert, Zait, ListeEntryAnzaal * EntryListeOktetAnzaal);

					var MaTableZwiscenscpaicerVerarbaitetLezteListeOktetUndAnzaal = MaTableZwiscenscpaicer.AusScpaicerLeeseLezteListeOktetUndAnzaal;

					var ListeEntryListeOktet = MaTableZwiscenscpaicerVerarbaitetLezteListeOktetUndAnzaal.Key;

					if (null != ListeEntryListeOktet)
					{
						var ListeEntryListeOktetUnverändert = false;

						{
							//	Berecne ob Scpaicerinhalt bai Adrese Ref_ma_table sait lezter Aktualisatioon unverändert isc.
						}

						if (!ListeEntryListeOktetUnverändert)
						{
							//	Scpaicerinhalt wurde verändert, noies Array mit Entry anleege. (andere Funktioone di diises nuze werde di Identitäät des Array verwende um zu ermitle ob änderung Sctatgefunde hat.

							var GeleeseListeEntryAnzaal = (int)(MaTableZwiscenscpaicerVerarbaitetLezteListeOktetUndAnzaal.Value / EntryListeOktetAnzaal);

							var	InternListeDictEntry = new SictPyDictEntry32[GeleeseListeEntryAnzaal];

							var structHandle = GCHandle.Alloc(InternListeDictEntry, GCHandleType.Pinned);

							try
							{
								Marshal.Copy(ListeEntryListeOktet, 0, structHandle.AddrOfPinnedObject(), InternListeDictEntry.Length * EntryListeOktetAnzaal);
							}
							finally
							{
								structHandle.Free();
							}

							ListeDictEntry = InternListeDictEntry;
						}
					}
				}
			}
			finally
			{
				this.ListeDictEntry = ListeDictEntry;
			}
		}
	}
}
