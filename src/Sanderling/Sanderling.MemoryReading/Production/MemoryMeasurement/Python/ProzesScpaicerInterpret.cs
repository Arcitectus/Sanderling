using BotEngine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Optimat.EveOnline
{
	public class SictNaacHerkunftAdrese : IComparer<SictAuswertObjMitAdrese>
	{
		public int Compare(SictAuswertObjMitAdrese a, SictAuswertObjMitAdrese b)
		{
			if (null == a && null == b)
			{
				return 0;
			}

			if (null == a)
			{
				return -1;
			}

			if (null == b)
			{
				return 1;
			}

			var aHerkunftAdrese = a.HerkunftAdrese;
			var bHerkunftAdrese = b.HerkunftAdrese;

			if (aHerkunftAdrese == bHerkunftAdrese)
			{
				return 0;
			}

			return (aHerkunftAdrese < bHerkunftAdrese) ? -1 : 1;
		}
	}

	public class SictAuswertObjMitAdrese
	{
		static public double? DoubleAusListeOktet(byte[] ListeOktet, int BeginOktetIndex)
		{
			if (null == ListeOktet)
			{
				return null;
			}

			if (ListeOktet.Length <= BeginOktetIndex + 8)
			{
				return null;
			}

			return BitConverter.ToDouble(ListeOktet, BeginOktetIndex);
		}

		static public Int32? Int32AusListeOktet(byte[] ListeOktet, int BeginOktetIndex)
		{
			if (null == ListeOktet)
			{
				return null;
			}

			if (ListeOktet.Length < BeginOktetIndex + 4)
			{
				return null;
			}

			return BitConverter.ToInt32(ListeOktet, BeginOktetIndex);
		}

		static public UInt32? UInt32AusListeOktet(byte[] ListeOktet, int BeginOktetIndex)
		{
			if (null == ListeOktet)
			{
				return null;
			}

			if (ListeOktet.Length < BeginOktetIndex + 4)
			{
				return null;
			}

			return BitConverter.ToUInt32(ListeOktet, BeginOktetIndex);
		}

		public Int32? Int32AusListeOktet(int BeginOktetIndex)
		{
			return SictAuswertObjMitAdrese.Int32AusListeOktet(this.ListeOktet, BeginOktetIndex);
		}

		public UInt32? UInt32AusListeOktet(int BeginOktetIndex)
		{
			return SictAuswertObjMitAdrese.UInt32AusListeOktet(this.ListeOktet, BeginOktetIndex);
		}

		readonly public	Int64 HerkunftAdrese;

		public byte[] ListeOktet
		{
			private set;
			get;
		}

		public SictAuswertObjMitAdrese(
			Int64 HerkunftAdrese,
			IMemoryReader DaatenKwele = null,
			byte[] ListeOktet = null)
		{
			this.HerkunftAdrese = HerkunftAdrese;

			this.ListeOktet = ListeOktet;

			Ersctele(DaatenKwele);
		}

		virtual public void Ersctele(IMemoryReader DaatenKwele)
		{
			LaadeListeOktetWenBisherKlainerAlsAnzaalVermuutung(DaatenKwele);
		}

		public void LaadeListeOktetWenBisherKlainerAlsAnzaalVermuutung(IMemoryReader DaatenKwele)
		{
			if (null == DaatenKwele)
			{
				return;
			}

			var ListeOktetAnzaal = Math.Min(0x10000, ListeOktetAnzaalBerecne());

			var ListeOktetLeese = false;

			if (null == ListeOktet)
			{
				ListeOktetLeese = true;
			}
			else
			{
				if (ListeOktet.Length < ListeOktetAnzaal)
				{
					ListeOktetLeese = true;
				}
			}

			if (ListeOktetLeese)
			{
				ListeOktet = DaatenKwele.ListeOktetLeeseVonAdrese(HerkunftAdrese, ListeOktetAnzaal, false);
			}
		}

		virtual public int ListeOktetAnzaalBerecne()
		{
			return 0;
		}
	}

	public class SictAuswertPythonDictEntry : SictAuswertObjMitAdrese
	{
		public const int EntryListeOktetAnzaal = 0xC;

		public Int32 hash
		{
			private set;
			get;
		}

		public Int64 ReferenzKey
		{
			private set;
			get;
		}

		public Int64 ReferenzValue
		{
			private set;
			get;
		}

		public SictAuswertPythonObj Key;
		public SictAuswertPythonObj Value;

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 12);
		}

		public SictAuswertPythonDictEntry(
			Int64 HerkunftAdrese,
			IMemoryReader DaatenKwele = null,
			byte[] ListeOktet = null)
			:
			base(HerkunftAdrese, DaatenKwele, ListeOktet)
		{
		}

		override public void Ersctele(IMemoryReader DaatenKwele)
		{
			base.Ersctele(DaatenKwele);

			hash = Int32AusListeOktet(ListeOktet, 0) ?? -1;
			ReferenzKey = UInt32AusListeOktet(ListeOktet, 4) ?? 0;
			ReferenzValue = UInt32AusListeOktet(ListeOktet, 8) ?? 0;
		}
	}

	/// <summary>
	/// 2013.07.17
	/// Auf höheren Durcsaz Optimiirte Versioon des DictEntry
	/// </summary>
	public class SictAuswertPythonDictEntryAinfac
	{
		public const int EntryListeOktetAnzaal = 0xC;

		public Int32 hash;

		public Int64 ReferenzKey;

		public Int64 ReferenzValue;

		public SictAuswertPythonObj Key;
		public SictAuswertPythonObj Value;

		public SictAuswertPythonDictEntryAinfac(
			UInt32[] AusListeEntryListeInt,
			int EntryIndex)
		{
			if(null	== AusListeEntryListeInt)
			{
				return;
			}

			hash = (Int32)AusListeEntryListeInt[EntryIndex * 3 + 0];
			ReferenzKey = AusListeEntryListeInt[EntryIndex * 3 + 1];
			ReferenzValue = AusListeEntryListeInt[EntryIndex * 3 + 2];
		}
	}

	/*
	 *
	/// <summary>
	/// 2013.09.22
	/// Optimiirung Durcsaz, Implementiirung als Value Type und Scpezialisatioon auf 32Bit
	/// </summary>
	public class SictAuswertPythonDictEntryAinfacValue32
	{
		public const int EntryListeOktetAnzaal = 0xC;

		public Int32 hash;

		public UInt32 ReferenzKey;

		public UInt32 ReferenzValue;

		public SictAuswertPythonObj Key;
		public SictAuswertPythonObj Value;

		public SictAuswertPythonDictEntryAinfac(
			UInt32[] AusListeEntryListeInt,
			int EntryIndex)
		{
			if (null == AusListeEntryListeInt)
			{
				return;
			}

			hash = (Int32)AusListeEntryListeInt[EntryIndex * 3 + 0];
			ReferenzKey = AusListeEntryListeInt[EntryIndex * 3 + 1];
			ReferenzValue = AusListeEntryListeInt[EntryIndex * 3 + 2];
		}
	}
	 * */

	public class SictAuswertPythonObj : SictAuswertObjMitAdrese
	{
		public Int32 RefCount
		{
			private set;
			get;
		}

		public Int64 RefType
		{
			private set;
			get;
		}

		public SictAuswertPythonObjType ObjType
		{
			set;
			get;
		}

		override public int ListeOktetAnzaalBerecne()
		{
			var SelbstScrankeMin = 8;

			var ObjType = this.ObjType;

			if (null != ObjType)
			{
				SelbstScrankeMin = Math.Max(SelbstScrankeMin, ObjType.tp_basicsize);
			}

			return Math.Max(base.ListeOktetAnzaalBerecne(), SelbstScrankeMin);
		}

		public SictAuswertPythonObj(
			Int64 HerkunftAdrese,
			byte[]	ListeOktet	= null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, DaatenKwele, ListeOktet)
		{
		}

		override public void Ersctele(IMemoryReader DaatenKwele)
		{
			base.Ersctele(DaatenKwele);

			RefCount = Int32AusListeOktet(ListeOktet, 0) ?? -1;
			RefType = UInt32AusListeOktet(ListeOktet, 4) ?? 0;
		}

		virtual public void LaadeReferenziirte(IMemoryReader DaatenKwele)
		{
		}
	}

	public class SictAuswertPythonObjVar : SictAuswertPythonObj
	{
		public Int32 ob_size
		{
			private set;
			get;
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 0xC);
		}

		public SictAuswertPythonObjVar(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public void Ersctele(IMemoryReader DaatenKwele)
		{
			base.Ersctele(DaatenKwele);

			ob_size = Int32AusListeOktet(ListeOktet, 8) ?? 0;
		}
	}

	public class SictAuswertPyObjGbsAst : SictAuswertPythonObjMitRefDictBaiPlus8
	{
		public const int AusDictRenderObjectRefVersazNaacRenderObjectBlok = -8;

		public Int64? AusDictParentRef;

		public Int64[] AusChildrenListRef;

		public Int64? AusDictRenderObjectRef;

		public SictAuswertPythonObjMitRefBaiPlus8 AusDictRenderObject;

		public SictAuswertPythonObj AusDictName;

		public string	AusDictNameString;

		public SictAuswertPythonObj AusDictText;

		public string	AusDictTextString;

		public SictAuswertPythonObj AusDictLinkText;

		public string	AusDictLinkTextString;

		public SictAuswertPythonObj AusDictSetText;

		public string	AusDictSetTextString;

		public SictAuswertPythonObj AusDictCaption;
		public string AusDictCaptionString;

		public SictAuswertPythonObj AusDictWindowID;
		public string AusDictWindowIDString;

		public SictAuswertPythonObj AusDictMinimized;
		public bool? AusDictMinimizedBool;
		public int? AusDictMinimizedInt;

		public SictAuswertPythonObj AusDictIsModal;
		public bool? AusDictIsModalBool;

		public SictAuswertPythonObj AusDictLastState;
		public double? AusDictLastStateFloat;

		public SictAuswertPythonObj AusDictRotation;
		public double? AusDictRotationFloat;

		public SictAuswertPythonObj AusDict_Sr;
		public SictAuswertPythonObj AusDict_SrEntryHtmlstr;
		public SictAuswertPythonObj AusDict_SrEntryNode;
		public string AusDict_SrEntryHtmlstrString;
		public string AusDict_SrEntryNodeGlyphStringText;

		public SictAuswertPythonObj AusDictColor;

		public SictAuswertPythonObj AusDictTexture;
		public Int64? AusDictTextureMemBlockPlus80Ref;

		public SictAuswertPyObjGbsAst(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 0x10);
		}

		override public void Ersctele(IMemoryReader DaatenKwele)
		{
			base.Ersctele(DaatenKwele);
		}
	}

	public class SictAuswertPyObjPyColor : SictAuswertPythonObjMitRefDictBaiPlus8
	{
		public SictAuswertPythonObj AusDictA;
		public int? AusDictWertAMilli;

		public SictAuswertPythonObj AusDictR;
		public int? AusDictWertRMilli;

		public SictAuswertPythonObj AusDictG;
		public int? AusDictWertGMilli;

		public SictAuswertPythonObj AusDictB;
		public int? AusDictWertBMilli;


		public SictAuswertPyObjPyColor(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 0x10);
		}

		override public void Ersctele(IMemoryReader DaatenKwele)
		{
			base.Ersctele(DaatenKwele);
		}
	}

	public class SictAuswertPythonObjDict : SictAuswertPythonObj
	{
		/* # Active + # Dummy */
		public Int32 ma_fill
		{
			private set;
			get;
		}

		/* # Active */
		public Int32 ma_used
		{
			private set;
			get;
		}

		/* The table contains ma_mask + 1 slots, and that's a power of 2.
		 * We store the mask instead of the size because the mask is more
		 * frequently needed.
		 */
		public	Int32 ma_mask
		{
			private set;
			get;
		}

		/* ma_table points to ma_smalltable for small tables, else to
		 * additional malloc'ed memory.  ma_table is never NULL!  This rule
		 * saves repeated runtime null-tests in the workhorse getitem and
		 * setitem calls.
		 */
		public	Int64	Ref_ma_table
		{
			private set;
			get;
		}

		/*
		 * 2013.07.18
		 * 
		public SictAuswertPythonDictEntry[] ListeDictEntry
		{
			private set;
			get;
		}
		 * */

		public SictAuswertPythonDictEntryAinfac[] ListeDictEntry
		{
			private set;
			get;
		}

		public SictAuswertPythonObjDict(
			Int64 HerkunftAdrese,
			byte[]	ListeOktet	= null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 0x18);
		}

		override public void Ersctele(IMemoryReader DaatenKwele)
		{
			base.Ersctele(DaatenKwele);

			ma_fill = Int32AusListeOktet(0x8)	?? 0;
			ma_used = Int32AusListeOktet(0xC)	?? 0;
			ma_mask = Int32AusListeOktet(0x10) ?? 0;
			Ref_ma_table = UInt32AusListeOktet(0x14) ?? 0;
		}

		override public void LaadeReferenziirte(
			IMemoryReader DaatenKwele)
		{
			LaadeReferenziirte(DaatenKwele, null);
		}

		public void LaadeReferenziirte(
			IMemoryReader DaatenKwele,
			int?	ListeEntryAnzaalScrankeMax)
		{
			if (null == DaatenKwele)
			{
				return;
			}

			var	Ref_ma_table	= this.Ref_ma_table;

			var ma_mask = this.ma_mask;
			var ma_used = this.ma_used;
			var ma_fill = this.ma_fill;

			if (ma_mask < ma_fill)
			{
				return;
			}

			if (0 != Ref_ma_table)
			{
				var ListeEntryAnzaal = ma_mask	+ 1;

				if (ListeEntryAnzaal < 0)
				{
					return;
				}

				if (ListeEntryAnzaalScrankeMax < ListeEntryAnzaal)
				{
					return;
				}

				var EntryListeOktetAnzaal = SictAuswertPythonDictEntry.EntryListeOktetAnzaal;

				var ListeEntryListeOktet = DaatenKwele.ListeOktetLeeseVonAdrese(Ref_ma_table, ListeEntryAnzaal * EntryListeOktetAnzaal, false);

				if (null != ListeEntryListeOktet)
				{
					/*
					 * 2013.07.18
					 * Perf: Umsctelung auf SictAuswertPythonDictEntryAinfac
					 * 
					var ListeEntry = new List<SictAuswertPythonDictEntry>();
					 * */

					var ListeEntry = new List<SictAuswertPythonDictEntryAinfac>();

					ListeEntryAnzaal = (int)(ListeEntryListeOktet.LongLength / EntryListeOktetAnzaal);

					{
						var ListeEntryListeInt = new UInt32[ListeEntryListeOktet.Length / 4];

						Buffer.BlockCopy(ListeEntryListeOktet, 0, ListeEntryListeInt, 0, ListeEntryListeInt.Length * 4);

						for (int EntryIndex = 0; EntryIndex < ListeEntryAnzaal; EntryIndex++)
						{
							var Entry = new SictAuswertPythonDictEntryAinfac(ListeEntryListeInt, EntryIndex);

							ListeEntry.Add(Entry);
						}
					}

					this.ListeDictEntry = ListeEntry.ToArray();
				}
			}
		}

	}

	public class SictAuswertPythonObjList : SictAuswertPythonObjVar
	{
		public Int64 Ref_ob_item
		{
			private set;
			get;
		}

		/* ob_item contains space for 'allocated' elements.  The number
		 * currently in use is ob_size.
		 * Invariants:
		 *     0 <= ob_size <= allocated
		 *     len(list) == ob_size
		 *     ob_item == NULL implies ob_size == allocated == 0
		 * list.sort() temporarily sets allocated to -1 to detect mutations.
		 *
		 * Items must normally not be NULL, except during construction when
		 * the list is not yet visible outside the function that builds it.
		 */
		public Int32 allocated
		{
			private set;
			get;
		}

		public Int64[] ListeItemRef
		{
			private set;
			get;
		}

		public SictAuswertPythonObjList(
			Int64 HerkunftAdrese,
			byte[]	ListeOktet	= null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 0x14);
		}

		override public void Ersctele(IMemoryReader DaatenKwele)
		{
			base.Ersctele(DaatenKwele);

			Ref_ob_item = UInt32AusListeOktet(0xC) ?? 0;
			allocated = Int32AusListeOktet(0x10) ?? 0;
		}

		override	public void LaadeReferenziirte(IMemoryReader DaatenKwele)
		{
			if (null == DaatenKwele)
			{
				return;
			}

			var Ref_ob_item = this.Ref_ob_item;

			if (0 != Ref_ob_item)
			{
				var ListeItemAnzaal = ob_size;

				var ItemListeOktetAnzaal = 4;

				var ListeItemListeOktet = DaatenKwele.ListeOktetLeeseVonAdrese(Ref_ob_item, ListeItemAnzaal * ItemListeOktetAnzaal, false);

				var	ListeItemRef	= new	List<Int64>();

				for (int ItemIndex = 0; ItemIndex < ListeItemListeOktet.LongLength / ItemListeOktetAnzaal; ItemIndex++)
				{
					var InBlokItemAdrese = ItemIndex * ItemListeOktetAnzaal;

					var	ItemRef	= UInt32AusListeOktet(ListeItemListeOktet, InBlokItemAdrese)	?? 0;

					ListeItemRef.Add(ItemRef);
				}

				this.ListeItemRef = ListeItemRef.ToArray();
			}
		}

	}

	/// <summary>
	/// https://github.com/evandrix/cPython-2.7.3/blob/master/Include/tupleobject.h
	/// </summary>
	public class SictAuswertPythonObjTuple : SictAuswertPythonObjVar
	{
		public Int64[] ListeItemRef
		{
			private set;
			get;
		}

		public SictAuswertPythonObjTuple(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 4 * this.ob_size + 0xC);
		}

		override public void Ersctele(IMemoryReader DaatenKwele)
		{
			base.Ersctele(DaatenKwele);

			var	ListeOktet	= this.ListeOktet;

			if (null != ListeOktet)
			{
				var ListeItemListeOktet = (ListeOktet ?? new byte[0]).Skip(0xC).ToArray();

				var ListeItemAnzaal = ListeItemListeOktet.Length / 4;

				var ListeItemRef = new UInt32[ListeItemAnzaal];

				Buffer.BlockCopy(ListeOktet, 0xC, ListeItemRef, 0, ListeItemRef.Length * 4);

				this.ListeItemRef = ListeItemRef.Select((ItemRef) => (Int64)ItemRef).ToArray();
			}
		}
	}

	public class SictAuswertPythonObjBunch : SictAuswertPythonObj
	{
		public const int AnnaameDictEntryAnzaalOktetIndex = 0x10;
		public const int AnnaameDictAdreseOktetIndex = 0x14;

		public int? AnnaameDictEntryAnzaal
		{
			private set;
			get;
		}

		public Int64? AnnaameRefListeDictEntry
		{
			private set;
			get;
		}

		public SictAuswertPythonDictEntryAinfac[] ListeEntry
		{
			private set;
			get;
		}

		public SictAuswertPythonObjBunch(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 132);
		}

		override public void Ersctele(IMemoryReader DaatenKwele)
		{
			base.Ersctele(DaatenKwele);

			AnnaameDictEntryAnzaal = Int32AusListeOktet(AnnaameDictEntryAnzaalOktetIndex);

			AnnaameRefListeDictEntry = UInt32AusListeOktet(AnnaameDictAdreseOktetIndex);

			/*
			 * 2013.08.00
			 * 
			var ListeOktet = this.ListeOktet;

			if (null	!= DaatenKwele	&&
				AnnaameRefListeDictEntry.HasValue	&&
				AnnaameDictEntryAnzaal.HasValue)
			{
				var EntryListeOktetAnzaal = SictAuswertPythonDictEntryAinfac.EntryListeOktetAnzaal;

				var ListeEntryListeOktet = DaatenKwele.ListeOktetLeeseVonAdrese(AnnaameRefListeDictEntry.Value, AnnaameDictEntryAnzaal.Value * EntryListeOktetAnzaal);

				var ListeEntry = new List<SictAuswertPythonDictEntryAinfac>();

				var	ListeEntryAnzaal = (int)(ListeEntryListeOktet.LongLength / EntryListeOktetAnzaal);

				{
					var ListeEntryListeInt = new UInt32[ListeEntryListeOktet.Length / 4];

					Buffer.BlockCopy(ListeEntryListeOktet, 0, ListeEntryListeInt, 0, ListeEntryListeInt.Length * 4);

					for (int EntryIndex = 0; EntryIndex < ListeEntryAnzaal; EntryIndex++)
					{
						var Entry = new SictAuswertPythonDictEntryAinfac(ListeEntryListeInt, EntryIndex);

						ListeEntry.Add(Entry);
					}
				}

				this.ListeEntry = ListeEntry.ToArray();
			}
			 * */
		}
		public void LaadeReferenziirte(
			IMemoryReader DaatenKwele,
			int? ListeEntryAnzaalScrankeMax)
		{
			if (null == DaatenKwele)
			{
				return;
			}

			var	AnnaameRefListeDictEntry	= this.AnnaameRefListeDictEntry;
			var	AnnaameDictEntryAnzaal	= this.AnnaameDictEntryAnzaal;

			if (AnnaameRefListeDictEntry.HasValue	&&
				AnnaameDictEntryAnzaal.HasValue)
			{
				var ListeEntryAnzaal = AnnaameDictEntryAnzaal.Value;

				if (ListeEntryAnzaal < 0)
				{
					return;
				}

				if (ListeEntryAnzaalScrankeMax < ListeEntryAnzaal)
				{
					return;
				}

				var EntryListeOktetAnzaal = SictAuswertPythonDictEntry.EntryListeOktetAnzaal;

				var ListeEntryListeOktet = DaatenKwele.ListeOktetLeeseVonAdrese(AnnaameRefListeDictEntry.Value, ListeEntryAnzaal * EntryListeOktetAnzaal, false);

				if (null != ListeEntryListeOktet)
				{
					var ListeEntry = new List<SictAuswertPythonDictEntryAinfac>();

					ListeEntryAnzaal = (int)(ListeEntryListeOktet.LongLength / EntryListeOktetAnzaal);

					{
						var ListeEntryListeInt = new UInt32[ListeEntryListeOktet.Length / 4];

						Buffer.BlockCopy(ListeEntryListeOktet, 0, ListeEntryListeInt, 0, ListeEntryListeInt.Length * 4);

						for (int EntryIndex = 0; EntryIndex < ListeEntryAnzaal; EntryIndex++)
						{
							var Entry = new SictAuswertPythonDictEntryAinfac(ListeEntryListeInt, EntryIndex);

							ListeEntry.Add(Entry);
						}
					}

					this.ListeEntry = ListeEntry.ToArray();
				}
			}
		}
	}

	/// <summary>
	/// https://bitbucket.org/mirror/cpython/src/5d41ebc79738cdf3fed6ea3a30519fb71c8efd0d/Include/unicodeobject.h?at=2.7
	/// </summary>
	public class SictAuswertPythonObjUnicode : SictAuswertPythonObj
	{
		readonly public Int32 LengthScranke	= 0x10000;

		public Int32 length
		{
			private set;
			get;
		}

		public Int64 Ref_str
		{
			private set;
			get;
		}

		public Int32 hash
		{
			private set;
			get;
		}

		public Int64 Ref_defenc
		{
			private set;
			get;
		}

		public	string	String
		{
			private	set;
			get;
		}

		public bool? LengthScrankeAingehalte
		{
			private set;
			get;
		}

		public SictAuswertPythonObjUnicode(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 0x18);
		}

		override public void Ersctele(IMemoryReader DaatenKwele)
		{
			base.Ersctele(DaatenKwele);

			length = Int32AusListeOktet(0x8) ?? 0;
			Ref_str = Int32AusListeOktet(0xC) ?? 0;
			hash = Int32AusListeOktet(0x10) ?? 0;
			Ref_defenc = UInt32AusListeOktet(0x14) ?? 0;
		}

		override public void LaadeReferenziirte(IMemoryReader DaatenKwele)
		{
			if (null == DaatenKwele)
			{
				return;
			}

			var Ref_str = this.Ref_str;
			var length = this.length;

			if (0 != Ref_str)
			{
				/*
				 * 2013.10.15
				 * Beobactung von Testsystem System.OverflowException mit Callstack:
				 * 
				   bei System.IntPtr.ToInt32()
				   bei Optimat.Glob.Kernel32ReadProcessMemory(IntPtr ProcessHandle, Int64 Adrese, Int64 ListeOktetAnzaalScrankeMax) in t:\Günta\Projekt\Ausboit VS\Debug W³\Entwiklung.Behältnis[1]\Anwendung\Optimat.PIpMR.Grepo\sln\Optimat.PIpMR.Grepo\Glob\Optimat.Glob.cs:Zeile 1608.
				   bei Optimat.SictAusProzesDirektLeeser.InternListeOktetLeeseVonAdrese(Int64 Adrese, Int64 ListeOktetZuLeeseAnzaal) in t:\Günta\Projekt\Ausboit VS\Debug W³\Entwiklung.Behältnis[1]\Anwendung\Optimat.PIpMR.Grepo\sln\Optimat.PIpMR.Grepo\ProzesScnapscus.cs:Zeile 20.
				   bei Optimat.EveOnline.SictAusProzesZuusctandLeeser.ListeOktetLeeseVonAdrese(Int64 Adrese, Int64 ListeOktetZuLeeseAnzaal, Boolean GibZurükNullWennGeleeseneAnzahlKlainer) in t:\Günta\Projekt\Ausboit VS\Debug W³\Entwiklung.Behältnis[1]\Anwendung\Optimat.PIpMR.Grepo\sln\Optimat.PIpMR.Grepo\ScpezEveOnln\Nuzer\ProzesScpaicerInterpret.cs:Zeile 18.
				   bei Optimat.EveOnline.SictAuswertPythonObjUnicode.LaadeReferenziirte(SictAusProzesZuusctandLeeser DaatenKwele) in t:\Günta\Projekt\Ausboit VS\Debug W³\Entwiklung.Behältnis[1]\Anwendung\Optimat.PIpMR.Grepo\sln\Optimat.PIpMR.Grepo\ScpezEveOnln\Nuzer\ProzesScpaicerInterpret.cs:Zeile 1041.
				   bei Optimat.EveOnline.SictProzesAuswertZuusctand.LaadeReferenziirte(SictAuswertPyObjGbsAst PyObj, SictAusProzesZuusctandLeeser ProzesLeeser, Boolean ObjSezeNaacScpaicer, Boolean ErmitleTypNurAusScpaicer, Nullable`1 DictListeEntryAnzaalScrankeMax) in t:\Günta\Projekt\Ausboit VS\Debug W³\Entwiklung.Behältnis[1]\Anwendung\Optimat.PIpMR.Grepo\sln\Optimat.PIpMR.Grepo\ScpezEveOnln\Nuzer\ProzesAuswertZuusctand.cs:Zeile 2121.
				   bei Optimat.EveOnline.Nuzer.SictProzesAuswert.Berecne(SictAusProzesZuusctandLeeser ProzesLeeser) in t:\Günta\Projekt\Ausboit VS\Debug W³\Entwiklung.Behältnis[1]\Anwendung\Optimat.PIpMR.Grepo\sln\Optimat.PIpMR.Grepo\ScpezEveOnln\Nuzer\ProzesAuswert.cs:Zeile 623.
				   bei Optimat.EveOnline.Nuzer.SictProzesAuswert.Berecne() in t:\Günta\Projekt\Ausboit VS\Debug W³\Entwiklung.Behältnis[1]\Anwendung\Optimat.PIpMR.Grepo\sln\Optimat.PIpMR.Grepo\ScpezEveOnln\Nuzer\ProzesAuswert.cs:Zeile 568.
				 * .......
				 * 
				 * Daher Ainfüürung LengthBescrankt.
				 * 
				var ListeOktetAnzaal = length * 2;
				 * */

				var LengthBescrankt = Math.Max(0, Math.Min(LengthScranke, length));

				LengthScrankeAingehalte = LengthBescrankt == length;

				var ListeOktetAnzaal = LengthBescrankt * 2;

				var StringListeOktet = DaatenKwele.ListeOktetLeeseVonAdrese(Ref_str, ListeOktetAnzaal, false);

				String = Encoding.Unicode.GetString(StringListeOktet);
			}
		}

	}

	public class SictAuswertPythonObjMitRefBaiPlus8 : SictAuswertPythonObj
	{
		public Int64	RefBaiOktet8
		{
			private set;
			get;
		}

		public SictAuswertPythonObjMitRefBaiPlus8(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 0xC);
		}

		override public void Ersctele(IMemoryReader DaatenKwele)
		{
			base.Ersctele(DaatenKwele);

			RefBaiOktet8 = UInt32AusListeOktet(ListeOktet, 8)	?? 0;
		}
	}

	public class SictAuswertPythonObjMitRefBaiPlus20 : SictAuswertPythonObj
	{
		public Int64	RefBaiOktet20
		{
			private set;
			get;
		}

		public SictAuswertPythonObjMitRefBaiPlus20(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 24);
		}

		override public void Ersctele(IMemoryReader DaatenKwele)
		{
			base.Ersctele(DaatenKwele);

			RefBaiOktet20 = UInt32AusListeOktet(ListeOktet, 20) ?? 0;
		}
	}

	public class SictAuswertPythonObjMitRefBaiPlus12 : SictAuswertPythonObj
	{
		public Int64	RefBaiOktet12
		{
			private set;
			get;
		}

		public SictAuswertPythonObjMitRefBaiPlus12(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 0xC);
		}

		override public void Ersctele(IMemoryReader DaatenKwele)
		{
			base.Ersctele(DaatenKwele);

			RefBaiOktet12 = UInt32AusListeOktet(ListeOktet, 12) ?? 0;
		}
	}

	public class SictAuswertPythonObjTrinityTr2Sprite2dTexture : SictAuswertPythonObjMitRefBaiPlus8
	{
		public SictAuswertPythonObjTrinityTr2Sprite2dTexture(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 0xC);
		}
	}

	public class SictAuswertPythonObjMitRefDictBaiPlus8 : SictAuswertPythonObjMitRefBaiPlus8, ISictAuswertPythonObjMitRefDict
	{
		public Int64 RefDict
		{
			get
			{
				return RefBaiOktet8;
			}
		}

		public SictAuswertPythonObjDict Dict
		{
			set;
			get;
		}

		public SictAuswertPythonObjMitRefDictBaiPlus8(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 0xC);
		}
	}

	public class SictAuswertPythonObjMitRefDictBaiPlus20 : SictAuswertPythonObjMitRefBaiPlus20, ISictAuswertPythonObjMitRefDict
	{
		public Int64 RefDict
		{
			get
			{
				return RefBaiOktet20;
			}
		}

		public SictAuswertPythonObjDict Dict
		{
			set;
			get;
		}

		public SictAuswertPythonObjMitRefDictBaiPlus20(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 24);
		}
	}

	public interface ISictAuswertPythonObjMitRefDict
	{
		Int64 RefDict
		{
			get;
		}

		SictAuswertPythonObjDict Dict
		{
			set;
			get;
		}
	}

	public class SictAuswertPythonObjMitRefDictBaiPlus12 : SictAuswertPythonObjMitRefBaiPlus12, ISictAuswertPythonObjMitRefDict
	{
		public Int64 RefDict
		{
			get
			{
				return RefBaiOktet12;
			}
		}

		public SictAuswertPythonObjDict Dict
		{
			set;
			get;
		}

		public SictAuswertPythonObjMitRefDictBaiPlus12(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 0x10);
		}
	}

	public class SictAuswertPythonObjInstance : SictAuswertPythonObjMitRefDictBaiPlus12
	{
		public SictAuswertPythonObjInstance(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}
	}

	public class SictAuswertPythonObjMitRefPyObjBaiPlus8 : SictAuswertPythonObjMitRefBaiPlus8
	{
		public SictAuswertPythonObj	PyObjRefVonOktet8;

		public SictAuswertPythonObjMitRefPyObjBaiPlus8(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 0xC);
		}
	}

	public class SictAuswertPythonObjWeakRef : SictAuswertPythonObjMitRefPyObjBaiPlus8
	{
		public SictAuswertPythonObjWeakRef(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 0x10);
		}
	}

	public class SictAuswertPythonObjPyOderUiChildrenList : SictAuswertPythonObjMitRefDictBaiPlus8
	{
		public SictAuswertPythonObjPyOderUiChildrenList(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 0x10);
		}
	}

	public class SictAuswertPythonObjTr2GlyphString : SictAuswertPythonObjMitRefDictBaiPlus20
	{
		public SictAuswertPythonObjTr2GlyphString(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 28);
		}
	}

	public class SictAuswertPythonObjStr : SictAuswertPythonObjVar
	{
		public const int StringBeginOktetIndex = 20;

		public	Int32 ob_shash
		{
			private set;
			get;
		}

		public	Int32 ob_sstate
		{
			private set;
			get;
		}

		public string String
		{
			private set;
			get;
		}

		public SictAuswertPythonObjStr(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), StringBeginOktetIndex	+ ob_size);
		}

		override public void Ersctele(IMemoryReader DaatenKwele)
		{
			base.Ersctele(DaatenKwele);

			ob_shash = Int32AusListeOktet(0xC)	?? 0;
			ob_sstate = Int32AusListeOktet(0x10)	?? 0;

			LaadeListeOktetWenBisherKlainerAlsAnzaalVermuutung(DaatenKwele);

			var ListeOktet = this.ListeOktet;

			if (null == ListeOktet)
			{
				return;
			}

			var StringListeOktet = ListeOktet.Skip(StringBeginOktetIndex).ToArray();

			String = Encoding.ASCII.GetString(StringListeOktet.TakeWhile((t) => 0 != t).ToArray());
		}
	}

	/// <summary>
	/// https://bitbucket.org/mirror/cpython/src/5063dab96843f5d078b4cbdcd8bc270b45c804fc/Objects/longobject.c?at=2.7
	/// </summary>
	public class SictAuswertPythonObjLong : SictAuswertPythonObjVar
	{
		public const int IntBeginOktetIndex = 12;

		public byte[] WertSictListeOktet
		{
			private set;
			get;
		}

		public Int64 WertSictIntModulo64Abbild
		{
			private set;
			get;
		}

		public SictAuswertPythonObjLong(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), IntBeginOktetIndex + 2 * ob_size);
		}

		override public void Ersctele(IMemoryReader DaatenKwele)
		{
			base.Ersctele(DaatenKwele);

			LaadeListeOktetWenBisherKlainerAlsAnzaalVermuutung(DaatenKwele);

			var ListeOktet = this.ListeOktet;

			if (null == ListeOktet)
			{
				return;
			}

			WertSictListeOktet = ListeOktet.Skip(IntBeginOktetIndex).ToArray();

			this.WertSictIntModulo64Abbild = WertSictIntModulo64(WertSictListeOktet);
		}

		static public Int64 WertSictIntModulo64(byte[] WertSictListeOktet)
		{
			if (null == WertSictListeOktet)
			{
				return	0;
			}

			if (8 <= WertSictListeOktet.Length)
			{
				return BitConverter.ToInt64(WertSictListeOktet,	0);
			}

			var WertSictListeOktetAufgefült = new byte[8];

			for (int i = 0; i < WertSictListeOktet.Length; i++)
			{
				WertSictListeOktetAufgefült[i] = WertSictListeOktet[i];
			}

			return BitConverter.ToInt64(WertSictListeOktetAufgefült, 0);
		}
	}

	/// <summary>
	/// https://bitbucket.org/mirror/cpython/src/5063dab96843f5d078b4cbdcd8bc270b45c804fc/Include/intobject.h?at=2.7
	/// </summary>
	public class SictAuswertPythonObjInt : SictAuswertPythonObj
	{
		public Int32? Int
		{
			private set;
			get;
		}

		public SictAuswertPythonObjInt(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 0x10);
		}

		override public void Ersctele(IMemoryReader DaatenKwele)
		{
			base.Ersctele(DaatenKwele);

			Int = Int32AusListeOktet(0x8);
		}
	}

	/// <summary>
	/// https://bitbucket.org/mirror/cpython/src/5063dab96843f5d078b4cbdcd8bc270b45c804fc/Include/intobject.h?at=2.7
	/// </summary>
	public class SictAuswertPythonObjFloat : SictAuswertPythonObj
	{
		public double? Float
		{
			private set;
			get;
		}

		public SictAuswertPythonObjFloat(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 0x10);
		}

		override public void Ersctele(IMemoryReader DaatenKwele)
		{
			base.Ersctele(DaatenKwele);

			Float =	SictAuswertPythonObj.DoubleAusListeOktet(this.ListeOktet,	0x8);
		}
	}

	/// <summary>
	/// https://bitbucket.org/mirror/cpython/src/5063dab96843f5d078b4cbdcd8bc270b45c804fc/Include/boolobject.h?at=2.7
	/// </summary>
	public class SictAuswertPythonObjBool : SictAuswertPythonObjInt
	{
		public bool?	Bool
		{
			get
			{
				var Int = this.Int;

				if (Int.HasValue)
				{
					return Int != 0;
				}

				return null;
			}
		}

		public SictAuswertPythonObjBool(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}
	}

	/// <summary>
	/// https://bitbucket.org/mirror/cpython/src/5d41ebc79738cdf3fed6ea3a30519fb71c8efd0d/Include/object.h?at=2.7
	/// </summary>
	public class SictAuswertPythonObjType : SictAuswertPythonObjVar
	{
		public Int64 ReferenzTp_name;

		public Int32 tp_basicsize;

		public Int32 tp_itemsize;

		public string tp_name;

		public SictAuswertPythonObjType(
			Int64 HerkunftAdrese,
			byte[] ListeOktet = null,
			IMemoryReader DaatenKwele = null)
			:
			base(HerkunftAdrese, ListeOktet, DaatenKwele)
		{
		}

		override public int ListeOktetAnzaalBerecne()
		{
			return Math.Max(base.ListeOktetAnzaalBerecne(), 0x400);
		}

		override public void Ersctele(IMemoryReader DaatenKwele)
		{
			base.Ersctele(DaatenKwele);

			ReferenzTp_name = UInt32AusListeOktet(ListeOktet, 0xC)	?? 0;

			tp_basicsize = Int32AusListeOktet(ListeOktet, 0x10) ?? 0;
			tp_itemsize = Int32AusListeOktet(ListeOktet, 0x14) ?? 0;
		}

		override public void LaadeReferenziirte(IMemoryReader DaatenKwele)
		{
			base.LaadeReferenziirte(DaatenKwele);

			if (null == DaatenKwele)
			{
				return;
			}

			var ReferenzTp_name = this.ReferenzTp_name;

			if (0 != ReferenzTp_name)
			{
				var TpNameListeOktet = DaatenKwele.ListeOktetLeeseVonAdrese(ReferenzTp_name, 0x100, false);

				if (null != TpNameListeOktet)
				{
					var TpName = Encoding.ASCII.GetString(TpNameListeOktet.TakeWhile((t) => 0 != t).ToArray());

					this.tp_name = TpName;
				}
			}
		}
	}
}
