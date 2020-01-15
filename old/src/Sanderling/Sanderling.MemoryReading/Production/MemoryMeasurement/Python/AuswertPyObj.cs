using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Fasterflect;
using BotEngine.Interface;

namespace Optimat.EveOnline
{
	[StructLayout(LayoutKind.Explicit)]
	public struct SictPyObjAusrictCPython32
	{
		[FieldOffset(0)]
		public UInt32 RefCount;

		/// <summary>
		/// Adrese des Type Obj welces den Type diiser Instanz bescraibt.
		/// </summary>
		[FieldOffset(4)]
		public UInt32 RefType;
	}

	[StructLayout(LayoutKind.Explicit,	Size=32)]
	public struct SictPyObjAusrictCPython32Erwaitert1
	{
		[FieldOffset(0)]
		public SictPyObjAusrictCPython32 ObjBegin;

		[FieldOffset(8)]
		public UInt32 BaiPlus8UInt32;

		[FieldOffset(8)]
		public Int32 BaiPlus8Int32;

		[FieldOffset(8)]
		public Double BaiPlus8Double;

		[FieldOffset(12)]
		public Int32 BaiPlus12Int32;

		[FieldOffset(12)]
		public UInt32 BaiPlus12UInt32;

		[FieldOffset(16)]
		public Int32 BaiPlus16Int32;

		[FieldOffset(20)]
		public UInt32 BaiPlus20UInt32;
	}

	[StructLayout(LayoutKind.Explicit,	Size=12)]
	public struct SictPyDictEntry32
	{
		public const int EntryListeOktetAnzaal = 0xC;

		[FieldOffset(0)]
		public UInt32 hash;

		[FieldOffset(4)]
		public UInt32 ReferenzKey;

		[FieldOffset(8)]
		public UInt32 ReferenzValue;
	}

	public class SictAuswertPyObj32Zuusctand
	{
		static readonly	public	int ObjektBeginListeOktetAnzaal = Marshal.SizeOf(typeof(SictPyObjAusrictCPython32Erwaitert1));

		protected int ObjektListeOktetAnzaal = ObjektBeginListeOktetAnzaal;

		public	bool InternAnnaameImmutable
		{
			protected set;
			get;
		}

		/// <summary>
		/// Enthalt di Früühest in Funktioon Aktualisiire gemesene Adrese des Python Objekt Type.
		/// </summary>
		public UInt32? RefTypeFrühest
		{
			private set;
			get;
		}

		/// <summary>
		/// Gibt an ob in mindestens ainem Aufruuf von Funktioon Aktualisiire ain anderer Type als der in RefTypeFrühest angegeebene gemese wurde.
		/// Aufgrund der Annaame das Python Objekte zu deere Leebenszait nit an andere Orte versciibt kan wen diis true ist davon ausgegange werde das das Objekt an HerkunftAdrese entfernt wurde.
		/// </summary>
		public bool RefTypeGeändert
		{
			private set;
			get;
		}

		/// <summary>
		/// Struct werd in Array Instanziirt um bescraibe mit Buffer.BlockCopy zu ermööglice.
		/// </summary>
		readonly	SictPyObjAusrictCPython32Erwaitert1[] InternObjektBegin = new SictPyObjAusrictCPython32Erwaitert1[1];

		public SictPyObjAusrictCPython32Erwaitert1 ObjektBegin
		{
			get
			{
				return InternObjektBegin[0];
			}
		}

		public Int64 RefType
		{
			get
			{
				return ObjektBegin.ObjBegin.RefType;
			}
		}

		readonly public Int64 HerkunftAdrese;

		readonly public Int64 BeginZait;

		public Int64 AktualisLezteZait
		{
			private set;
			get;
		}

		public KeyValuePair<byte[], int> AusScpaicerLeeseLezteListeOktetUndAnzaal
		{
			private set;
			get;
		}

		public Optimat.EveOnline.SictAuswertPythonObjType TypeObjektKlas;

		public SictAuswertPyObj32Zuusctand(
			Int64 HerkunftAdrese,
			Int64 BeginZait)
		{
			this.HerkunftAdrese = HerkunftAdrese;
			this.BeginZait = BeginZait;
		}

		protected	int	ZwiscenscpaicerListeOktetFüleAusProzes(
			IMemoryReader AusProzesLeeser,
			int	ListeOktetAnzaal)
		{
			var	HerkunftAdrese	= this.HerkunftAdrese;

			var ListeOktetAnzaalBescrankt = Math.Max(0, ListeOktetAnzaal);

			var VorherZwiscenscpaicerListeOktet = AusScpaicerLeeseLezteListeOktetUndAnzaal.Key;

			var ZwiscenscpaicerListeOktet = VorherZwiscenscpaicerListeOktet;

			if (null != ZwiscenscpaicerListeOktet)
			{
				if (ZwiscenscpaicerListeOktet.Length < ListeOktetAnzaalBescrankt ||
					ListeOktetAnzaalBescrankt < ZwiscenscpaicerListeOktet.Length / 2 - 10)
				{
					//	Vorhandener Puffer ist zu klain oder zu groos und wird daher nit waiterverwendet.
					ZwiscenscpaicerListeOktet = null;
				}
			}

			if (null == ZwiscenscpaicerListeOktet)
			{
				ZwiscenscpaicerListeOktet = new byte[ListeOktetAnzaalBescrankt + 4];
			}
			else
			{
				//	Für Debugging: für den Fal das leese feelscläägt wääre es üübersictlicer wen Array nuur 0 enthalt.
				Array.Clear(ZwiscenscpaicerListeOktet, 0, ListeOktetAnzaalBescrankt);
			}

			int	GeleeseListeOktetAnzaal	= 0;

			try
			{
				if (null == AusProzesLeeser)
				{
					return GeleeseListeOktetAnzaal;
				}

				GeleeseListeOktetAnzaal	= (int)AusProzesLeeser.ListeOktetLeeseVonAdrese(HerkunftAdrese, ListeOktetAnzaalBescrankt, ZwiscenscpaicerListeOktet);
			}
			finally
			{
				this.AusScpaicerLeeseLezteListeOktetUndAnzaal = new KeyValuePair<byte[], int>(ZwiscenscpaicerListeOktet, GeleeseListeOktetAnzaal);
			}

			return GeleeseListeOktetAnzaal;
		}

		virtual	public void Aktualisiire(
			IMemoryReader AusProzesLeeser,
			out	bool	Geändert,
			Int64	Zait,
			int? ZuLeeseListeOktetAnzaal	= null)
		{
			if (!ZuLeeseListeOktetAnzaal.HasValue)
			{
				ZuLeeseListeOktetAnzaal = Math.Min(ObjektBeginListeOktetAnzaal, ObjektListeOktetAnzaal);
			}

			Geändert = false;

			if (null == AusProzesLeeser)
			{
				return;
			}

			AktualisLezteZait = Zait;

			ZuLeeseListeOktetAnzaal = Math.Max(0, ZuLeeseListeOktetAnzaal.Value);

			var VerarbaitetLezteListeOktet = this.AusScpaicerLeeseLezteListeOktetUndAnzaal;

			var GeleeseListeOktetAnzaal = ZwiscenscpaicerListeOktetFüleAusProzes(AusProzesLeeser, ZuLeeseListeOktetAnzaal.Value);

			var AusScpaicerLeeseLezteListeOktetUndAnzaal = this.AusScpaicerLeeseLezteListeOktetUndAnzaal;

			var ZuKopiireListeOktetAnzaal = Math.Min(GeleeseListeOktetAnzaal, ObjektBeginListeOktetAnzaal);

			if (null == AusScpaicerLeeseLezteListeOktetUndAnzaal.Key	||
				AusScpaicerLeeseLezteListeOktetUndAnzaal.Value < ObjektBeginListeOktetAnzaal)
			{
				//	Objekt Begin wurde nit volsctändig aus Prozes geleese, daher Werte in ObjektBegin auf 0 seze.
				Array.Clear(InternObjektBegin, 0, 1);
			}

			if (null != AusScpaicerLeeseLezteListeOktetUndAnzaal.Key)
			{
				/*
				 * 2013.11.06
				 * Buffer.BlockCopy funktioniirt nur mit primitive
				 * 
				Buffer.BlockCopy(ZwiscenscpaicerListeOktetUndAnzaal.Key, 0, InternObjektBegin, 0, Math.Min(ZwiscenscpaicerListeOktetUndAnzaal.Value, ZuLeeseListeOktetAnzaal));
				 * */

				//	Array.Copy(ZwiscenscpaicerListeOktetUndAnzaal.Key, InternObjektBegin, 1);

				/*
				 * 2014.09.22
				 * 
				 * folgende Metoode zum kopiire solte getesctet werde:
				 * http://www.ownedcore.com/forums/general/programming/345997-c-converting-array-of-bytes-array-of-structs.html
				 * 
				 * fixed (byte* pBuffer = bytes)
				 * for(int i = 0; i < count; i++)
				 *	ret[i] = ((StructureType*)pBuffer)[i];
				 * 
				 * */

				var	structHandle = GCHandle.Alloc(InternObjektBegin, GCHandleType.Pinned);

				try
				{
					Marshal.Copy(AusScpaicerLeeseLezteListeOktetUndAnzaal.Key, 0, structHandle.AddrOfPinnedObject(), ZuKopiireListeOktetAnzaal);
				}
				finally
				{
					structHandle.Free();
				}
			}

			var	ObjektBegin	= this.ObjektBegin;

			if (!RefTypeFrühest.HasValue)
			{
				RefTypeFrühest = ObjektBegin.ObjBegin.RefType;
			}

			if (!(RefTypeFrühest == ObjektBegin.ObjBegin.RefType))
			{
				RefTypeGeändert = true;
			}

			/*
			 * 2013.11.08
			 * Rükgaabewert Geändert werd zurzait nit verwendet.
			 * Fals diise überprüüfung nocmaals verwendet werden sol ist diise noc zu optimiire: Di Überprüüfung per SequenceEqual ist viilfac langsamer als optimum (memcmp).
			 * 
			if (null == VerarbaitetLezteListeOktet.Key)
			{
				Geändert = true;
			}
			else
			{
				if (AusScpaicerLeeseLezteListeOktetUndAnzaal.Value == VerarbaitetLezteListeOktet.Key.Length)
				{
					Geändert = true;
				}
				else
				{
					if (VerarbaitetLezteListeOktet.Key.SequenceEqual(AusScpaicerLeeseLezteListeOktetUndAnzaal.Key))
					{
						Geändert = true;
					}
				}
			}
			 * */

			this.AusScpaicerLeeseLezteListeOktetUndAnzaal = AusScpaicerLeeseLezteListeOktetUndAnzaal;
		}
	}

	public class SictAuswertPyObj32VarZuusctand : SictAuswertPyObj32Zuusctand
	{
		public SictAuswertPyObj32VarZuusctand(
			Int64 HerkunftAdrese,
			Int64 BeginZait)
			:
			base(HerkunftAdrese, BeginZait)
		{
		}

		public Int32 ob_size
		{
			private set;
			get;
		}

		override public void Aktualisiire(
			IMemoryReader AusProzesLeeser,
			out	bool Geändert,
			Int64 Zait,
			int? ZuLeeseListeOktetAnzaal = null)
		{
			base.Aktualisiire(
				AusProzesLeeser,
				out	Geändert,
				Zait,
				ZuLeeseListeOktetAnzaal);

			ob_size = ObjektBegin.BaiPlus8Int32; 
		}
	}

	public class SictAuswertPyObj32BunchZuusctand : SictAuswertPyObj32Zuusctand
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

		public SictPyDictEntry32[] ListeDictEntry
		{
			private set;
			get;
		}

		public int? ListeEntryAnzaalScrankeMax	= 0x400;

		public SictAuswertPyObj32BunchZuusctand(
			Int64 HerkunftAdrese,
			Int64 BeginZait)
			:
			base(HerkunftAdrese, BeginZait)
		{
		}

		override public void Aktualisiire(
			IMemoryReader AusProzesLeeser,
			out	bool Geändert,
			Int64 Zait,
			int? ZuLeeseListeOktetAnzaal = null)
		{
			ObjektListeOktetAnzaal = ObjektBeginListeOktetAnzaal;

			base.Aktualisiire(
				AusProzesLeeser,
				out	Geändert,
				Zait,
				ZuLeeseListeOktetAnzaal);

			var ObjektBegin = this.ObjektBegin;

			var	AnnaameDictEntryAnzaal = ObjektBegin.BaiPlus16Int32;
			var AnnaameRefListeDictEntry = ObjektBegin.BaiPlus20UInt32;

			this.AnnaameDictEntryAnzaal = AnnaameDictEntryAnzaal;
			this.AnnaameRefListeDictEntry = AnnaameRefListeDictEntry;

			SictPyDictEntry32[] ListeDictEntry = null;

			try
			{
				if (null == AusProzesLeeser)
				{
					return;
				}

				if (AnnaameDictEntryAnzaal < 0)
				{
					return;
				}

				if (0 < AnnaameDictEntryAnzaal)
				{
				}

				if (ListeEntryAnzaalScrankeMax < AnnaameDictEntryAnzaal)
				{
					return;
				}

				var EntryListeOktetAnzaal = SictPyDictEntry32.EntryListeOktetAnzaal;

				var ListeEntryListeOktet = AusProzesLeeser.ListeOktetLeeseVonAdrese(AnnaameRefListeDictEntry, AnnaameDictEntryAnzaal * EntryListeOktetAnzaal, false);

				if (null != ListeEntryListeOktet)
				{
					var ListeEntryListeOktetUnverändert = false;

					{
						//	Berecne ob Scpaicerinhalt bai Adrese Ref_ma_table sait lezter Aktualisatioon unverändert isc.
					}

					if (!ListeEntryListeOktetUnverändert)
					{
						//	Scpaicerinhalt wurde verändert, noies Array mit Entry anleege. (andere Funktioone di diises nuze werde di Identitäät des Array verwende um zu ermitle ob änderung Sctatgefunde hat.

						var GeleeseListeEntryAnzaal = (int)(ListeEntryListeOktet.Length	/ EntryListeOktetAnzaal);

						var InternListeDictEntry = new SictPyDictEntry32[GeleeseListeEntryAnzaal];

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
			finally
			{
				this.ListeDictEntry = ListeDictEntry;
			}
		}
	}

	public class SictAuswertPyObj32MitBaiPlus8RefZuusctand : SictAuswertPyObj32Zuusctand
	{
		public UInt32 BaiPlus8Ref
		{
			private set;
			get;
		}

		public SictAuswertPyObj32MitBaiPlus8RefZuusctand(
			Int64 HerkunftAdrese,
			Int64 BeginZait)
			:
			base(HerkunftAdrese, BeginZait)
		{
		}

		override public void Aktualisiire(
			IMemoryReader AusProzesLeeser,
			out	bool Geändert,
			Int64 Zait,
			int? ZuLeeseListeOktetAnzaal = null)
		{
			Geändert = false;

			bool BaseGeändert;

			base.Aktualisiire(AusProzesLeeser, out	BaseGeändert, Zait, ZuLeeseListeOktetAnzaal);

			Geändert = BaseGeändert;

			BaiPlus8Ref = ObjektBegin.BaiPlus8UInt32;
		}
	}

	public class SictAuswertPyObj32MitBaiPlus20RefZuusctand : SictAuswertPyObj32Zuusctand
	{
		public UInt32 BaiPlus20Ref
		{
			private set;
			get;
		}

		public SictAuswertPyObj32MitBaiPlus20RefZuusctand(
			Int64 HerkunftAdrese,
			Int64 BeginZait)
			:
			base(HerkunftAdrese, BeginZait)
		{
			ObjektListeOktetAnzaal = 24;
		}	

		override public void Aktualisiire(
			IMemoryReader AusProzesLeeser,
			out	bool Geändert,
			Int64 Zait,
			int? ZuLeeseListeOktetAnzaal = null)
		{
			Geändert = false;

			bool BaseGeändert;

			base.Aktualisiire(AusProzesLeeser, out	BaseGeändert, Zait, ZuLeeseListeOktetAnzaal);

			Geändert = BaseGeändert;

			BaiPlus20Ref = ObjektBegin.BaiPlus20UInt32;
		}
	}

	public class SictAuswertPyObj32MitBaiPlus8RefDictZuusctand : SictAuswertPyObj32MitBaiPlus8RefZuusctand
	{
		public int DictListeEntryAnzaalScrankeMax;

		public Int64 VonDictEntryMemberAktualisatioonNootwendigLezteZait
		{
			private set;
			get;
		}

		public Int64 RefDict
		{
			get
			{
				return BaiPlus8Ref;
			}
		}

		public SictAuswertPyObj32DictZuusctand DictObj
		{
			private set;
			get;
		}

		public SictAuswertPyObj32MitBaiPlus8RefDictZuusctand(
			Int64 HerkunftAdrese,
			Int64 BeginZait)
			:
			base(HerkunftAdrese, BeginZait)
		{
		}

		/// <summary>
		/// Sezt di Werte für Member welce AusDictEntry Attribut aufwaise auf Nul fals deren RefType geändert wurde.
		/// </summary>
		public	void VonDictEntryMemberEntferneWelceRefTypeGeändert(Int64	Zait)
		{
			var MengeZuDictEntryKeyMemberInfo =
				SictProzesAuswertZuusctandScpezGbsBaum.
				ZuTypeMengeZuDictKeyFieldMemberInfoAusScatenscpaicerOderBerecne(this.GetType());

			if (null != MengeZuDictEntryKeyMemberInfo)
			{
				foreach (var MemberInfo in MengeZuDictEntryKeyMemberInfo)
				{
					var MemberWert = MemberInfo.Value.Getter(this) as SictAuswertPyObj32Zuusctand;

					if (null == MemberWert)
					{
						continue;
					}

					if (MemberWert.RefTypeGeändert)
					{
						//	Type des Member wurde geändert, warscainlic isc des Uursprünglic überwacte Objekt entfernt worde.
						MemberInfo.Value.Setter(this, null);

						VonDictEntryMemberAktualisatioonNootwendigLezteZait = Zait;
					}
				}
			}
		}

		override public void Aktualisiire(
			IMemoryReader AusProzesLeeser,
			out	bool Geändert,
			Int64	Zait,
			int? ZuLeeseListeOktetAnzaal	= null)
		{
			Geändert = false;

			bool BaseGeändert;

			base.Aktualisiire(AusProzesLeeser, out	BaseGeändert, Zait, ZuLeeseListeOktetAnzaal);

			Geändert = BaseGeändert;

			if (null == AusProzesLeeser)
			{
				return;
			}

			var DictObj = this.DictObj;

			var	RefDict	= this.RefDict;

			if (null != DictObj)
			{
				if (DictObj.HerkunftAdrese != RefDict)
				{
					DictObj = null;
				}
			}

			if (null == DictObj)
			{
				DictObj = new SictAuswertPyObj32DictZuusctand(RefDict, Zait);

				DictObj.ListeEntryAnzaalScrankeMax = DictListeEntryAnzaalScrankeMax;
			}

			this.DictObj = DictObj;
		}
	}

	public class SictAuswertPyObj32Tr2GlyphStringZuusctand : SictAuswertPyObj32MitBaiPlus20RefDictZuusctand
	{
		public SictAuswertPyObj32Tr2GlyphStringZuusctand(
			Int64 HerkunftAdrese,
			Int64 BeginZait)
			:
			base(HerkunftAdrese, BeginZait)
		{
		}

	}

	public class SictAuswertPyObj32MitBaiPlus20RefDictZuusctand : SictAuswertPyObj32MitBaiPlus20RefZuusctand
	{
		public int DictListeEntryAnzaalScrankeMax;

		public Int64 VonDictEntryMemberAktualisatioonNootwendigLezteZait
		{
			private set;
			get;
		}

		public Int64 RefDict
		{
			get
			{
				return BaiPlus20Ref;
			}
		}

		public SictAuswertPyObj32DictZuusctand DictObj
		{
			private set;
			get;
		}

		public SictAuswertPyObj32MitBaiPlus20RefDictZuusctand(
			Int64 HerkunftAdrese,
			Int64 BeginZait)
			:
			base(HerkunftAdrese, BeginZait)
		{
		}

		/// <summary>
		/// Sezt di Werte für Member welce AusDictEntry Attribut aufwaise auf Nul fals deren RefType geändert wurde.
		/// </summary>
		public	void VonDictEntryMemberEntferneWelceRefTypeGeändert(Int64	Zait)
		{
			var MengeZuDictEntryKeyMemberInfo =
				SictProzesAuswertZuusctandScpezGbsBaum.
				ZuTypeMengeZuDictKeyFieldMemberInfoAusScatenscpaicerOderBerecne(this.GetType());

			if (null != MengeZuDictEntryKeyMemberInfo)
			{
				foreach (var MemberInfo in MengeZuDictEntryKeyMemberInfo)
				{
					var MemberWert = MemberInfo.Value.Getter(this) as SictAuswertPyObj32Zuusctand;

					if (null == MemberWert)
					{
						continue;
					}

					if (MemberWert.RefTypeGeändert)
					{
						//	Type des Member wurde geändert, warscainlic isc des Uursprünglic überwacte Objekt entfernt worde.
						MemberInfo.Value.Setter(this, null);

						VonDictEntryMemberAktualisatioonNootwendigLezteZait = Zait;
					}
				}
			}
		}

		override public void Aktualisiire(
			IMemoryReader AusProzesLeeser,
			out	bool Geändert,
			Int64	Zait,
			int? ZuLeeseListeOktetAnzaal	= null)
		{
			Geändert = false;

			bool BaseGeändert;

			base.Aktualisiire(AusProzesLeeser, out	BaseGeändert, Zait, ZuLeeseListeOktetAnzaal);

			Geändert = BaseGeändert;

			if (null == AusProzesLeeser)
			{
				return;
			}

			var DictObj = this.DictObj;

			var	RefDict	= this.RefDict;

			if (null != DictObj)
			{
				if (DictObj.HerkunftAdrese != RefDict)
				{
					DictObj = null;
				}
			}

			if (null == DictObj)
			{
				DictObj = new SictAuswertPyObj32DictZuusctand(RefDict, Zait);

				DictObj.ListeEntryAnzaalScrankeMax = DictListeEntryAnzaalScrankeMax;
			}

			this.DictObj = DictObj;
		}
	}

	public class SictAuswertPyObj32TextureZuusctand : SictAuswertPyObj32MitBaiPlus8RefZuusctand
	{
		public	UInt32	RefTexture
		{
			private set;
			get;
		}

		public SictAuswertPyObj32TextureZuusctand(
			Int64 HerkunftAdrese,
			Int64 BeginZait)
			:
			base(HerkunftAdrese, BeginZait)
		{
		}

		override public void Aktualisiire(
			IMemoryReader AusProzesLeeser,
			out	bool Geändert,
			Int64 Zait,
			int? ZuLeeseListeOktetAnzaal = null)
		{
			Geändert = false;

			bool BaseGeändert;

			base.Aktualisiire(AusProzesLeeser, out	BaseGeändert, Zait, ZuLeeseListeOktetAnzaal);

			Geändert = BaseGeändert;

			RefTexture = BaiPlus8Ref;
		}
	}

	[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
	public class SictInPyDictEntryKeyAttribut : Attribute
	{
		readonly public string DictEntryKeyString;

		public SictInPyDictEntryKeyAttribut(string DictEntryKeyString)
		{
			this.DictEntryKeyString = DictEntryKeyString;
		}
	}

	public class SictAuswertPyObj32PyOderUiChildrenList : SictAuswertPyObj32MitBaiPlus8RefDictZuusctand
	{
		[SictInPyDictEntryKeyAttribut("_childrenObjects")]
		public SictAuswertPyObj32Zuusctand DictEntryListChildrenObj;

		public SictAuswertPyObj32PyOderUiChildrenList(
			Int64 HerkunftAdrese,
			Int64 BeginZait)
			:
			base(HerkunftAdrese, BeginZait)
		{
			DictListeEntryAnzaalScrankeMax = 0x100;
		}
	}

	/// <summary>
	/// https://bitbucket.org/mirror/cpython/src/5063dab96843f5d078b4cbdcd8bc270b45c804fc/Include/intobject.h?at=2.7
	/// </summary>
	public class SictAuswertPyObj32Int32Zuusctand : SictAuswertPyObj32Zuusctand
	{
		public Int32 WertInt32
		{
			private set;
			get;
		}

		public SictAuswertPyObj32Int32Zuusctand(
			Int64 HerkunftAdrese,
			Int64 BeginZait)
			:
			base(HerkunftAdrese, BeginZait)
		{
		}

		override public void Aktualisiire(
			IMemoryReader AusProzesLeeser,
			out	bool Geändert,
			Int64 Zait,
			int? ZuLeeseListeOktetAnzaal = null)
		{
			base.Aktualisiire(
				AusProzesLeeser,
				out	Geändert,
				Zait,
				ZuLeeseListeOktetAnzaal);

			WertInt32 = ObjektBegin.BaiPlus8Int32;
		}
	}

	public class SictAuswertPyObj32BoolZuusctand : SictAuswertPyObj32Int32Zuusctand
	{
		public bool? WertBool
		{
			private set;
			get;
		}

		public SictAuswertPyObj32BoolZuusctand(
			Int64 HerkunftAdrese,
			Int64 BeginZait)
			:
			base(HerkunftAdrese, BeginZait)
		{
		}

		override public void Aktualisiire(
			IMemoryReader AusProzesLeeser,
			out	bool Geändert,
			Int64 Zait,
			int? ZuLeeseListeOktetAnzaal = null)
		{
			base.Aktualisiire(
				AusProzesLeeser,
				out	Geändert,
				Zait,
				ZuLeeseListeOktetAnzaal);

			WertBool = 0 != WertInt32;
		}
	}

	public class SictAuswertPyObj32Float64Zuusctand : SictAuswertPyObj32Zuusctand
	{
		public Double WertFloat64
		{
			private set;
			get;
		}

		public SictAuswertPyObj32Float64Zuusctand(
			Int64 HerkunftAdrese,
			Int64	BeginZait)
			:
			base(HerkunftAdrese, BeginZait)
		{
		}

		override public void Aktualisiire(
			IMemoryReader AusProzesLeeser,
			out	bool Geändert,
			Int64 Zait,
			int? ZuLeeseListeOktetAnzaal = null)
		{
			base.Aktualisiire(
				AusProzesLeeser,
				out	Geändert,
				Zait,
				ZuLeeseListeOktetAnzaal);

			WertFloat64 = ObjektBegin.BaiPlus8Double;
		}
	}

	public class SictObjektDictEntryAusSrBunch
	{
		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstSrBunchSclüselHtmlstrString)]
		public SictAuswertPyObj32Zuusctand DictEntryHtmlstr;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstSrBunchSclüselNodeString)]
		public SictAuswertPyObj32Zuusctand DictEntryNode;
	}

	public class SictObjektDictEntryAusSrBunchNode
	{
		[SictInPyDictEntryKeyAttribut("glyphString")]
		public SictAuswertPyObj32Zuusctand DictEntryGlyphString;
	}

	public class SictObjektDictEntryAusTr2GlyphStringDict
	{
		[SictInPyDictEntryKeyAttribut("text")]
		public SictAuswertPyObj32Zuusctand DictEntryText;
	}
}
