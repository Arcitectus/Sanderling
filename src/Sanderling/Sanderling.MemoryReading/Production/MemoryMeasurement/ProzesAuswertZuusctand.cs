using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bib3;
using Fasterflect;
using Optimat.EveOnline;
using BotEngine.Interface;

namespace Optimat.EveOnline
{
	/// <summary>
	/// Auswertung des Scpaicerblok welcer vom RenderObject des GbsAst referenziirt werd.
	/// </summary>
	public class SictGbsAstAusRenderObjectMemBlok
	{
		static int OktetSictbarkaitAdrese = 0x48;

		static public int GbsBaumAstListeOktetAnzaal = 396;

		static public int Referenz0KweleAdreseDistanzVonBlokBegin = 0xc0;
		static public int Referenz0ZiilAdreseDistanzVonBlokBegin = 0x50;
		static public int TestSingleDistanzVonBlokBegin = 0x1c;

		public Int64 Adrese;

		public byte[] ListeOktet;

		public SictGbsAstAusRenderObjectMemBlok[] MengeKandidaatAstEnthalte;

		public SictGbsAstAusRenderObjectMemBlok[] MengeAstEnthalte
		{
			get
			{
				var ListeKandidaatAstEnthalte = this.MengeKandidaatAstEnthalte;

				if (null == ListeKandidaatAstEnthalte)
				{
					return null;
				}

				var ListeAstEnthalte =
					ListeKandidaatAstEnthalte
					.Where((Kandidaat) => null != Kandidaat.ListeOktet)
					.ToArray();

				return ListeAstEnthalte;
			}
		}

		public SictGbsAstAusRenderObjectMemBlok(Int64 Adrese, byte[] ListeOktet = null)
		{
			this.Adrese = Adrese;
			this.ListeOktet = ListeOktet;
		}

		public int ZääleAstAnzaalRekursiiv(int? DistanzScrankeMax = null)
		{
			var ListeAstEnthalte = this.MengeAstEnthalte;

			if (null == ListeAstEnthalte)
			{
				return 0;
			}

			var ListeAstAnzaal =
				ListeAstEnthalte
				.Select((Ast) => null == Ast ? 0 : Ast.ZääleAstAnzaalRekursiiv(DistanzScrankeMax - 1))
				.ToArray();

			return ListeAstAnzaal.Sum() + ListeAstAnzaal.Count();
		}

		public bool BerecneObEnthältAstMitBeginAdrese(Int64 AstBeginAdrese, int? TiifeScrankeMax = null)
		{
			if (Adrese == AstBeginAdrese)
			{
				return true;
			}

			if (TiifeScrankeMax < 1)
			{
				return false;
			}

			var ListeAstEnthalte = this.MengeAstEnthalte;

			if (null == ListeAstEnthalte)
			{
				return false;
			}

			return ListeAstEnthalte.Any((Ast) => Ast.BerecneObEnthältAstMitBeginAdrese(AstBeginAdrese, TiifeScrankeMax - 1));
		}

		public SictGbsAstAusRenderObjectMemBlok[] BerecneMengeEnthalteneAst(int? TiifeScrankeMax = null)
		{
			var MengeAstEnthalte = this.MengeAstEnthalte;

			var ListeAstMengeAst = new List<SictGbsAstAusRenderObjectMemBlok[]>();

			ListeAstMengeAst.Add(new SictGbsAstAusRenderObjectMemBlok[] { this });

			if (TiifeScrankeMax < 1 || null == MengeAstEnthalte)
			{
				goto Ende;
			}

			foreach (var Ast in MengeAstEnthalte)
			{
				ListeAstMengeAst.Add(Ast.BerecneMengeEnthalteneAst(TiifeScrankeMax - 1));
			}

			Ende:

			return Bib3.Glob.ArrayAusListeFeldGeflact(ListeAstMengeAst);
		}

		public SictGbsAstAusRenderObjectMemBlok[] BerecnePfaadZuAst(SictGbsAstAusRenderObjectMemBlok Ast)
		{
			if (null == Ast)
			{
				return null;
			}

			if (Ast.Adrese == Adrese)
			{
				return new SictGbsAstAusRenderObjectMemBlok[] { this };
			}

			var MengeAstEnthalte = this.MengeAstEnthalte;

			if (null == MengeAstEnthalte)
			{
				return null;
			}

			var MengeAstEnthalteMitPfaad =
				MengeAstEnthalte
				.Select((AstEnthalte) => new KeyValuePair<SictGbsAstAusRenderObjectMemBlok, SictGbsAstAusRenderObjectMemBlok[]>(AstEnthalte, AstEnthalte.BerecnePfaadZuAst(Ast)))
				.ToArray();

			var Pfaad =
				MengeAstEnthalteMitPfaad
				.OrderByDescending((Kandidaat) => (null == Kandidaat.Value) ? 0 : Kandidaat.Value.Length)
				.FirstOrDefault();

			if (null != Pfaad.Value)
			{
				return new SictGbsAstAusRenderObjectMemBlok[] { this }.Concat(Pfaad.Value).ToArray();
			}

			return null;
		}

		public void InMengeAstEnthalteFüügeAinFallsPasend(
			SictGbsAstAusRenderObjectMemBlok[] ListeKandidaatAstEnthalte)
		{
			if (null == ListeKandidaatAstEnthalte)
			{
				return;
			}

			var ListeAstEnthalte = new List<SictGbsAstAusRenderObjectMemBlok>();

			foreach (var KandidaatFensterEnthalte in ListeKandidaatAstEnthalte)
			{
				if (KandidaatFensterEnthalte == this)
				{
					continue;
				}

				if (KandidaatFensterEnthalte.AstEnthaltendMemBlokBeginAdrese == this.Adrese)
				{
					ListeAstEnthalte.Add(KandidaatFensterEnthalte);
				}
			}

			var VorherMengeKandidaatAstEnthalte = this.MengeKandidaatAstEnthalte;

			this.MengeKandidaatAstEnthalte =
				(VorherMengeKandidaatAstEnthalte ?? new SictGbsAstAusRenderObjectMemBlok[0])
				.Concat(ListeAstEnthalte).ToArray();
		}

		public Int64 ReferenzAstEnthaltend
		{
			get
			{
				return BitConverter.ToUInt32(ListeOktet, 20);
			}
		}

		public Int64 ReferenzStringTyp
		{
			get
			{
				return BitConverter.ToUInt32(ListeOktet, 44);
			}
		}

		public string StringTyp;

		public byte? OktetSictbarkaitWert
		{
			get
			{
				var ListeOktet = this.ListeOktet;

				if (ListeOktet.Length <= OktetSictbarkaitAdrese)
				{
					return null;
				}

				return ListeOktet[OktetSictbarkaitAdrese];
			}
		}

		public Int64 AstEnthaltendMemBlokBeginAdrese
		{
			get
			{
				return ReferenzAstEnthaltend - 8;
			}
		}

		public Int64? PyObject0Adrese
		{
			get
			{
				var ListeOktet = this.ListeOktet;

				if (null == ListeOktet)
				{
					return null;
				}

				if (ListeOktet.Length < 8)
				{
					return null;
				}

				return BitConverter.ToUInt32(ListeOktet, 4);
			}
		}

		public Int64 ArrayListeAstEnthaltenAdrese
		{
			get
			{
				return BitConverter.ToUInt32(ListeOktet, 112);
			}
		}

		public float[] LaageUndGrööseListeSingle
		{
			get
			{
				var ListeOktet = this.ListeOktet;

				if (null == ListeOktet)
				{
					return null;
				}

				var ListeSingleAnzaal = Math.Min(4, (ListeOktet.Length - TestSingleDistanzVonBlokBegin) / 4);

				if (ListeSingleAnzaal < 4)
				{
					return null;
				}

				var LaageUndGrööseListeSingle =
					Enumerable.Range(0, ListeSingleAnzaal)
					.Select((Index) => BitConverter.ToSingle(ListeOktet, Index * 4 + TestSingleDistanzVonBlokBegin))
					.ToArray();

				return LaageUndGrööseListeSingle;
			}
		}

		public float[] Laage
		{
			get
			{
				var LaageUndGrööseListeSingle = this.LaageUndGrööseListeSingle;

				if (null == LaageUndGrööseListeSingle)
				{
					return null;
				}

				return LaageUndGrööseListeSingle.Take(2).ToArray();
			}
		}

		public float[] Grööse
		{
			get
			{
				var LaageUndGrööseListeSingle = this.LaageUndGrööseListeSingle;

				if (null == LaageUndGrööseListeSingle)
				{
					return null;
				}

				return LaageUndGrööseListeSingle.Skip(2).Take(2).ToArray();
			}
		}
	}

	public class SictProzesAuswertZuusctand
	{
		protected SictMesungZaitraumAusStopwatch InternDauer;

		public SictMesungZaitraumAusStopwatch Dauer
		{
			get
			{
				return InternDauer;
			}
		}

		public Dictionary<Int64, SictAuswertPythonObj> MengeFürHerkunftAdrPyObj = new Dictionary<Int64, SictAuswertPythonObj>();

		public void AusMengePyObjEntferneMitHerkunftAdrese(Int64 HerkunftAdrese)
		{
			AusMengePyObjEntferne((Kandidaat) => Kandidaat.HerkunftAdrese == HerkunftAdrese);
		}

		public void AusMengePyObjEntferne(Func<SictAuswertPythonObj, bool> Prädikat)
		{
			if (null == Prädikat)
			{
				return;
			}

			var MengeZuEntferne =
				MengeFürHerkunftAdrPyObj
				.Where((Kandidaat) => Prädikat(Kandidaat.Value))
				.ToArray();

			foreach (var ZuEntferne in MengeZuEntferne)
			{
				MengeFürHerkunftAdrPyObj.Remove(ZuEntferne.Key);
			}
		}

		public const string GbsPyChildrenListSclüselOwnerRefString = "_ownerRef";
		public const string PyChildrenListSclüselChildrenObjectsString = "_childrenObjects";
		public const string GbsAstSclüselParentRefString = "_parentRef";
		public const string GbsAstSclüselChildrenString = "children";
		public const string GbsAstSclüselRenderObjectString = "renderObject";
		public const string GbsAstSclüselNameString = "_name";

		/// <summary>
		/// 2015.08.26
		/// Beobactung in Type ShipHudSpriteGauge (verwandt jewails für Shield, Armor, Struct)
		/// </summary>
		public const string GbsAstSclüselLastValueString = "_lastValue";

		public const string GbsAstSclüselLastStateString = "lastState";
		public const string GbsAstSclüselRotationString = "_rotation";
		public const string GbsAstSclüselTextureString = "_texture";
		public const string GbsAstSclüselColorString = "_color";

		/*
		 * 2014.03.00
		 * Eve Online Client "Version: 8.43.753967" ("Released on Tuesday, April 1st, 2014") introduced a change in Memory Layout that breaks old Version of Bot
		 * 
		 * "hint"	-> "_hint"
		 * 
		public const string GbsAstSclüselHintString = "hint";
		 * */
		public const string GbsAstSclüselHintString = "_hint";
		public const string GbsAstSclüsel_SrString = "_sr";
		public const string GbsAstSclüselLastRenderDataString = "_lastRenderData";
		public const string GbsAstLabelSclüselTextString = "_text";
		public const string GbsAstLabelSclüselLinkTextString = "linkText";
		public const string GbsAstLabelSclüselSetTextString = "_setText";
		public const string GbsAstWindowSclüselIsModalString = "isModal";

		/// <summary>
		/// 2015.08.19
		/// in MarketOrder aus 2015.08.19 RegionalMarket.Quickbar.Sellers.Buyers+Modify.Order+Sell+Buy
		/// </summary>
		public const string GbsAstWindowSclüselBackgroundListString = "_backgroundlist";

		/// <summary>
		/// 2015.07.09 werd z.B. in Type "OverviewScrollEntry" verwandt.
		/// </summary>
		public const string OverviewScrollEntrySclüselIsSelectedString = "_isSelected";
		/// <summary>
		/// 2015.07.09 werd z.B. in Type "TreeViewEntryInventory" verwandt.
		/// </summary>
		public const string TreeViewEntrySclüselIsSelectedString = "isSelected";
		public const string GbsAstWindowSclüselCaptionString = "_caption";
		public const string GbsAstWindowSclüselMinimizedString = "_minimized";
		public const string GbsAstWindowSclüselIsDialogString = "isDialog";
		public const string GbsAstWindowSclüselWindowIdString = "windowID"; //	str
		public const string GbsAstWindowSclüselPinnedString = "_pinned";
		public const string GbsAstWindowSclüselTexturePathString = "texturePath";
		/// <summary>
		/// werd z.B. von TreeViewEntry in Inventory verwendet.
		/// </summary>
		public const string GbsAstSrBunchSclüselHtmlstrString = "htmlstr";
		public const string GbsAstSrBunchSclüselNodeString = "node";

		public const string GbsAstShipUISclüselShieldFülsctandString = "lastShield";
		public const string GbsAstShipUISclüselArmorFülsctandString = "lastArmor";
		public const string GbsAstShipUISclüselStructureFülsctandString = "lastStructure";
		public const string GbsAstShipUISclüselCapacitorFülsctandString = "lastsetcapacitor";
		public const string GbsAstShipUISclüselSpeedString = "lastSpeed";

		static readonly public string[] GbsListeRenderObjectTyp = {
																			  "trinity.Tr2Sprite2dScene",   "trinity.Tr2Sprite2dContainer", "trinity.Tr2Sprite2dTransform",
																			  "EveLabelSmall",  "EveLabelMedium",   "EveLabelLarge"};

		public SictAuswertPythonObjStr PyObjStrGbsAstEntryChildren;

		public SictAuswertPythonObjType[] MengePyObjTyp
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypType
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypWeakref
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypStr
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypLong
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypInstance
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypInt
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypBool
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypFloat
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypList
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypDict
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypTuple
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypBunch
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypUnicode
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypPyChildrenList
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypBackgroundList
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypUIChildrenListAutoSize
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypUIRoot
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypPyColor
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypTrinityTr2Sprite2dTexture
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjType PyObjTypTr2GlyphString
		{
			protected set;
			get;
		}

		public SictAuswertPythonObjPyOderUiChildrenList[] MengePyObjPyChildrenList
		{
			protected set;
			get;
		}

		public SictAuswertPyObjGbsAst[] AusPyChildrenListMengeGbsAst
		{
			protected set;
			get;
		}

		public SictAuswertPyObjGbsAst[] GbsMengeWurzelObj
		{
			protected set;
			get;
		}

		public SictAuswertPyObjGbsAst[] MengeGbsAst
		{
			protected set;
			get;
		}

		public void MengePyObjTypSezeAusMengeFürHerkunftAdrPyObj()
		{
			SictAuswertPythonObjType[] MengePyObjTyp = null;

			try
			{
				var MengeFürHerkunftAdrPyObj = this.MengeFürHerkunftAdrPyObj;

				if (null == MengeFürHerkunftAdrPyObj)
				{
					return;
				}

				MengePyObjTyp =
					MengeFürHerkunftAdrPyObj
					.Select((FürHerkunftAdrPyObj) => FürHerkunftAdrPyObj.Value)
					.OfType<SictAuswertPythonObjType>()
					.OrderBy((Kandidaat) => Kandidaat.tp_name)
					.ToArray();
			}
			finally
			{
				this.MengePyObjTyp = MengePyObjTyp;
			}
		}

		public void FüleRefTypScpezVonMengePyObjType()
		{
			MengePyObjTypSezeAusMengeFürHerkunftAdrPyObj();

			var MengePyObjTyp = this.MengePyObjTyp;

			if (null == MengePyObjTyp)
			{
				MengePyObjTyp = new SictAuswertPythonObjType[0];
			}

			PyObjTypType = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("type", Kandidaat.tp_name));

			PyObjTypWeakref = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("weakref", Kandidaat.tp_name));

			PyObjTypInt = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("int", Kandidaat.tp_name));

			PyObjTypBool = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("bool", Kandidaat.tp_name));

			PyObjTypFloat = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("float", Kandidaat.tp_name));

			PyObjTypStr = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("str", Kandidaat.tp_name));

			PyObjTypLong = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("long", Kandidaat.tp_name));

			PyObjTypInstance = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("instance", Kandidaat.tp_name));

			PyObjTypList = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("list", Kandidaat.tp_name));

			PyObjTypDict = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("dict", Kandidaat.tp_name));

			PyObjTypTuple = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("tuple", Kandidaat.tp_name));

			PyObjTypBunch = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("Bunch", Kandidaat.tp_name));

			PyObjTypUnicode = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("unicode", Kandidaat.tp_name));

			PyObjTypPyChildrenList = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("PyChildrenList", Kandidaat.tp_name));

			PyObjTypBackgroundList = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("BackgroundList", Kandidaat.tp_name));

			PyObjTypUIChildrenListAutoSize = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("UIChildrenListAutoSize", Kandidaat.tp_name));

			PyObjTypUIRoot = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("UIRoot", Kandidaat.tp_name));

			PyObjTypPyColor = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("PyColor", Kandidaat.tp_name));

			PyObjTypTrinityTr2Sprite2dTexture = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("trinity.Tr2Sprite2dTexture", Kandidaat.tp_name));

			PyObjTypTr2GlyphString = MengePyObjTyp.FirstOrDefault((Kandidaat) => string.Equals("Tr2GlyphString", Kandidaat.tp_name));
		}

		public void KopiireVon(SictProzesAuswertZuusctand ZuKopiire)
		{
			if (null == ZuKopiire)
			{
				return;
			}

			MengeFürHerkunftAdrPyObj = new Dictionary<Int64, SictAuswertPythonObj>(ZuKopiire.MengeFürHerkunftAdrPyObj);

			MengePyObjTyp = MengeFürHerkunftAdrPyObj.Select((HerkunftAdrUndPyObj) => HerkunftAdrUndPyObj.Value).OfType<SictAuswertPythonObjType>().ToArray();

			FüleRefTypScpezVonMengePyObjType();

			MengePyObjPyChildrenList = ZuKopiire.MengePyObjPyChildrenList;

			GbsMengeWurzelObj = ZuKopiire.GbsMengeWurzelObj;
		}

		public SictAuswertPythonDictEntryAinfac InPyBunchSuuceEntryFürKeyString(
			SictAuswertPythonObjBunch PyBunch,
			string Key,
			IMemoryReader ProzesLeeser,
			bool ScraibePyObjNaacScpaicer,
			bool ErmitleTypNurAusScpaicer = false,
			int? ListeEntryAnzaalScrankeMax = null)
		{
			if (null == PyBunch)
			{
				return null;
			}

			if (null == Key)
			{
				return null;
			}

			var ListeDictEntry = PyBunch.ListeEntry;

			if (null == ListeDictEntry)
			{
				PyBunch.LaadeReferenziirte(ProzesLeeser, ListeEntryAnzaalScrankeMax);

				ListeDictEntry = PyBunch.ListeEntry;
			}

			return InListePyDictEntrySuuceEntryFürKeyString(
				ListeDictEntry,
				Key,
				ProzesLeeser,
				ScraibePyObjNaacScpaicer,
				ErmitleTypNurAusScpaicer);
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

		static Dictionary<Type, KeyValuePair<string, Fasterflect.MemberSetter>[]>
			DictZuTypeMengeZuDictKeyFieldSetter =
			new Dictionary<Type, KeyValuePair<string, Fasterflect.MemberSetter>[]>();

		static public KeyValuePair<string, Fasterflect.MemberSetter>[] ZuTypeMengeZuDictKeyFieldSetterBerecne(Type Type)
		{
			if (null == Type)
			{
				return null;
			}

			var ListeZuDictEntryKeySetter = new List<KeyValuePair<string, Fasterflect.MemberSetter>>();

			var MengeField = Type.GetFields();

			foreach (var Field in MengeField)
			{
				var FieldMengeAttribut = Field.GetCustomAttributes(typeof(SictInPyDictEntryKeyAttribut), true);

				if (null == FieldMengeAttribut)
				{
					continue;
				}

				foreach (var FieldAttribut in FieldMengeAttribut)
				{
					var FieldAttributAlsDictEntryKey = FieldAttribut as SictInPyDictEntryKeyAttribut;

					if (null == FieldAttributAlsDictEntryKey)
					{
						continue;
					}

					var DictEntryKeyString = FieldAttributAlsDictEntryKey.DictEntryKeyString;

					var Setter = Type.DelegateForSetFieldValue(Field.Name);

					ListeZuDictEntryKeySetter.Add(new KeyValuePair<string, Fasterflect.MemberSetter>(DictEntryKeyString, Setter));
				}
			}

			return ListeZuDictEntryKeySetter.ToArray();
		}

		public class SictSuuceMengeDictEntryScpezGbsAstErgeebnis
		{
			[SictInPyDictEntryKeyAttribut(GbsAstSclüselParentRefString)]
			public SictAuswertPythonDictEntryAinfac DictEntryParentRef;

			[SictInPyDictEntryKeyAttribut(GbsAstSclüselChildrenString)]
			public SictAuswertPythonDictEntryAinfac DictEntryChildren;

			[SictInPyDictEntryKeyAttribut(GbsAstSclüselRenderObjectString)]
			public SictAuswertPythonDictEntryAinfac DictEntryRenderObject;

			[SictInPyDictEntryKeyAttribut(GbsAstSclüselNameString)]
			public SictAuswertPythonDictEntryAinfac DictEntryName;

			[SictInPyDictEntryKeyAttribut(GbsAstSclüselLastStateString)]
			public SictAuswertPythonDictEntryAinfac DictEntryLastState;

			[SictInPyDictEntryKeyAttribut(GbsAstSclüselRotationString)]
			public SictAuswertPythonDictEntryAinfac DictEntryRotation;

			[SictInPyDictEntryKeyAttribut(GbsAstSclüselTextureString)]
			public SictAuswertPythonDictEntryAinfac DictEntryTexture;

			[SictInPyDictEntryKeyAttribut(GbsAstSclüselColorString)]
			public SictAuswertPythonDictEntryAinfac DictEntryColor;

			[SictInPyDictEntryKeyAttribut(GbsAstSclüselHintString)]
			public SictAuswertPythonDictEntryAinfac DictEntryHint;

			[SictInPyDictEntryKeyAttribut(GbsAstSclüsel_SrString)]
			public SictAuswertPythonDictEntryAinfac DictEntry_Sr;

			[SictInPyDictEntryKeyAttribut(GbsAstLabelSclüselTextString)]
			public SictAuswertPythonDictEntryAinfac DictEntryText;

			[SictInPyDictEntryKeyAttribut(GbsAstLabelSclüselLinkTextString)]
			public SictAuswertPythonDictEntryAinfac DictEntryLinkText;

			[SictInPyDictEntryKeyAttribut(GbsAstLabelSclüselSetTextString)]
			public SictAuswertPythonDictEntryAinfac DictEntrySetText;

			[SictInPyDictEntryKeyAttribut(GbsAstWindowSclüselIsModalString)]
			public SictAuswertPythonDictEntryAinfac DictEntryIsModal;

			[SictInPyDictEntryKeyAttribut(GbsAstWindowSclüselCaptionString)]
			public SictAuswertPythonDictEntryAinfac DictEntryCaption;

			[SictInPyDictEntryKeyAttribut(GbsAstWindowSclüselMinimizedString)]
			public SictAuswertPythonDictEntryAinfac DictEntryMinimized;

			[SictInPyDictEntryKeyAttribut(GbsAstWindowSclüselIsDialogString)]
			public SictAuswertPythonDictEntryAinfac DictEntryIsDialog;

			[SictInPyDictEntryKeyAttribut(GbsAstWindowSclüselWindowIdString)]
			public SictAuswertPythonDictEntryAinfac DictEntryWindowId;

			[SictInPyDictEntryKeyAttribut(GbsAstWindowSclüselPinnedString)]
			public SictAuswertPythonDictEntryAinfac DictEntryPinned;
		}

		/// <summary>
		/// 2013.09.23
		/// Versuuc reduktioon Reaktioonszait durc optimiirung für Scpeziaalfal Suuce Menge Entry für Gbs Ast
		/// </summary>
		/// <param name="PyDict"></param>
		/// <param name="ProzesLeeser"></param>
		/// <param name="ScraibePyObjNaacScpaicer"></param>
		/// <param name="ErmitleTypNurAusScpaicer"></param>
		/// <param name="ListeEntryAnzaalScrankeMax"></param>
		/// <returns></returns>
		public SictSuuceMengeDictEntryScpezGbsAstErgeebnis
			InDictMengeEntryScpezGbsAstBerecne(
			SictAuswertPythonObjDict PyDict,
			IMemoryReader ProzesLeeser,
			bool ScraibePyObjNaacScpaicer,
			bool ErmitleTypNurAusScpaicer = false,
			int? ListeEntryAnzaalScrankeMax = null)
		{
			var ListeDictEntry = PyDict.ListeDictEntry;

			if (null == ListeDictEntry)
			{
				PyDict.LaadeReferenziirte(ProzesLeeser, ListeEntryAnzaalScrankeMax);

				ListeDictEntry = PyDict.ListeDictEntry;
			}

			if (null == ListeDictEntry)
			{
				return null;
			}

			return InMengeDictEntryMengeEntryScpezGbsAstBerecne(
				ListeDictEntry,
				ProzesLeeser,
				ScraibePyObjNaacScpaicer,
				ErmitleTypNurAusScpaicer);
		}

		public object
			InMengeDictEntryMengeEntryScpezFürZiilObjektTypeBerecne(
			object ZiilObjekt,
			SictAuswertPythonDictEntryAinfac[] ListePyDictEntry,
			IMemoryReader ProzesLeeser,
			bool ScraibePyObjNaacScpaicer,
			bool ErmitleTypNurAusScpaicer = false)
		{
			if (null == ListePyDictEntry)
			{
				return null;
			}

			if (null == ZiilObjekt)
			{
				return null;
			}

			var ZiilObjektType = ZiilObjekt.GetType();

			//	var MengeZuDictEntryKeySetter = Optimat.Glob.TAD(DictZuTypeMengeZuDictKeyFieldSetter, ZiilObjektType);
			var MengeZuDictEntryKeySetter = DictZuTypeMengeZuDictKeyFieldSetter.TryGetValueOrDefault(ZiilObjektType);

			if (null == MengeZuDictEntryKeySetter)
			{
				MengeZuDictEntryKeySetter = ZuTypeMengeZuDictKeyFieldSetterBerecne(ZiilObjektType);

				DictZuTypeMengeZuDictKeyFieldSetter[ZiilObjektType] = MengeZuDictEntryKeySetter;
			}

			if (null == DictZuTypeMengeZuDictKeyFieldSetter)
			{
				return null;
			}

			for (int EntryIndex = 0; EntryIndex < ListePyDictEntry.Length; EntryIndex++)
			{
				var Entry = ListePyDictEntry[EntryIndex];

				if (null == Entry)
				{
					continue;
				}

				var EntryReferenzKey = Entry.ReferenzKey;

				if (0 == EntryReferenzKey)
				{
					continue;
				}

				var EntryKey = Entry.Key;

				if (null == EntryKey)
				{
					EntryKey = PyObjFürHerkunftAdreseAusScpaicerOderErsctele(EntryReferenzKey, ProzesLeeser, ErmitleTypNurAusScpaicer);

					if (ScraibePyObjNaacScpaicer)
					{
						PyObjSezeNaacScpaicer(EntryKey);
					}

					Entry.Key = EntryKey;
				}

				var EntryKeyAlsStr = EntryKey as SictAuswertPythonObjStr;

				if (null == EntryKeyAlsStr)
				{
					continue;
				}

				var EntryKeyStr = EntryKeyAlsStr.String;

				if (null == EntryKeyStr)
				{
					continue;
				}

				foreach (var ZuDictKeyFieldSetter in MengeZuDictEntryKeySetter)
				{
					if (!string.Equals(EntryKeyStr, ZuDictKeyFieldSetter.Key))
					{
						continue;
					}

					ZuDictKeyFieldSetter.Value(ZiilObjekt, Entry);
				}
			}

			return ZiilObjekt;
		}

		public SictSuuceMengeDictEntryScpezGbsAstErgeebnis
			InMengeDictEntryMengeEntryScpezGbsAstBerecne(
			SictAuswertPythonDictEntryAinfac[] ListePyDictEntry,
			IMemoryReader ProzesLeeser,
			bool ScraibePyObjNaacScpaicer,
			bool ErmitleTypNurAusScpaicer = false)
		{
			if (null == ListePyDictEntry)
			{
				return null;
			}

			var Ergeebnis = new SictSuuceMengeDictEntryScpezGbsAstErgeebnis();

			Ergeebnis =
				InMengeDictEntryMengeEntryScpezFürZiilObjektTypeBerecne(
				Ergeebnis,
				ListePyDictEntry,
				ProzesLeeser,
				ScraibePyObjNaacScpaicer,
				ErmitleTypNurAusScpaicer)
				as SictSuuceMengeDictEntryScpezGbsAstErgeebnis;

			/*
			 * 
			for (int EntryIndex = 0; EntryIndex < ListePyDictEntry.Length; EntryIndex++)
			{
				var Entry = ListePyDictEntry[EntryIndex];

				if (null == Entry)
				{
					continue;
				}

				var EntryReferenzKey = Entry.ReferenzKey;

				if (0 == EntryReferenzKey)
				{
					continue;
				}

				var EntryKey = Entry.Key;

				if (null == EntryKey)
				{
					EntryKey = PyObjFürHerkunftAdreseAusScpaicerOderErsctele(EntryReferenzKey, ProzesLeeser, ErmitleTypNurAusScpaicer);

					if (ScraibePyObjNaacScpaicer)
					{
						PyObjSezeNaacScpaicer(EntryKey);
					}

					Entry.Key = EntryKey;
				}

				var EntryKeyAlsStr = EntryKey as SictAuswertPythonObjStr;

				if (null == EntryKeyAlsStr)
				{
					continue;
				}

				var EntryKeyStr = EntryKeyAlsStr.String;

				if (null == EntryKeyStr)
				{
					continue;
				}

				var EntryKeyStrZaicen0 = EntryKeyStr.ElementAtOrDefault(0);

				if (EntryKeyStrZaicen0 == '_')
				{
					if (string.Equals(GbsAstSclüselParentRefString, EntryKeyStr))
					{
						Ergeebnis.DictEntryParentRef = Entry;
					}
				}
				else
				{
					if (string.Equals(GbsAstSclüselChildrenString, EntryKeyStr))
					{
						Ergeebnis.DictEntryChildren = Entry;
					}

					if (string.Equals(GbsAstSclüselRenderObjectString, EntryKeyStr))
					{
						Ergeebnis.DictEntryRenderObject = Entry;
					}
				}
			}

			 * */

			return Ergeebnis;
		}

		public SictAuswertPythonDictEntryAinfac InPyDictSuuceEntryFürKeyString(
			SictAuswertPythonObjDict PyDict,
			string Key,
			IMemoryReader ProzesLeeser,
			bool ScraibePyObjNaacScpaicer,
			bool ErmitleTypNurAusScpaicer = false,
			int? ListeEntryAnzaalScrankeMax = null)
		{
			if (null == PyDict)
			{
				return null;
			}

			if (null == Key)
			{
				return null;
			}

			var ListeDictEntry = PyDict.ListeDictEntry;

			if (null == ListeDictEntry)
			{
				PyDict.LaadeReferenziirte(ProzesLeeser, ListeEntryAnzaalScrankeMax);

				ListeDictEntry = PyDict.ListeDictEntry;
			}

			return InListePyDictEntrySuuceEntryFürKeyString(
				ListeDictEntry,
				Key,
				ProzesLeeser,
				ScraibePyObjNaacScpaicer,
				ErmitleTypNurAusScpaicer);
		}

		static public bool KeyStringEqualSictbarkaitProfiler(string str0, string str1)
		{
			return string.Equals(str0, str1);
		}

		/// <summary>
		/// Fals Key nict unter beraits aingetraagene Key gefunde werd, werdn Key aus ProzesLeeser ersctelt.
		/// </summary>
		/// <param name="PyDict"></param>
		/// <param name="Key"></param>
		/// <param name="ProzesLeeser"></param>
		/// <returns></returns>
		public SictAuswertPythonDictEntryAinfac InListePyDictEntrySuuceEntryFürKeyString(
			SictAuswertPythonDictEntryAinfac[] ListePyDictEntry,
			string Key,
			IMemoryReader ProzesLeeser,
			bool ScraibePyObjNaacScpaicer,
			bool ErmitleTypNurAusScpaicer = false)
		{
			if (null == ListePyDictEntry)
			{
				return null;
			}

			if (null == Key)
			{
				return null;
			}

			int ListeEntryNocZuLaadeFrüühesteIndex = -1;
			int ListeEntryNocZuLaadeLezteIndex = -1;

			//	zuersct wern beraits vorhandene Key durcsuuct

			for (int EntryIndex = 0; EntryIndex < ListePyDictEntry.Length; EntryIndex++)
			{
				var Entry = ListePyDictEntry[EntryIndex];

				if (null == Entry)
				{
					continue;
				}

				var EntryReferenzKey = Entry.ReferenzKey;

				if (0 == EntryReferenzKey)
				{
					continue;
				}

				var EntryKey = Entry.Key;

				if (null == EntryKey)
				{
					/*
					 * 2013.07.18
					 * Optimiirung durcsaz:
					 * Für nocmaaliges durclaufe von Sclaife werd in früherem Durclauf gespaicert bai welce Index noc Key zu laade sin.
					 * */
					ListeEntryNocZuLaadeLezteIndex = EntryIndex;

					if (ListeEntryNocZuLaadeFrüühesteIndex < 0)
					{
						ListeEntryNocZuLaadeFrüühesteIndex = EntryIndex;
					}

					continue;
				}

				var EntryKeyAlsStr = EntryKey as SictAuswertPythonObjStr;

				if (null == EntryKeyAlsStr)
				{
					continue;
				}

				/*
				 * 2013.09.22
				 * Zuorndung recenzait in Profiler sictbar mace durc Funktioonsaufruuf
				 * 
				if (string.Equals(Key, EntryKeyAlsStr.String))
				 * */
				if (KeyStringEqualSictbarkaitProfiler(Key, EntryKeyAlsStr.String))
				{
					return Entry;
				}
			}

			if (0 <= ListeEntryNocZuLaadeFrüühesteIndex)
			{
				//	Fals Entry noc nit gefunde wurde, werd sicergesctelt das ale Entry gelaade
				for (int EntryIndex = ListeEntryNocZuLaadeFrüühesteIndex; EntryIndex <= ListeEntryNocZuLaadeLezteIndex; EntryIndex++)
				{
					var Entry = ListePyDictEntry[EntryIndex];

					var EntryReferenzKey = Entry.ReferenzKey;

					if (0 == EntryReferenzKey)
					{
						continue;
					}

					var EntryKey = Entry.Key;

					if (null == EntryKey)
					{
						EntryKey = PyObjFürHerkunftAdreseAusScpaicerOderErsctele(EntryReferenzKey, ProzesLeeser, ErmitleTypNurAusScpaicer);

						if (ScraibePyObjNaacScpaicer)
						{
							PyObjSezeNaacScpaicer(EntryKey);
						}

						Entry.Key = EntryKey;
					}

					var EntryKeyAlsStr = EntryKey as SictAuswertPythonObjStr;

					if (null == EntryKeyAlsStr)
					{
						continue;
					}

					if (string.Equals(Key, EntryKeyAlsStr.String))
					{
						return Entry;
					}
				}
			}

			return null;
		}

		public void PyObjDictEntryFüleAusScpaicer(SictAuswertPythonDictEntry Entry)
		{
			if (null == Entry)
			{
				return;
			}

			SictAuswertPythonObj Key, Value;

			MengeFürHerkunftAdrPyObj.TryGetValue(Entry.ReferenzKey, out Key);
			MengeFürHerkunftAdrPyObj.TryGetValue(Entry.ReferenzValue, out Value);

			Entry.Key = Key;
			Entry.Value = Value;
		}

		public void PyObjDictEntryFüleAusScpaicer(SictAuswertPythonDictEntryAinfac Entry)
		{
			if (null == Entry)
			{
				return;
			}

			SictAuswertPythonObj Key, Value;

			MengeFürHerkunftAdrPyObj.TryGetValue(Entry.ReferenzKey, out Key);
			MengeFürHerkunftAdrPyObj.TryGetValue(Entry.ReferenzValue, out Value);

			Entry.Key = Key;
			Entry.Value = Value;
		}

		public void PyObjDictEntryFüleAusScpaicerOderProzes(
			SictAuswertPythonDictEntry Entry,
			IMemoryReader ProzesLeeser,
			bool ObjSezeNaacScpaicer = false,
			bool ErmitleTypNurAusScpaicer = true)
		{
			if (null == Entry)
			{
				return;
			}

			PyObjDictEntryFüleAusScpaicer(Entry);

			if (null == ProzesLeeser)
			{
				return;
			}

			if (null == Entry.Key)
			{
				Entry.Key = PyObjFürHerkunftAdreseAusScpaicerOderErsctele(Entry.ReferenzKey, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			}

			if (null == Entry.Value)
			{
				Entry.Value = PyObjFürHerkunftAdreseAusScpaicerOderErsctele(Entry.ReferenzValue, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			}
		}

		public void PyObjDictEntryFüleAusScpaicerOderProzes(
			SictAuswertPythonDictEntryAinfac Entry,
			IMemoryReader ProzesLeeser,
			bool ObjSezeNaacScpaicer = false,
			bool ErmitleTypNurAusScpaicer = false)
		{
			if (null == Entry)
			{
				return;
			}

			PyObjDictEntryFüleAusScpaicer(Entry);

			if (null == ProzesLeeser)
			{
				return;
			}

			if (null == Entry.Key)
			{
				Entry.Key = PyObjFürHerkunftAdreseAusScpaicerOderErsctele(Entry.ReferenzKey, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			}

			if (null == Entry.Value)
			{
				Entry.Value = PyObjFürHerkunftAdreseAusScpaicerOderErsctele(Entry.ReferenzValue, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			}
		}

		public SictAuswertPythonObj PyObjFürHerkunftAdreseAusScpaicer(
			Int64 PyObjHerkunftAdr)
		{
			SictAuswertPythonObj PyObj;

			MengeFürHerkunftAdrPyObj.TryGetValue(PyObjHerkunftAdr, out PyObj);

			return PyObj;
		}

		public SictAuswertPythonObj PyObjFürHerkunftAdreseAusScpaicerOderErsctele(
			Int64 PyObjHerkunftAdr,
			IMemoryReader ProzesLeeser,
			bool ObjSezeNaacScpaicer = false,
			bool ErmitleTypNurAusScpaicer = false,
			int? RekursScranke = null)
		{
			return PyObjFürHerkunftAdreseAusScpaicerOderErsctele(
				null,
				PyObjHerkunftAdr,
				ProzesLeeser,
				ObjSezeNaacScpaicer,
				ErmitleTypNurAusScpaicer,
				RekursScranke);
		}

		public Typ PyObjFürHerkunftAdreseAusScpaicerOderErsctele<Typ>(
			Int64 PyObjHerkunftAdr,
			IMemoryReader ProzesLeeser,
			bool ObjSezeNaacScpaicer = false,
			bool ErmitleTypNurAusScpaicer = false,
			int? RekursScranke = null)
			where Typ : SictAuswertPythonObj
		{
			var ObjAssemblyTyp = typeof(Typ);

			var ObjGen =
				PyObjFürHerkunftAdreseAusScpaicerOderErsctele(
				ObjAssemblyTyp,
				PyObjHerkunftAdr,
				ProzesLeeser,
				ObjSezeNaacScpaicer,
				ErmitleTypNurAusScpaicer,
				RekursScranke);

			var ObjScpez = ObjGen as Typ;

			return ObjScpez;
		}

		public SictAuswertPythonObj PyObjFürHerkunftAdreseAusScpaicerOderErsctele(
			Type ObjAssemblyTyp,
			Int64 PyObjHerkunftAdr,
			IMemoryReader ProzesLeeser,
			bool ObjSezeNaacScpaicer = false,
			bool ErmitleTypNurAusScpaicer = false,
			int? RekursScranke = null)
		{
			var PyObj = PyObjFürHerkunftAdreseAusScpaicer(PyObjHerkunftAdr);

			if (null != PyObj)
			{
				return PyObj;
			}

			PyObj = PyObjFürHerkunftAdreseErsctele(ObjAssemblyTyp, PyObjHerkunftAdr, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer, RekursScranke);

			if (ObjSezeNaacScpaicer && null != PyObj)
			{
				PyObjSezeNaacScpaicer(PyObj);
			}

			return PyObj;
		}

		public Type GibAssemblyTypFürPythonTypeMitHerkunftAdrese(Int64 HerkunftAdrese)
		{
			var PyObjType = PyObjFürHerkunftAdreseAusScpaicer(HerkunftAdrese) as SictAuswertPythonObjType;

			return GibAssemblyTypFürPythonType(PyObjType);
		}

		public Type GibAssemblyTypFürPythonType(SictAuswertPythonObjType PyObjType)
		{
			var AssemblyTyp = typeof(SictAuswertPythonObj);

			if (null == PyObjType)
			{
				return AssemblyTyp;
			}

			if (PyObjType == PyObjTypType)
			{
				AssemblyTyp = typeof(SictAuswertPythonObjType);
			}

			if (PyObjType == PyObjTypWeakref)
			{
				AssemblyTyp = typeof(SictAuswertPythonObjWeakRef);
			}

			if (PyObjType == PyObjTypDict)
			{
				AssemblyTyp = typeof(SictAuswertPythonObjDict);
			}

			if (PyObjTypList == PyObjType)
			{
				AssemblyTyp = typeof(SictAuswertPythonObjList);
			}

			if (PyObjTypTuple == PyObjType)
			{
				AssemblyTyp = typeof(SictAuswertPythonObjTuple);
			}

			if (PyObjTypBunch == PyObjType)
			{
				AssemblyTyp = typeof(SictAuswertPythonObjBunch);
			}

			if (PyObjTypUnicode == PyObjType)
			{
				AssemblyTyp = typeof(SictAuswertPythonObjUnicode);
			}

			if (PyObjTypInt == PyObjType)
			{
				AssemblyTyp = typeof(SictAuswertPythonObjInt);
			}

			if (PyObjTypBool == PyObjType)
			{
				AssemblyTyp = typeof(SictAuswertPythonObjBool);
			}

			if (PyObjTypFloat == PyObjType)
			{
				AssemblyTyp = typeof(SictAuswertPythonObjFloat);
			}

			if (PyObjTypStr == PyObjType)
			{
				AssemblyTyp = typeof(SictAuswertPythonObjStr);
			}

			if (PyObjTypLong == PyObjType)
			{
				AssemblyTyp = typeof(SictAuswertPythonObjLong);
			}

			if (PyObjTypInstance == PyObjType)
			{
				AssemblyTyp = typeof(SictAuswertPythonObjInstance);
			}

			if (PyObjTypPyChildrenList == PyObjType)
			{
				AssemblyTyp = typeof(SictAuswertPythonObjPyOderUiChildrenList);
			}

			if (PyObjTypUIChildrenListAutoSize == PyObjType)
			{
				AssemblyTyp = typeof(SictAuswertPythonObjPyOderUiChildrenList);
			}

			if (PyObjTypBackgroundList == PyObjType)
			{
				AssemblyTyp = typeof(SictAuswertPythonObjPyOderUiChildrenList);
			}

			if (PyObjTypUIRoot == PyObjType)
			{
				AssemblyTyp = typeof(SictAuswertPyObjGbsAst);
			}

			if (PyObjTypPyColor == PyObjType)
			{
				AssemblyTyp = typeof(SictAuswertPyObjPyColor);
			}

			if (PyObjTypTrinityTr2Sprite2dTexture == PyObjType)
			{
				AssemblyTyp = typeof(SictAuswertPythonObjTrinityTr2Sprite2dTexture);
			}

			if (PyObjTypTr2GlyphString == PyObjType)
			{
				AssemblyTyp = typeof(SictAuswertPythonObjTr2GlyphString);
			}

			return AssemblyTyp;
		}

		public SictAuswertPythonObj PyObjFürTypErsctele(
			SictAuswertPythonObjType PyObjType,
			Int64 HerkunftAdrese,
			byte[] ListeOktet,
			IMemoryReader ProzesLeeser)
		{
			var AssemblyTyp = GibAssemblyTypFürPythonType(PyObjType);

			var PyObj = PyObjFürTypErsctele(AssemblyTyp, HerkunftAdrese, ListeOktet, ProzesLeeser);

			return PyObj;
		}

		public Typ PyObjFürTypGenErsctele<Typ>(
			Int64 HerkunftAdrese,
			byte[] ListeOktet,
			IMemoryReader ProzesLeeser)
			where Typ : SictAuswertPythonObj
		{
			var AssemblyTyp = typeof(Typ);

			var PyObj = PyObjFürTypErsctele(AssemblyTyp, HerkunftAdrese, ListeOktet, ProzesLeeser);

			var PyObjScpez = PyObj as Typ;

			return PyObjScpez;
		}

		public SictAuswertPythonObj PyObjFürTypErsctele(
			Type AssemblyType,
			Int64 HerkunftAdrese,
			byte[] ListeOktet,
			IMemoryReader ProzesLeeser)
		{
			var Dauer = new SictMesungZaitraumAusStopwatch(true);

			var TailVorConstructDauer = new SictMesungZaitraumAusStopwatch(true);

			try
			{
				if (null == AssemblyType)
				{
					return null;
				}

				if (!typeof(SictAuswertPythonObj).IsAssignableFrom(AssemblyType))
				{
					return null;
				}

				var MengeConstructor = AssemblyType.GetConstructors();

				var MengeConstructorOrdnet =
					MengeConstructor
					.OrderByDescending((Kandidaat) => Kandidaat.GetParameters().Length)
					.ToArray();

				foreach (var Constructor in MengeConstructorOrdnet)
				{
					var ConstructorListeParam = Constructor.GetParameters();

					if (3 != ConstructorListeParam.Length)
					{
						continue;
					}

					var ParamHerkunftAdr = ConstructorListeParam[0];
					var ParamListeOktet = ConstructorListeParam[1];
					var ParamProzesLeeser = ConstructorListeParam[2];

					if (ParamHerkunftAdr.ParameterType != typeof(Int64))
					{
						continue;
					}

					if (ParamListeOktet.ParameterType != typeof(byte[]))
					{
						continue;
					}

					if (ParamProzesLeeser.ParameterType != typeof(IMemoryReader))
					{
						continue;
					}

					var ListeArgument = new object[] { HerkunftAdrese, ListeOktet, ProzesLeeser };

					TailVorConstructDauer.EndeSezeJezt();

					var Obj = Constructor.Invoke(ListeArgument);

					var PyObj = Obj as SictAuswertPythonObj;

					return PyObj;
				}

				return null;
			}
			finally
			{
				Dauer.EndeSezeJezt();
			}
		}

		public SictAuswertPythonObj PyObjFürHerkunftAdreseErsctele(
			Type ObjAssemblyTyp,
			Int64 PyObjHerkunftAdr,
			IMemoryReader ProzesLeeser,
			bool ObjSezeNaacScpaicer = false,
			bool ErmitleTypNurAusScpaicer = false,
			int? RekursScranke = null)
		{
			if (null == ProzesLeeser)
			{
				return null;
			}

			var PyObjListeOktet = ProzesLeeser.ListeOktetLeeseVonAdrese(PyObjHerkunftAdr, 0x100, false);

			SictAuswertPythonObjType PyObjTypScpez = null;

			if (null == ObjAssemblyTyp)
			{
				var ErmitlungTypPyObj = new SictAuswertPythonObj(PyObjHerkunftAdr, PyObjListeOktet, null);

				var PyObjTyp = PyObjFürHerkunftAdreseAusScpaicer(ErmitlungTypPyObj.RefType);

				if (null == PyObjTyp)
				{
					if (!ErmitleTypNurAusScpaicer)
					{
						if (!(RekursScranke < 1))
						{
							PyObjTyp = PyObjFürHerkunftAdreseAusScpaicerOderErsctele<SictAuswertPythonObjType>(
								ErmitlungTypPyObj.RefType, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer, (RekursScranke ?? 2) - 1);
						}
					}
				}

				PyObjTypScpez = PyObjTyp as SictAuswertPythonObjType;

				ObjAssemblyTyp = GibAssemblyTypFürPythonType(PyObjTypScpez);
			}

			var PyObj = PyObjFürTypErsctele(ObjAssemblyTyp, PyObjHerkunftAdr, PyObjListeOktet, ProzesLeeser);

			PyObj.ObjType = PyObjTypScpez;

			return PyObj;
		}

		public bool AssemblyTypHinraicendGenerelZuRefPyObjTyp(
			SictAuswertPythonObj PyObj)
		{
			if (null == PyObj)
			{
				return false;
			}

			var PyObjRefTyp = PyObj.RefType;

			var AssemblyTypMaxScpezialisiirt = GibAssemblyTypFürPythonTypeMitHerkunftAdrese(PyObjRefTyp);

			var HinraicendGenerel = AssemblyTypMaxScpezialisiirt.IsAssignableFrom(PyObj.GetType());

			return HinraicendGenerel;
		}

		public void LaadeType(
			SictAuswertPythonObj PyObj,
			IMemoryReader ProzesLeeser,
			bool ObjSezeNaacScpaicer = false,
			bool ErmitleTypNurAusScpaicer = true)
		{
			if (null == PyObj)
			{
				return;
			}

			var ObjType = PyObj.ObjType;
			var RefType = PyObj.RefType;

			if (null == ObjType && 0 != RefType)
			{
				PyObj.ObjType = ObjType =
					PyObjFürHerkunftAdreseAusScpaicerOderErsctele<SictAuswertPythonObjType>(
					RefType, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);
			}
		}

		public void LaadeReferenziirte(
			ISictAuswertPythonObjMitRefDict PyObj,
			IMemoryReader ProzesLeeser,
			bool ObjSezeNaacScpaicer = false,
			bool ErmitleTypNurAusScpaicer = true)
		{
			if (null == PyObj)
			{
				return;
			}

			var PyObjRefDict = PyObj.RefDict;

			if (null == PyObj.Dict)
			{
				if (0 != PyObjRefDict)
				{
					var KandidaatDict =
						PyObjFürHerkunftAdreseAusScpaicerOderErsctele<SictAuswertPythonObjDict>(
						PyObjRefDict,
						ProzesLeeser,
						ObjSezeNaacScpaicer,
						ErmitleTypNurAusScpaicer);

					if (AssemblyTypHinraicendGenerelZuRefPyObjTyp(KandidaatDict))
					{
						PyObj.Dict = KandidaatDict;
					}
				}
			}
		}

		public void LaadeReferenziirte(
			SictAuswertPyObjGbsAst PyObj,
			IMemoryReader ProzesLeeser,
			bool ObjSezeNaacScpaicer = false,
			bool ErmitleTypNurAusScpaicer = true,
			int? DictListeEntryAnzaalScrankeMax = 0x400)
		{
			if (null == PyObj)
			{
				return;
			}

			LaadeReferenziirte(PyObj as SictAuswertPythonObjMitRefDictBaiPlus8, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);

			var PyObjDict = PyObj.Dict;

			if (null == PyObjDict)
			{
				return;
			}

			var SuuceMengeEntryErgeebnis =
				InDictMengeEntryScpezGbsAstBerecne(
				PyObjDict,
				ProzesLeeser,
				true,
				ErmitleTypNurAusScpaicer,
				DictListeEntryAnzaalScrankeMax);

			if (null == SuuceMengeEntryErgeebnis)
			{
				return;
			}

			{
				var EntryParentRef = SuuceMengeEntryErgeebnis.DictEntryParentRef;

				if (null != EntryParentRef)
				{
					PyObjDictEntryFüleAusScpaicerOderProzes(
						EntryParentRef,
						ProzesLeeser,
						ObjSezeNaacScpaicer,
						ErmitleTypNurAusScpaicer);

					var WeakRefParentRef = EntryParentRef.Value as SictAuswertPythonObjWeakRef;

					if (null != WeakRefParentRef)
					{
						PyObj.AusDictParentRef = WeakRefParentRef.RefBaiOktet8;
					}
				}
			}

			{
				var EntryRenderObject = SuuceMengeEntryErgeebnis.DictEntryRenderObject;

				if (null != EntryRenderObject)
				{
					var RenderObjectRef = EntryRenderObject.ReferenzValue;

					PyObj.AusDictRenderObjectRef = RenderObjectRef;

					if (0 != RenderObjectRef)
					{
						PyObj.AusDictRenderObject =
							PyObjFürHerkunftAdreseAusScpaicerOderErsctele<SictAuswertPythonObjMitRefBaiPlus8>(
							RenderObjectRef,
							ProzesLeeser,
							ObjSezeNaacScpaicer,
							ErmitleTypNurAusScpaicer);
					}
				}
			}

			{
				var Entry = SuuceMengeEntryErgeebnis.DictEntryName;

				if (null != Entry)
				{
					PyObjDictEntryFüleAusScpaicerOderProzes(
						Entry,
						ProzesLeeser,
						ObjSezeNaacScpaicer,
						ErmitleTypNurAusScpaicer);

					PyObj.AusDictName = Entry.Value;

					var AusDictEntryValueAlsScpez = Entry.Value as SictAuswertPythonObjStr;

					if (null != AusDictEntryValueAlsScpez)
					{
						AusDictEntryValueAlsScpez.LaadeReferenziirte(ProzesLeeser);

						PyObj.AusDictNameString = AusDictEntryValueAlsScpez.String;
					}
				}
			}

			{
				var Entry = SuuceMengeEntryErgeebnis.DictEntryText;

				if (null != Entry)
				{
					PyObjDictEntryFüleAusScpaicerOderProzes(
						Entry,
						ProzesLeeser,
						ObjSezeNaacScpaicer,
						ErmitleTypNurAusScpaicer);

					PyObj.AusDictText = Entry.Value;

					var AusDictEntryValueAlsScpez = Entry.Value as SictAuswertPythonObjUnicode;

					if (null != AusDictEntryValueAlsScpez)
					{
						AusDictEntryValueAlsScpez.LaadeReferenziirte(ProzesLeeser);

						PyObj.AusDictTextString = AusDictEntryValueAlsScpez.String;
					}
				}
			}

			{
				var EntryLinkText = SuuceMengeEntryErgeebnis.DictEntryLinkText;

				if (null != EntryLinkText)
				{
					PyObjDictEntryFüleAusScpaicerOderProzes(
						EntryLinkText,
						ProzesLeeser,
						ObjSezeNaacScpaicer,
						ErmitleTypNurAusScpaicer);

					PyObj.AusDictLinkText = EntryLinkText.Value;

					var AlsUnicode = EntryLinkText.Value as SictAuswertPythonObjUnicode;

					if (null != AlsUnicode)
					{
						AlsUnicode.LaadeReferenziirte(ProzesLeeser);

						PyObj.AusDictLinkTextString = AlsUnicode.String;
					}
				}
			}

			{
				var EntrySetText = SuuceMengeEntryErgeebnis.DictEntrySetText;

				if (null != EntrySetText)
				{
					PyObjDictEntryFüleAusScpaicerOderProzes(
						EntrySetText,
						ProzesLeeser,
						ObjSezeNaacScpaicer,
						ErmitleTypNurAusScpaicer);

					PyObj.AusDictSetText = EntrySetText.Value;

					var AusDictSetTextAlsStr = EntrySetText.Value as SictAuswertPythonObjStr;

					if (null != AusDictSetTextAlsStr)
					{
						AusDictSetTextAlsStr.LaadeReferenziirte(ProzesLeeser);

						PyObj.AusDictSetTextString = AusDictSetTextAlsStr.String;
					}

					var AusDictSetTextAlsUnicode = EntrySetText.Value as SictAuswertPythonObjUnicode;

					if (null != AusDictSetTextAlsUnicode)
					{
						AusDictSetTextAlsUnicode.LaadeReferenziirte(ProzesLeeser);

						PyObj.AusDictSetTextString = AusDictSetTextAlsUnicode.String;
					}
				}
			}

			{
				var Entry = SuuceMengeEntryErgeebnis.DictEntryCaption;

				if (null != Entry)
				{
					PyObjDictEntryFüleAusScpaicerOderProzes(Entry, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);

					PyObj.AusDictCaption = Entry.Value;

					var AusDictValueAlsStr = Entry.Value as SictAuswertPythonObjStr;

					if (null != AusDictValueAlsStr)
					{
						AusDictValueAlsStr.LaadeReferenziirte(ProzesLeeser);

						PyObj.AusDictCaptionString = AusDictValueAlsStr.String;
					}

					var AusDictValueAlsUnicode = Entry.Value as SictAuswertPythonObjUnicode;

					if (null != AusDictValueAlsUnicode)
					{
						AusDictValueAlsUnicode.LaadeReferenziirte(ProzesLeeser);

						PyObj.AusDictCaptionString = AusDictValueAlsUnicode.String;
					}
				}
			}

			{
				var Entry = SuuceMengeEntryErgeebnis.DictEntryWindowId;

				if (null != Entry)
				{
					PyObjDictEntryFüleAusScpaicerOderProzes(Entry, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);

					PyObj.AusDictWindowID = Entry.Value;

					var AusDictValueAlsScpez = Entry.Value as SictAuswertPythonObjStr;

					if (null != AusDictValueAlsScpez)
					{
						//	AusDictValueAlsScpez.LaadeReferenziirte(ProzesLeeser);

						PyObj.AusDictWindowIDString = AusDictValueAlsScpez.String;
					}
				}
			}

			{
				var Entry = SuuceMengeEntryErgeebnis.DictEntryMinimized;

				if (null != Entry)
				{
					PyObjDictEntryFüleAusScpaicerOderProzes(Entry, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);

					PyObj.AusDictMinimized = Entry.Value;

					var AusDictValueAlsBool = Entry.Value as SictAuswertPythonObjBool;

					if (null != AusDictValueAlsBool)
					{
						PyObj.AusDictMinimizedBool = AusDictValueAlsBool.Bool;
					}

					var AusDictValueAlsInt = Entry.Value as SictAuswertPythonObjInt;

					if (null != AusDictValueAlsInt)
					{
						PyObj.AusDictMinimizedInt = AusDictValueAlsInt.Int;
					}
				}
			}

			{
				var Entry = SuuceMengeEntryErgeebnis.DictEntryIsModal;

				if (null != Entry)
				{
					PyObjDictEntryFüleAusScpaicerOderProzes(Entry, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);

					PyObj.AusDictIsModal = Entry.Value;

					var AusDictValueAlsScpez = Entry.Value as SictAuswertPythonObjBool;

					if (null != AusDictValueAlsScpez)
					{
						//	AusDictValueAlsScpez.LaadeReferenziirte(ProzesLeeser);

						PyObj.AusDictIsModalBool = AusDictValueAlsScpez.Bool;
					}
				}
			}

			{
				var Entry = SuuceMengeEntryErgeebnis.DictEntryLastState;

				if (null != Entry)
				{
					PyObjDictEntryFüleAusScpaicerOderProzes(Entry, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);

					PyObj.AusDictLastState = Entry.Value;

					var AusDictValueAlsScpez = Entry.Value as SictAuswertPythonObjFloat;

					if (null != AusDictValueAlsScpez)
					{
						//	AusDictValueAlsScpez.LaadeReferenziirte(ProzesLeeser);

						PyObj.AusDictLastStateFloat = AusDictValueAlsScpez.Float;
					}
				}
			}

			{
				var Entry = SuuceMengeEntryErgeebnis.DictEntryRotation;

				if (null != Entry)
				{
					PyObjDictEntryFüleAusScpaicerOderProzes(Entry, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);

					PyObj.AusDictRotation = Entry.Value;

					var AusDictValueAlsScpez = Entry.Value as SictAuswertPythonObjFloat;

					if (null != AusDictValueAlsScpez)
					{
						//	AusDictValueAlsScpez.LaadeReferenziirte(ProzesLeeser);

						PyObj.AusDictRotationFloat = AusDictValueAlsScpez.Float;
					}
				}
			}

			{
				var Entry = SuuceMengeEntryErgeebnis.DictEntryTexture;

				if (null != Entry)
				{
					PyObjDictEntryFüleAusScpaicerOderProzes(Entry, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);

					PyObj.AusDictTexture = Entry.Value;

					var AusDictValueAlsScpez = Entry.Value as SictAuswertPythonObjTrinityTr2Sprite2dTexture;

					if (null != AusDictValueAlsScpez)
					{
						var MemBlockRef = AusDictValueAlsScpez.RefBaiOktet8 - 8;

						//	Bisher warn di gesicteten MemBlock 440 Oktet groos.
						var MemBlockListeOktet = ProzesLeeser.ListeOktetLeeseVonAdrese(MemBlockRef, 0x100, false);

						if (null != MemBlockListeOktet)
						{
							var BaiPlus80Ref = BitConverter.ToUInt32(MemBlockListeOktet, 80);

							PyObj.AusDictTextureMemBlockPlus80Ref = BaiPlus80Ref;
						}
					}
				}
			}

			{
				var Entry = SuuceMengeEntryErgeebnis.DictEntryColor;

				if (null != Entry)
				{
					PyObjDictEntryFüleAusScpaicerOderProzes(Entry, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);

					PyObj.AusDictColor = Entry.Value;

					var AusDictValueAlsScpez = Entry.Value as SictAuswertPyObjPyColor;

					if (null != AusDictValueAlsScpez)
					{
						LaadeReferenziirte(AusDictValueAlsScpez, ProzesLeeser, true, true, 0x10);
					}
				}
			}

			{
				var Entry = SuuceMengeEntryErgeebnis.DictEntry_Sr;

				if (null != Entry)
				{
					PyObjDictEntryFüleAusScpaicerOderProzes(Entry, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);

					PyObj.AusDict_Sr = Entry.Value;

					var AusDictValueAlsScpez = Entry.Value as SictAuswertPythonObjBunch;

					if (null != AusDictValueAlsScpez)
					{
						AusDictValueAlsScpez.LaadeReferenziirte(ProzesLeeser, 0x100);

						{
							var EntryHtmlstr =
								InPyBunchSuuceEntryFürKeyString(
								AusDictValueAlsScpez,
								GbsAstSrBunchSclüselHtmlstrString, ProzesLeeser,
								true,
								ErmitleTypNurAusScpaicer,
								DictListeEntryAnzaalScrankeMax);

							if (null != EntryHtmlstr)
							{
								PyObjDictEntryFüleAusScpaicerOderProzes(EntryHtmlstr, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);

								PyObj.AusDict_SrEntryHtmlstr = EntryHtmlstr.Value;

								var EntryHtmlstrValueAlsScpez = EntryHtmlstr.Value as SictAuswertPythonObjUnicode;

								if (null != EntryHtmlstrValueAlsScpez)
								{
									EntryHtmlstrValueAlsScpez.LaadeReferenziirte(ProzesLeeser);

									PyObj.AusDict_SrEntryHtmlstrString = EntryHtmlstrValueAlsScpez.String;
								}
							}
						}
					}
				}
			}

			{
				/*
				 * 2013.09.24 Ersaz durc InDictMengeEntryScpezGbsAstBerecne
				 * 
				var EntryChildren =
					InPyDictSuuceEntryFürKeyString(
					PyObjDict,
					GbsAstSclüselChildrenString,
					ProzesLeeser,
					true,
					ErmitleTypNurAusScpaicer,
					DictListeEntryAnzaalScrankeMax);
				 * */

				var EntryChildren = SuuceMengeEntryErgeebnis.DictEntryChildren;

				if (null != EntryChildren)
				{
					PyObjDictEntryFüleAusScpaicerOderProzes(EntryChildren, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);

					var PyObjChildrenList = EntryChildren.Value as SictAuswertPythonObjPyOderUiChildrenList;

					if (null != PyObjChildrenList)
					{
						LaadeReferenziirte(PyObjChildrenList, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);

						var PyObjChildrenListDict = PyObjChildrenList.Dict;

						if (null != PyObjChildrenListDict)
						{
							PyObjChildrenListDict.LaadeReferenziirte(
								ProzesLeeser,
								DictListeEntryAnzaalScrankeMax);

							var EntryChildrenObjects =
								InPyDictSuuceEntryFürKeyString(
								PyObjChildrenListDict,
								PyChildrenListSclüselChildrenObjectsString,
								ProzesLeeser,
								ObjSezeNaacScpaicer,
								ErmitleTypNurAusScpaicer);

							if (null != EntryChildrenObjects)
							{
								PyObjDictEntryFüleAusScpaicerOderProzes(EntryChildrenObjects, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);

								var ChildrenObjectsList = EntryChildrenObjects.Value as SictAuswertPythonObjList;

								if (null != ChildrenObjectsList)
								{
									ChildrenObjectsList.LaadeReferenziirte(ProzesLeeser);

									var ListeItemRef = ChildrenObjectsList.ListeItemRef;

									PyObj.AusChildrenListRef = (null == ListeItemRef) ? null : ListeItemRef.ToArray();
								}
							}
						}
					}
				}
			}
		}

		public void LaadeReferenziirte(
			SictAuswertPyObjPyColor PyObj,
			IMemoryReader ProzesLeeser,
			bool ObjSezeNaacScpaicer = false,
			bool ErmitleTypNurAusScpaicer = true,
			int? DictListeEntryAnzaalScrankeMax = 0x400)
		{
			if (null == PyObj)
			{
				return;
			}

			LaadeReferenziirte(PyObj as SictAuswertPythonObjMitRefDictBaiPlus8, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);

			var PyObjDict = PyObj.Dict;

			if (null == PyObjDict)
			{
				return;
			}

			var ListeKomponenteBezaicner = new string[] { "_a", "_r", "_g", "_b" };

			var ListeKomponenteObjektUndWertMilli = new KeyValuePair<SictAuswertPythonObj, int?>[ListeKomponenteBezaicner.Length];

			for (int KomponenteIndex = 0; KomponenteIndex < ListeKomponenteBezaicner.Length; KomponenteIndex++)
			{
				var KomponenteBezaicner = ListeKomponenteBezaicner[KomponenteIndex];

				SictAuswertPythonObj KomponentePyObj = null;
				int? KomponenteWertMili = null;

				var Entry =
					InPyDictSuuceEntryFürKeyString(
					PyObjDict,
					KomponenteBezaicner,
					ProzesLeeser,
					true,
					ErmitleTypNurAusScpaicer,
					DictListeEntryAnzaalScrankeMax);

				if (null != Entry)
				{
					PyObjDictEntryFüleAusScpaicerOderProzes(Entry, ProzesLeeser, ObjSezeNaacScpaicer, ErmitleTypNurAusScpaicer);

					KomponentePyObj = Entry.Value;
				}

				{
					var AusDictValueAlsScpezInt = KomponentePyObj as SictAuswertPythonObjInt;

					if (null != AusDictValueAlsScpezInt)
					{
						KomponenteWertMili = AusDictValueAlsScpezInt.Int * 1000;
					}
				}

				{
					var AusDictValueAlsScpezFloat = KomponentePyObj as SictAuswertPythonObjFloat;

					if (null != AusDictValueAlsScpezFloat)
					{
						var AusDictValueAlsScpezFloatFloat = AusDictValueAlsScpezFloat.Float;

						if (AusDictValueAlsScpezFloatFloat.HasValue)
						{
							KomponenteWertMili = (int)(AusDictValueAlsScpezFloatFloat.Value * 1e+3);
						}
					}
				}

				ListeKomponenteObjektUndWertMilli[KomponenteIndex] = new KeyValuePair<SictAuswertPythonObj, int?>(KomponentePyObj, KomponenteWertMili);
			}

			PyObj.AusDictA = ListeKomponenteObjektUndWertMilli[0].Key;
			PyObj.AusDictWertAMilli = ListeKomponenteObjektUndWertMilli[0].Value;

			PyObj.AusDictR = ListeKomponenteObjektUndWertMilli[1].Key;
			PyObj.AusDictWertRMilli = ListeKomponenteObjektUndWertMilli[1].Value;

			PyObj.AusDictG = ListeKomponenteObjektUndWertMilli[2].Key;
			PyObj.AusDictWertGMilli = ListeKomponenteObjektUndWertMilli[2].Value;

			PyObj.AusDictB = ListeKomponenteObjektUndWertMilli[3].Key;
			PyObj.AusDictWertBMilli = ListeKomponenteObjektUndWertMilli[3].Value;
		}

		public void MengePyObjSezeNaacScpaicer(IEnumerable<SictAuswertPythonObj> MengePyObj)
		{
			if (null == MengePyObj)
			{
				return;
			}

			foreach (var PyObj in MengePyObj)
			{
				if (null == PyObj)
				{
					continue;
				}

				this.MengeFürHerkunftAdrPyObj[PyObj.HerkunftAdrese] = PyObj;
			}
		}

		public void PyObjSezeNaacScpaicer(SictAuswertPythonObj PyObj)
		{
			MengePyObjSezeNaacScpaicer(new SictAuswertPythonObj[] { PyObj });
		}
	}

}
