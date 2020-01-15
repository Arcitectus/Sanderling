using System;
using System.Collections.Generic;
using System.Linq;
using Fasterflect;
using Bib3;
using BotEngine.Interface;
using BotEngine.Common;
using Sanderling.MemoryReading.Production;

namespace Optimat.EveOnline
{
	public partial class SictProzesAuswertZuusctandScpezGbsBaum
	{
		readonly object Lock = new object();

		public Int64 Zait;

		readonly public Int64[] SuuceMengeWurzelAdrese;

		readonly public Int64[] MengeAstSuuceFortsazFraigaabe;

		public Int64 AstAlterScrankeMax;

		public int? AstListeChildAnzaalScrankeMax;

		public int? MengeAstAnzaalScrankeMax;

		public int? SuuceTiifeScrankeMax;

		readonly public Optimat.EveOnline.SictProzesAuswertZuusctand ProzesAuswertZuusctandBaasis;

		readonly public IMemoryReader MemoryReader;

		readonly Dictionary<Int64, SictAuswertPyObj32Zuusctand> DictZuHerkunftAdreseObjekt = new Dictionary<Int64, SictAuswertPyObj32Zuusctand>();

		readonly Dictionary<SictAuswertPyObj32Zuusctand, WertZuZaitpunktStruct<SictPyDictEntry32[]>> DictZuPyObjPropagatioonDictEntryNaacMemberLezteListeDictEntry =
			new Dictionary<SictAuswertPyObj32Zuusctand, WertZuZaitpunktStruct<SictPyDictEntry32[]>>();

		SictAuswertPyObj32GbsAstZuusctand[] GbsMengeWurzel;

		static SictScatenscpaicerDict<Type, System.Reflection.ConstructorInfo> ScatescpaicerZuTypeKonstruktorHerkunftAdreseUndBeginZait =
			new SictScatenscpaicerDict<Type, System.Reflection.ConstructorInfo>();

		public Int64 ScritLezteBeginZaitMili
		{
			private set;
			get;
		}

		public Int64 ScritLezteEndeZaitMili
		{
			private set;
			get;
		}

		public Int64 ScritLezteDauerMili
		{
			get
			{
				return ScritLezteEndeZaitMili - ScritLezteBeginZaitMili;
			}
		}

		public GbsAstInfo[] MengeGbsWurzelInfo
		{
			private set;
			get;
		}

		public GbsAstInfo GbsWurzelHauptInfo
		{
			get
			{
				/*
				 * 2015.00.06
				 * 
				 * Beobactung Probleem: ListeChildBerecne().CountNullable() nit ausraicend um "desktop" zu unterscaide von "desktopBlurred" da "desktopBlurred"
				 * mancmaal mindesctens glaic viile Ast direkt unter Wurzel aufwaist.
				 * 
				 * Daher MengeChild transitiiv zääle.
				 * 
				return
					MengeGbsWurzelInfo
					.OrderByDescendingNullable((Wurzel) => Wurzel.ListeChildBerecne().CountNullable() ?? 0)
					.FirstOrDefaultNullable();
				 * */

				return
					MengeGbsWurzelInfo
					?.OrderByDescending((Wurzel) => Wurzel.MengeChildAstTransitiiveHüle()?.Count() ?? 0)
					?.FirstOrDefault();
			}
		}

		public GbsAstInfo[] ScritLezteMengeAstGeändert
		{
			private set;
			get;
		}

		public SictProzesAuswertZuusctandScpezGbsBaum(
			IMemoryReader MemoryReader,
			Optimat.EveOnline.SictProzesAuswertZuusctand ProzesAuswertZuusctandBaasis,
			int? AstListeChildAnzaalScrankeMax = null,
			int? MengeAstAnzaalScrankeMax = null,
			int? SuuceTiifeScrankeMax = null,
			Int64[] SuuceMengeWurzelAdrese = null,
			IEnumerable<Int64> MengeAstSuuceFortsazFraigaabe = null)
		{
			this.MemoryReader = MemoryReader;
			this.ProzesAuswertZuusctandBaasis = ProzesAuswertZuusctandBaasis;
			this.AstListeChildAnzaalScrankeMax = AstListeChildAnzaalScrankeMax;
			this.MengeAstAnzaalScrankeMax = MengeAstAnzaalScrankeMax;
			this.SuuceTiifeScrankeMax = SuuceTiifeScrankeMax;
			this.SuuceMengeWurzelAdrese = SuuceMengeWurzelAdrese;
			this.MengeAstSuuceFortsazFraigaabe = MengeAstSuuceFortsazFraigaabe?.ToArray();
		}

		public Int64 DebugBrecpunktGbsAstHerkunftAdrese = -1;

		public void ZuusctandLeere()
		{
			MengeGbsWurzelInfo = null;

			DictZuHerkunftAdreseObjekt.Clear();
			DictZuPyObjPropagatioonDictEntryNaacMemberLezteListeDictEntry.Clear();
		}

		static System.Reflection.ConstructorInfo ZuTypeKonstruktorHerkunftAdreseUndBeginZaitBerecne(Type ziilType)
		{
			if (null == ziilType)
				return null;

			var MengeKonstruktor = ziilType.ConstructorsWith(Flags.InstancePublicDeclaredOnly, null);

			var ParameterListeSol = new Type[] { typeof(Int64), typeof(Int64) };

			foreach (var Konstruktor in MengeKonstruktor)
			{
				//	Prüüfe ob Parameter pasen

				var KonstruktorParameters = Konstruktor.GetParameters();

				if (null == KonstruktorParameters)
					continue;

				if (!KonstruktorParameters.Select(parameter => parameter.ParameterType).SequenceEqual(ParameterListeSol))
					continue;

				return Konstruktor;
			}

			return null;
		}

		static public Optimat.EveOnline.SictProzesAuswertZuusctand FürAuswertScritKopiiErsctele(
			Optimat.EveOnline.SictProzesAuswertZuusctand ProzesAuswertZuusctandZuKopiire)
		{
			if (null == ProzesAuswertZuusctandZuKopiire)
			{
				return null;
			}

			var Kopii = new Optimat.EveOnline.SictProzesAuswertZuusctand();

			Kopii.KopiireVon(ProzesAuswertZuusctandZuKopiire);

			//	Aus Scpaicer Objekte entferne bai deene damit zu recne isc das Werte vun Instanze sic ändern
			Kopii.AusMengePyObjEntferne((PyObj) => !((PyObj is Optimat.EveOnline.SictAuswertPythonObjType) || (PyObj is Optimat.EveOnline.SictAuswertPythonObjStr)));

			Kopii.MengePyObjSezeNaacScpaicer(Kopii.GbsMengeWurzelObj);

			return Kopii;
		}

		class SictInBerecnungAst
		{
			public SictInBerecnungAst Parent;
			public SictAuswertPyObj32GbsAstZuusctand PyObj;
			public Optimat.EveOnline.SictGbsAstAusRenderObjectMemBlok GbsAstAusRenderObjectMemoryBlok;
			public int? TiifeScrankeMax;
			public int? GbsAstAnzaalScrankeMax;

			public GbsAstInfo Info;
			//	public SictInBerecnungAst[] MengeChild;

			public SictInBerecnungAst(
				SictInBerecnungAst Parent = null,
				SictAuswertPyObj32GbsAstZuusctand PyObj = null,
				int? TiifeScrankeMax = null,
				int? GbsAstAnzaalScrankeMax = null,
				Optimat.EveOnline.SictGbsAstAusRenderObjectMemBlok GbsFensterBlok = null,
				GbsAstInfo Info = null)
			{
				this.Parent = Parent;

				this.PyObj = PyObj;

				this.TiifeScrankeMax = TiifeScrankeMax;
				this.GbsAstAnzaalScrankeMax = GbsAstAnzaalScrankeMax;

				this.GbsAstAusRenderObjectMemoryBlok = GbsFensterBlok;
				this.Info = Info;
			}
		}

		static public SictProzesAuswertZuusctandScpezGbsBaum BerecneScrit(SictProzesAuswertZuusctandScpezGbsBaum zuusctand)
		{
			zuusctand?.BerecneScrit();

			return zuusctand;
		}

		static Int64? WertMiliAusPyObj(SictAuswertPyObj32Zuusctand pyObj)
		{
			var PyObjAlsInt = pyObj as SictAuswertPyObj32Int32Zuusctand;

			if (null != PyObjAlsInt)
				return PyObjAlsInt.WertInt32 * 1000;

			var PyObjAlsFloat = pyObj as SictAuswertPyObj32Float64Zuusctand;

			if (null != PyObjAlsFloat)
			{
				var WertFloat = PyObjAlsFloat.WertFloat64;

				return (Int64)(PyObjAlsFloat.WertFloat64 * 1e+3);
			}

			return null;
		}

		public void BerecneScrit()
		{
			try
			{
				{
					var ProzesLeeser = MemoryReader;

					if (null == ProzesLeeser)
					{
						return;
					}

					bool tBool;

					var Dauer = new SictMesungZaitraumAusStopwatch(true);

					var BeginZaitMikro = Dauer.BeginZaitMikro ?? 0;

					var BeginZaitMili = BeginZaitMikro / 1000;

					var Zait = BeginZaitMili;

					try
					{
						Int64 InScritListeAstAnzaal = 0;

						lock (Lock)
						{
							var AstListeChildAnzaalScrankeMax = this.AstListeChildAnzaalScrankeMax;
							var MengeAstAnzaalScrankeMax = this.MengeAstAnzaalScrankeMax;
							var SuuceTiifeScrankeMax = this.SuuceTiifeScrankeMax;
							var SuuceMengeWurzelAdrese = this.SuuceMengeWurzelAdrese;
							var MengeAstSuuceFortsazFraigaabe = this.MengeAstSuuceFortsazFraigaabe;

							var ProzesAuswertZuusctand = FürAuswertScritKopiiErsctele(this.ProzesAuswertZuusctandBaasis);

							if (null == ProzesAuswertZuusctand)
							{
								return;
							}

							var GbsMengeWurzel = this.GbsMengeWurzel;

							if (null == SuuceMengeWurzelAdrese)
							{
								var KweleGbsMengeWurzelObj = ProzesAuswertZuusctand.GbsMengeWurzelObj;

								if (null != KweleGbsMengeWurzelObj)
								{
									SuuceMengeWurzelAdrese =
										KweleGbsMengeWurzelObj
										.Select((KweleWurzelObj) => KweleWurzelObj.HerkunftAdrese).ToArray();
								}
							}

							if (null != SuuceMengeWurzelAdrese)
							{
								GbsMengeWurzel =
									SuuceMengeWurzelAdrese
									.Select((WurzelAdrese) => new SictAuswertPyObj32GbsAstZuusctand(WurzelAdrese, BeginZaitMili)).ToArray();
							}

							if (null == GbsMengeWurzel)
							{
								return;
							}

							var InternListeWurzelInfo = new List<GbsAstInfo>();

							var GbsMengeWurzelObj = ProzesAuswertZuusctand.GbsMengeWurzelObj;

							var MengeAstInfoGeändert = new List<GbsAstInfo>();

							try
							{
								foreach (var GbsWurzel in GbsMengeWurzel)
								{
									if (MengeAstAnzaalScrankeMax < InScritListeAstAnzaal)
									{
										break;
									}

									var MengeGbsAstVerarbaitet = new Dictionary<Int64, bool>();

									var MengeGbsAstZuVerarbaite = new List<SictInBerecnungAst>();

									var BerecnungAstWurzel = new SictInBerecnungAst(null, GbsWurzel, SuuceTiifeScrankeMax);

									MengeGbsAstZuVerarbaite.Add(BerecnungAstWurzel);

									while (0 < MengeGbsAstZuVerarbaite.Count)
									{
										var GbsAst = MengeGbsAstZuVerarbaite[0];

										MengeGbsAstZuVerarbaite.RemoveAt(0);

										++InScritListeAstAnzaal;

										if (MengeAstAnzaalScrankeMax < InScritListeAstAnzaal)
										{
											/*
											 * 2013.11.30 Beobactung:
											 * 500k < InScritListeAstAnzaal: lange reaktioonszait bis fertigsctelung Funktioon,
											 * daher ainfüürung Scranke Anzaal gesamt und Pro Ast.
											 * Ursäclic könte auc aine Sclaife sain, daher auc ainfüürung Scranke für Tiife: SuuceTiifeScrankeMax.
											 * */
											break;
										}

										var GbsAstPyObj = GbsAst.PyObj;

										if (null == GbsAstPyObj)
										{
											continue;
										}

										if (GbsAstPyObj.HerkunftAdrese == DebugBrecpunktGbsAstHerkunftAdrese)
										{
											//	Verzwaigung zum seze von Haltepunkt für Inspekt
										}

										MengeGbsAstVerarbaitet[GbsAstPyObj.HerkunftAdrese] = true;

										if (null != MengeAstSuuceFortsazFraigaabe)
										{
											if (!(GbsAstPyObj.HerkunftAdrese == GbsWurzel.HerkunftAdrese))
											{
												if (!MengeAstSuuceFortsazFraigaabe.Contains(GbsAstPyObj.HerkunftAdrese))
												{
													continue;
												}
											}
										}

										GbsAst.Info = GbsAstPyObj.AstInfo;

										bool GbsAstGeändert;
										bool GbsAstDictGeändert;

										GbsAstPyObj.Aktualisiire(ProzesLeeser, out GbsAstGeändert, BeginZaitMili);

										if (null == GbsAstPyObj.TypeObjektKlas)
										{
											GbsAstPyObj.TypeObjektKlas = ProzesAuswertZuusctand.PyObjFürHerkunftAdreseAusScpaicer(GbsAstPyObj.RefType) as Optimat.EveOnline.SictAuswertPythonObjType;
										}

										var GbsAstPyObjDictObj = GbsAstPyObj.DictObj;

										if (null == GbsAstPyObjDictObj)
										{
											continue;
										}

										GbsAstPyObjDictObj.Aktualisiire(ProzesLeeser, out GbsAstDictGeändert, BeginZaitMili);

										AstInfoScteleSicerAktuelOoneListeChild(
											GbsAstPyObj,
											ProzesLeeser,
											ProzesAuswertZuusctand,
											BeginZaitMili,
											null);

										var AstInfo = GbsAstPyObj.AstInfo;

										{
											//	Mesung MemoryBlock aus RenderObject (darin sin z.B. Laage und Grööse enthalte)

											var AusDictRenderObject = GbsAstPyObj.DictEntryRenderObject;

											if (null != AusDictRenderObject)
											{
												var RenderObjectMemBlokAdr = AusDictRenderObject.ObjektBegin.BaiPlus8UInt32 + SictAuswertPyObj32GbsAstZuusctand.AusDictRenderObjectRefVersazNaacRenderObjectBlok;

												if (0x10 < RenderObjectMemBlokAdr && !"UIRoot".EqualsIgnoreCase(AstInfo.PyObjTypName))
												{
													var GbsFensterBlokListeOktet = ProzesLeeser.ListeOktetLeeseVonAdrese(RenderObjectMemBlokAdr, Optimat.EveOnline.SictGbsAstAusRenderObjectMemBlok.GbsBaumAstListeOktetAnzaal);

													var GbsFensterBlokAuswert = new Optimat.EveOnline.SictGbsAstAusRenderObjectMemBlok(RenderObjectMemBlokAdr, GbsFensterBlokListeOktet);

													GbsAst.GbsAstAusRenderObjectMemoryBlok = GbsFensterBlokAuswert;
												}
											}
										}

										byte? RenderObjectMemBlokOktetSictbarkait = null;

										{
											//	Extraktioon Info aus von RenderObject referenziirte Scpaicerblok

											var GbsAstBlokAuswert = GbsAst.GbsAstAusRenderObjectMemoryBlok;

											if (null != GbsAstBlokAuswert)
											{
												/*
												 * 2013.11.08
												 * Werd von Server zurzait nit verwendet daher aingescpaart.
												 * 
												AstInfo.RenderObjectMemBlokAdr = GbsAstBlokAuswert.Adrese;
												 * */
												/*
												 * 2014.01.04
												 * 
												var RenderObjectMemBlokOktetSictbarkait = AstInfo.RenderObjectMemBlokOktetSictbarkait = GbsAstBlokAuswert.OktetSictbarkaitWert;
												 * */

												RenderObjectMemBlokOktetSictbarkait = GbsAstBlokAuswert.OktetSictbarkaitWert;

												var Laage = GbsAstBlokAuswert.Laage;
												var Grööse = GbsAstBlokAuswert.Grööse;

												if (null != Laage)
												{
													AstInfo.LaageInParent = new Vektor2DSingle(Laage[0], Laage[1]);
												}

												if (null != Grööse)
												{
													AstInfo.Grööse = new Vektor2DSingle(Grööse[0], Grööse[1]);
												}
											}
										}

										{
											//	Berecnung Htmlstr

											var GbsAstPyObjDictEntry_Sr = GbsAstPyObj.DictEntry_Sr as SictAuswertPyObj32BunchZuusctand;

											if (null != GbsAstPyObjDictEntry_Sr)
											{
												var AusBunchEntry = new SictObjektDictEntryAusSrBunch();

												ScraibeDictEntryNaacMember(
													AusBunchEntry,
													GbsAstPyObjDictEntry_Sr.ListeDictEntry,
													ProzesLeeser,
													ProzesAuswertZuusctand,
													Zait);

												{
													var AusBunchDictEntryHtmlstr = AusBunchEntry.DictEntryHtmlstr as SictAuswertPyObj32UnicodeZuusctand;

													if (null != AusBunchDictEntryHtmlstr)
													{
														AusBunchDictEntryHtmlstr.Aktualisiire(
															ProzesLeeser,
															out tBool,
															Zait);

														AstInfo.SrHtmlstr = AusBunchDictEntryHtmlstr.WertString;
													}
												}

												{
													var AusBunchDictEntryNode = AusBunchEntry.DictEntryNode as SictAuswertPyObj32BunchZuusctand;

													if (null != AusBunchDictEntryNode)
													{
														AusBunchDictEntryNode.Aktualisiire(
															ProzesLeeser,
															out tBool,
															Zait);

														var AusBunchNodeEntry = new SictObjektDictEntryAusSrBunchNode();

														ScraibeDictEntryNaacMember(
															AusBunchNodeEntry,
															AusBunchDictEntryNode.ListeDictEntry,
															ProzesLeeser,
															ProzesAuswertZuusctand,
															Zait);

														var GlyphString = AusBunchNodeEntry.DictEntryGlyphString as SictAuswertPyObj32Tr2GlyphStringZuusctand;

														if (null != GlyphString)
														{
															GlyphString.Aktualisiire(
																ProzesLeeser,
																out tBool,
																Zait);

															var GlyphStringDict = GlyphString.DictObj;

															if (null != GlyphStringDict)
															{
																GlyphStringDict.ListeEntryAnzaalScrankeMax = 0x40;

																GlyphStringDict.Aktualisiire(
																	ProzesLeeser,
																	out tBool,
																	Zait);

																var AusGlyphStringDictEntry = new SictObjektDictEntryAusTr2GlyphStringDict();

																ScraibeDictEntryNaacMember(
																	AusGlyphStringDictEntry,
																	GlyphStringDict.ListeDictEntry,
																	ProzesLeeser,
																	ProzesAuswertZuusctand,
																	Zait);

																var GlyphStringDictEntryText = AusGlyphStringDictEntry.DictEntryText as SictAuswertPyObj32UnicodeZuusctand;

																if (null != GlyphStringDictEntryText)
																{
																	GlyphStringDictEntryText.Aktualisiire(
																		ProzesLeeser,
																		out tBool,
																		Zait);

																	AstInfo.EditTextlineCoreText = GlyphStringDictEntryText.WertString;
																}
															}
														}
													}
												}
											}
										}

										{
											//	Berecnung Texture Ident

											var DictEntryTexture = GbsAstPyObj.DictEntryTexture as SictAuswertPyObj32TextureZuusctand;

											if (null != DictEntryTexture)
											{
												var MemBlockRef = DictEntryTexture.BaiPlus8Ref - 8;

												//	Bisher warn di gesicteten MemBlock 440 Oktet groos.
												var MemBlockListeOktet = ProzesLeeser.ListeOktetLeeseVonAdrese(MemBlockRef, 0x100, true);

												var TextureIdent0RefAdreseVonBlokBegin = 80;

												if (TextureIdent0RefAdreseVonBlokBegin + 4 <= MemBlockListeOktet?.Count())
												{
													var TextureIdent0Ref = BitConverter.ToUInt32(MemBlockListeOktet, TextureIdent0RefAdreseVonBlokBegin);

													AstInfo.TextureIdent0 = TextureIdent0Ref;
												}
											}
										}

										{
											//	Berecnung Sictbarkait

											AstInfo.VisibleIncludingInheritance = true;

											if (null != GbsAst.Parent)
											{
												/*
												 * 2014.01.04
												 * 
												if (RenderObjectMemBlokOktetSictbarkait.HasValue)
												{
													if (1 != RenderObjectMemBlokOktetSictbarkait)
													{
														AstInfo.SictbarkaitMitVonParentErbe = false;
													}
												}
												 * */

												if (!(1 == RenderObjectMemBlokOktetSictbarkait))
												{
													AstInfo.VisibleIncludingInheritance = false;
												}

												var ParentInfo = GbsAst.Parent.Info;

												if (null != ParentInfo)
												{
													/*
													 * 2014.01.04
													 * VonParentErbeLaage werd zuukünftig ersct bai Auswertung Berecnet.
													 * 
													AstInfo.VonParentErbeLaage = ParentInfo.LaagePlusVonParentErbeLaage;
													 * */

													if (!(true == ParentInfo.VisibleIncludingInheritance) ||
														true == ParentInfo.Minimized)
													{
														AstInfo.VisibleIncludingInheritance = false;
													}

													/*
													 * 2014.01.04
													 * 
													if (ParentInfo.RenderObjectMemBlokOktetSictbarkait.HasValue)
													{
														if (1 != ParentInfo.RenderObjectMemBlokOktetSictbarkait)
														{
															AstInfo.SictbarkaitMitVonParentErbe = false;
														}
													}
													 * */

													if (!(true == ParentInfo.VisibleIncludingInheritance))
													{
														AstInfo.VisibleIncludingInheritance = false;
													}
												}
											}
										}

										if (!(true == AstInfo.VisibleIncludingInheritance))
										{
											//	Nict sictbaare Ast werde ignoriirt.
											AstInfo.ListChild = null;
											continue;
										}

										var GbsAstDictEntryChildren = GbsAstPyObj.DictEntryChildren as SictAuswertPyObj32PyOderUiChildrenList;

										if (null != GbsAstDictEntryChildren)
										{
											if (!(BeginZaitMili <= GbsAstDictEntryChildren.AktualisLezteZait))
											{
												GbsAstDictEntryChildren.Aktualisiire(
													ProzesLeeser,
													out tBool,
													BeginZaitMili);

											}

											var GbsAstDictEntryChildrenDict = GbsAstDictEntryChildren.DictObj;

											if (null != GbsAstDictEntryChildrenDict)
											{
												GbsAstDictEntryChildrenDict.Aktualisiire(ProzesLeeser, out GbsAstDictGeändert, BeginZaitMili);
											}

											MemberAusDictEntryScteleSicerAktuel(
												GbsAstDictEntryChildren,
												ProzesLeeser,
												ProzesAuswertZuusctand,
												Zait,
												null,
												true);

											var DictEntryChildrenDictEntryListChildrenObj = GbsAstDictEntryChildren.DictEntryListChildrenObj as SictAuswertPyObj32ListZuusctand;

											if (null == DictEntryChildrenDictEntryListChildrenObj)
											{
												GbsAstPyObj.ListeChild.Clear();
											}
											else
											{
												DictEntryChildrenDictEntryListChildrenObj.ListeItemAnzaalScrankeMax = 0x100;

												if (!(Zait <= DictEntryChildrenDictEntryListChildrenObj.AktualisLezteZait))
												{
													DictEntryChildrenDictEntryListChildrenObj.Aktualisiire(
														ProzesLeeser,
														out tBool,
														Zait);
												}

												{
													//	Aus List ListeItemRef Elemente naac GbsAstPyObj.ListeChild propagiire

													var ChildrenListeItemRef = DictEntryChildrenDictEntryListChildrenObj.ListeItemRef;

													if (null == ChildrenListeItemRef)
													{
														GbsAstPyObj.ListeChild.Clear();
													}
													else
													{
														var AstListeChildAnzaal = (int)Math.Min(AstListeChildAnzaalScrankeMax ?? int.MaxValue, ChildrenListeItemRef.Length);

														for (int ElementIndex = 0; ElementIndex < AstListeChildAnzaal; ElementIndex++)
														{
															var ChildObjHerkunftAdrese = ChildrenListeItemRef[ElementIndex];

															var ChildObj = ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer
																<SictAuswertPyObj32GbsAstZuusctand>(
																ChildObjHerkunftAdrese,
																ProzesLeeser,
																ProzesAuswertZuusctand,
																Zait);

															if (MengeGbsAstVerarbaitet.ContainsKey(ChildObjHerkunftAdrese))
															{
																/*
																 * 2013.11.30 Beobactung:
																 * StackOverflowException in SictRefBaumKopii.ObjektKopiiErsctele für GbsAstInfo:
																 * Diis doitet darauf hin das hiir aine Sclaife aingebaut wurde.
																 * Der Ainbau von Referenzsclaife in GbsAstInfo sol zuukünftig mit diiser Verzwaigung verhindert werde.
																 * */

																ChildObj = null;
															}

															if (GbsAstPyObj.ListeChild.Count <= ElementIndex)
															{
																GbsAstPyObj.ListeChild.Add(ChildObj);
															}
															else
															{
																GbsAstPyObj.ListeChild[ElementIndex] = ChildObj;
															}
														}

														var AnEndeZuEntferneAnzaal = GbsAstPyObj.ListeChild.Count - AstListeChildAnzaal;

														if (0 < AnEndeZuEntferneAnzaal)
														{
															GbsAstPyObj.ListeChild.RemoveRange(AstListeChildAnzaal, AnEndeZuEntferneAnzaal);
														}
													}
												}
											}
										}

										GbsAstPyObj.ListeChildPropagiireNaacInfoObjekt();

										foreach (var Child in GbsAstPyObj.ListeChild)
										{
											MengeGbsAstZuVerarbaite.Add(new SictInBerecnungAst(GbsAst, Child, GbsAst.TiifeScrankeMax - 1));
										}
									}

									InternListeWurzelInfo.Add(BerecnungAstWurzel.Info);
								}
							}
							finally
							{
								this.MengeGbsWurzelInfo = InternListeWurzelInfo.ToArray();

								this.ScritLezteMengeAstGeändert = MengeAstInfoGeändert.ToArray();
							}
						}
					}
					finally
					{
						ScritLezteBeginZaitMili = (Dauer.BeginZaitMikro ?? 0) / 1000;

						Dauer.EndeSezeJezt();

						ScritLezteEndeZaitMili = (Dauer.EndeZaitMikro ?? 0) / 1000;
					}
				}
			}
			finally
			{
			}
		}

		static Type AssemblyTypeFürPyTypeObjAdrese(
			Int64 PyTypeObjAdrese,
			Optimat.EveOnline.SictProzesAuswertZuusctand ProzesAuswertZuusctandKlas)
		{
			var KlasAssemblyType = ProzesAuswertZuusctandKlas.GibAssemblyTypFürPythonTypeMitHerkunftAdrese(PyTypeObjAdrese);

			if (null != KlasAssemblyType)
			{
				if (typeof(Optimat.EveOnline.SictAuswertPythonObjInt) == KlasAssemblyType)
				{
					return typeof(SictAuswertPyObj32Int32Zuusctand);
				}

				if (typeof(Optimat.EveOnline.SictAuswertPythonObjBool) == KlasAssemblyType)
				{
					return typeof(SictAuswertPyObj32BoolZuusctand);
				}

				if (typeof(Optimat.EveOnline.SictAuswertPythonObjFloat) == KlasAssemblyType)
				{
					return typeof(SictAuswertPyObj32Float64Zuusctand);
				}

				if (typeof(Optimat.EveOnline.SictAuswertPythonObjStr) == KlasAssemblyType)
				{
					return typeof(SictAuswertPyObj32StrZuusctand);
				}

				if (typeof(Optimat.EveOnline.SictAuswertPythonObjLong) == KlasAssemblyType)
				{
					return typeof(SictAuswertPyObj32LongZuusctand);
				}

				if (typeof(Optimat.EveOnline.SictAuswertPythonObjInstance) == KlasAssemblyType)
				{
					return typeof(SictAuswertPyObj32InstanceZuusctand);
				}

				if (typeof(Optimat.EveOnline.SictAuswertPythonObjUnicode) == KlasAssemblyType)
				{
					return typeof(SictAuswertPyObj32UnicodeZuusctand);
				}

				if (typeof(Optimat.EveOnline.SictAuswertPythonObjPyOderUiChildrenList) == KlasAssemblyType)
				{
					return typeof(SictAuswertPyObj32PyOderUiChildrenList);
				}

				if (typeof(Optimat.EveOnline.SictAuswertPyObjGbsAst) == KlasAssemblyType)
				{
					return typeof(SictAuswertPyObj32GbsAstZuusctand);
				}

				if (typeof(Optimat.EveOnline.SictAuswertPythonObjList) == KlasAssemblyType)
				{
					return typeof(SictAuswertPyObj32ListZuusctand);
				}

				if (typeof(Optimat.EveOnline.SictAuswertPythonObjBunch) == KlasAssemblyType)
				{
					return typeof(SictAuswertPyObj32BunchZuusctand);
				}

				if (typeof(Optimat.EveOnline.SictAuswertPythonObjTrinityTr2Sprite2dTexture) == KlasAssemblyType)
				{
					return typeof(SictAuswertPyObj32TextureZuusctand);
				}

				if (typeof(Optimat.EveOnline.SictAuswertPythonObjTr2GlyphString) == KlasAssemblyType)
				{
					return typeof(SictAuswertPyObj32Tr2GlyphStringZuusctand);
				}

				if (typeof(Optimat.EveOnline.SictAuswertPyObjPyColor) == KlasAssemblyType)
				{
					return typeof(SictAuswertPyObj32ColorZuusctand);
				}
			}

			return null;
		}

		public SictAuswertPyObj32Zuusctand ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer(
			Int64 HerkunftAdrese,
			IMemoryReader AusProzesLeeser,
			Optimat.EveOnline.SictProzesAuswertZuusctand ProzesAuswertZuusctandKlas,
			Int64 Zait)
		{
			return ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer<SictAuswertPyObj32Zuusctand>(
				HerkunftAdrese,
				AusProzesLeeser,
				ProzesAuswertZuusctandKlas,
				Zait,
				true);
		}

		public T ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer<T>(
			Int64 HerkunftAdrese,
			IMemoryReader AusProzesLeeser,
			Optimat.EveOnline.SictProzesAuswertZuusctand ProzesAuswertZuusctandKlas,
			Int64 Zait,
			bool TypeIgnoriire = false)
			where T : SictAuswertPyObj32Zuusctand
		{
			//	var Objekt = Optimat.Glob.TAD(DictZuHerkunftAdreseObjekt, HerkunftAdrese);

			SictAuswertPyObj32Zuusctand Objekt = null;

			DictZuHerkunftAdreseObjekt.TryGetValue(HerkunftAdrese, out Objekt);

			if (null != Objekt)
			{
				if (Objekt.RefTypeGeändert)
				{
					//	Python Type des Objekt wurde geändert -> Scpiigelobjekt solte hiir noi ersctelt werde damit AssemblyType zum noien PythonType past.
					DictZuHerkunftAdreseObjekt.Remove(HerkunftAdrese);

					Objekt = null;
				}
			}

			var ObjektScpez = Objekt as T;

			if (null == ObjektScpez)
			{
				if (TypeIgnoriire)
				{
					ObjektScpez = ObjektFürHerkunftAdreseErsctele(HerkunftAdrese, AusProzesLeeser, ProzesAuswertZuusctandKlas, Zait) as T;
				}
				else
				{
					ObjektScpez = ObjektFürHerkunftAdreseErsctele<T>(HerkunftAdrese, Zait);
				}

				DictZuHerkunftAdreseObjekt[HerkunftAdrese] = ObjektScpez;
			}

			return ObjektScpez;
		}

		SictAuswertPyObj32Zuusctand ObjektFürHerkunftAdreseErsctele(
			Int64 herkunftAdrese,
			IMemoryReader ausProzesLeeser,
			Optimat.EveOnline.SictProzesAuswertZuusctand prozesAuswertZuusctandKlas,
			Int64 zait)
		{
			var ZiilAssemblyType = typeof(SictAuswertPyObj32Zuusctand);

			if (null != prozesAuswertZuusctandKlas)
			{
				var TestTypeObj = new SictAuswertPyObj32Zuusctand(herkunftAdrese, zait);

				bool tBool;

				TestTypeObj.Aktualisiire(ausProzesLeeser, out tBool, zait);

				var RefType = TestTypeObj.RefType;

				var KandidaatZiilAssemblyType = AssemblyTypeFürPyTypeObjAdrese(RefType, prozesAuswertZuusctandKlas);

				if (null != KandidaatZiilAssemblyType)
				{
					ZiilAssemblyType = KandidaatZiilAssemblyType;
				}
			}

			return ObjektFürHerkunftAdreseErsctele(herkunftAdrese, ZiilAssemblyType, zait);
		}

		T ObjektFürHerkunftAdreseErsctele<T>(
			Int64 herkunftAdrese,
			Int64 zait)
			where T : SictAuswertPyObj32Zuusctand =>
			ObjektFürHerkunftAdreseErsctele(herkunftAdrese, typeof(T), zait) as T;

		SictAuswertPyObj32Zuusctand ObjektFürHerkunftAdreseErsctele(
			Int64 herkunftAdrese,
			Type assemblyType,
			Int64 zait)
		{
			if (null == assemblyType)
				return null;

			var Konstruktor = ScatescpaicerZuTypeKonstruktorHerkunftAdreseUndBeginZait.ValueFürKey(assemblyType, ZuTypeKonstruktorHerkunftAdreseUndBeginZaitBerecne);

			if (null == Konstruktor)
			{
				return null;
			}

			var Objekt = Konstruktor.Invoke(new object[] { herkunftAdrese, zait });

			return Objekt as SictAuswertPyObj32Zuusctand;
		}

		static SictScatenscpaicerDict<Type, KeyValuePair<string, SictZuMemberAusDictEntryInfo>[]> ScatescpaicerZuTypeMengeZuDictKeyFieldMemberInfo =
			new SictScatenscpaicerDict<Type, KeyValuePair<string, SictZuMemberAusDictEntryInfo>[]>();

		static public KeyValuePair<string, SictZuMemberAusDictEntryInfo>[] ZuTypeMengeZuDictKeyFieldMemberInfoAusScatenscpaicerOderBerecne(Type ziilType) =>
			ScatescpaicerZuTypeMengeZuDictKeyFieldMemberInfo.ValueFürKey(ziilType, ZuTypeMengeZuDictKeyFieldMemberInfoBerecne);

		static public KeyValuePair<string, SictZuMemberAusDictEntryInfo>[] ZuTypeMengeZuDictKeyFieldMemberInfoBerecne(Type ziilType)
		{
			if (null == ziilType)
				return null;

			var ListeZuDictEntryKeySetter = new List<KeyValuePair<string, SictZuMemberAusDictEntryInfo>>();

			var MengeField = ziilType.GetFields();

			foreach (var Field in MengeField)
			{
				var FieldMengeAttribut = Field.GetCustomAttributes(typeof(SictInPyDictEntryKeyAttribut), true);

				if (null == FieldMengeAttribut)
					continue;

				if (!typeof(SictAuswertPyObj32Zuusctand).IsAssignableFrom(Field.FieldType))
					continue;

				foreach (var FieldAttribut in FieldMengeAttribut)
				{
					var FieldAttributAlsDictEntryKey = FieldAttribut as SictInPyDictEntryKeyAttribut;

					if (null == FieldAttributAlsDictEntryKey)
						continue;

					var DictEntryKeyString = FieldAttributAlsDictEntryKey.DictEntryKeyString;

					var Setter = ziilType.DelegateForSetFieldValue(Field.Name);

					var Getter = ziilType.DelegateForGetFieldValue(Field.Name);

					ListeZuDictEntryKeySetter.Add(new KeyValuePair<string, SictZuMemberAusDictEntryInfo>(
						DictEntryKeyString,
						new SictZuMemberAusDictEntryInfo(Field.FieldType, Setter, Getter)));
				}
			}

			return ListeZuDictEntryKeySetter.ToArray();
		}

		/// <summary>
		/// Sezt di Member von <paramref name="ziilObjekt"/> welce ain Attribut vom Typ SictInPyDictEntryKeyAttribut haaben auf noien Wert fals di Value des DictEntry
		/// zu dem Sclüsel welcer glaic deem DictEntryKeyString des Attribut ist aine andere Adrese aufwaist als dii HerkunftAdrese des bisher
		/// vom jewailigen Member referenziirte SictAuswertPyObj32Zuusctand.
		/// Arbaitet mit der Annaame das <paramref name="dict"/> beraits Aktualisiirt wurde.
		/// </summary>
		/// <param name="ziilObjekt"></param>
		/// <param name="dict">Dict aus welcem dii HerkunftAdrese für di Member ermitelt were sole.</param>
		/// <param name="ausProzesLeeser">werd verwendet um dii Referenz auf deen Python Type aines noie Objekt zu leese.</param>
		/// <param name="prozesAuswertZuusctandKlas">werd verwendet um deen Python Type von noie Objekt zu ermitle.</param>
		public void ScraibeDictEntryNaacMember(
			object ziilObjekt,
			SictAuswertPyObj32DictZuusctand dict,
			IMemoryReader ausProzesLeeser,
			Optimat.EveOnline.SictProzesAuswertZuusctand prozesAuswertZuusctandKlas,
			Int64 zait,
			Int64? objektErhaltungBeginZaitScrankeMin = null,
			bool memberAktualisiire = false)
		{
			var ListeDictEntry = (null == dict) ? null : dict.ListeDictEntry;

			ScraibeDictEntryNaacMember(
				ziilObjekt,
				ListeDictEntry,
				ausProzesLeeser,
				prozesAuswertZuusctandKlas,
				zait,
				objektErhaltungBeginZaitScrankeMin,
				memberAktualisiire);
		}

		public void ScraibeDictEntryNaacMember(
			object ZiilObjekt,
			SictPyDictEntry32[] ListeDictEntry,
			IMemoryReader AusProzesLeeser,
			Optimat.EveOnline.SictProzesAuswertZuusctand ProzesAuswertZuusctandKlas,
			Int64 Zait,
			Int64? ObjektErhaltungBeginZaitScrankeMin = null,
			bool MemberAktualisiire = false)
		{
			if (null == ZiilObjekt)
			{
				return;
			}

			var ZiilObjektType = ZiilObjekt.GetType();

			var MengeZuDictEntryKeyMemberInfo = ZuTypeMengeZuDictKeyFieldMemberInfoAusScatenscpaicerOderBerecne(ZiilObjektType);

			if (MengeZuDictEntryKeyMemberInfo.IsNullOrEmpty())
				return;

			var MengeZuDictEntryKeyMemberInfoNaacherWertObjektAdrese = new Int64[MengeZuDictEntryKeyMemberInfo.Length];

			if (null != ListeDictEntry)
			{
				foreach (var DictEntry in ListeDictEntry)
				{
					var DictEntryReferenzKey = DictEntry.ReferenzKey;
					var DictEntryReferenzValue = DictEntry.ReferenzValue;

					if (0 == DictEntryReferenzKey)
					{
						continue;
					}

					var DictEntryKeyObj = ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer(
						DictEntryReferenzKey,
						AusProzesLeeser,
						ProzesAuswertZuusctandKlas,
						Zait);

					bool tBool;

					var DictEntryKeyObjAlsString = DictEntryKeyObj as SictAuswertPyObj32StrZuusctand;

					if (null == DictEntryKeyObjAlsString)
					{
						continue;
					}

					if (!(Zait <= DictEntryKeyObjAlsString.AktualisLezteZait))
					{
						DictEntryKeyObjAlsString.Aktualisiire(AusProzesLeeser, out tBool, Zait);
					}

					var DictEntryKeyString = DictEntryKeyObjAlsString.WertString;

					if (null == DictEntryKeyString)
					{
						continue;
					}

					for (int MemberInfoIndex = 0; MemberInfoIndex < MengeZuDictEntryKeyMemberInfo.Length; MemberInfoIndex++)
					{
						var MemberInfo = MengeZuDictEntryKeyMemberInfo[MemberInfoIndex];

						//	SictAuswertPyObj32Zuusctand NaacherWert = null;

						//	var VorherWert = MemberInfoMitVorherWert.Value;

						try
						{
							if (!string.Equals(DictEntryKeyString, MemberInfo.Key))
							{
								continue;
							}

							MengeZuDictEntryKeyMemberInfoNaacherWertObjektAdrese[MemberInfoIndex] = DictEntryReferenzValue;

							break;
						}
						finally
						{
						}
					}
				}
			}

			for (int MemberInfoIndex = 0; MemberInfoIndex < MengeZuDictEntryKeyMemberInfo.Length; MemberInfoIndex++)
			{
				var MemberInfo = MengeZuDictEntryKeyMemberInfo[MemberInfoIndex];

				var VorherWert = MemberInfo.Value.Getter(ZiilObjekt);

				var NaacherWertObjektAdrese = MengeZuDictEntryKeyMemberInfoNaacherWertObjektAdrese[MemberInfoIndex];

				SictAuswertPyObj32Zuusctand NaacherWert = null;

				try
				{
					if (0 == NaacherWertObjektAdrese)
					{
						continue;
					}

					var VorherWertAlsPyObj = VorherWert as SictAuswertPyObj32Zuusctand;

					if (null != VorherWertAlsPyObj)
					{
						if (VorherWertAlsPyObj.RefTypeGeändert)
						{
						}

						if (VorherWertAlsPyObj.HerkunftAdrese == NaacherWertObjektAdrese &&
							!VorherWertAlsPyObj.RefTypeGeändert)
						{
							NaacherWert = VorherWertAlsPyObj;
						}
					}

					if (null == NaacherWert)
					{
						NaacherWert = ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer(
							NaacherWertObjektAdrese,
							AusProzesLeeser,
							ProzesAuswertZuusctandKlas,
							Zait);
					}
				}
				finally
				{
					if (null != NaacherWert)
					{
						if (MemberAktualisiire)
						{
							if (!(Zait <= NaacherWert.AktualisLezteZait))
							{
								bool tBool;

								NaacherWert.Aktualisiire(
									AusProzesLeeser,
									out tBool,
									Zait);
							}
						}
						else
						{
							if (!(Zait <= NaacherWert.AktualisLezteZait))
							{
								//	Temp Verzwaigung Debug.
							}
						}
					}

					MemberInfo.Value.Setter(ZiilObjekt, NaacherWert);
				}
			}
		}

		/// <summary>
		/// Funktioon arbaitet mit Annaame das Dict beraits aktualisiirt wurde.
		/// </summary>
		/// <param name="AusProzesLeeser"></param>
		public void AstInfoScteleSicerAktuelOoneListeChild(
			SictAuswertPyObj32GbsAstZuusctand GbsAst,
			IMemoryReader AusProzesLeeser,
			Optimat.EveOnline.SictProzesAuswertZuusctand ProzesAuswertZuusctand,
			Int64 Zait,
			Int64? ObjektErhaltungBeginZaitScrankeMin)
		{
			if (null == AusProzesLeeser)
			{
				return;
			}

			if (null == GbsAst)
			{
				return;
			}

			GbsAst.VonDictEntryMemberEntferneWelceRefTypeGeändert(Zait);

			MemberAusDictEntryScteleSicerAktuel(
				GbsAst,
				AusProzesLeeser,
				ProzesAuswertZuusctand,
				Zait,
				ObjektErhaltungBeginZaitScrankeMin,
				true);

			{
				//	Sicersctele das Member aktuel
				var MengeZuDictEntryKeyMemberInfo = ZuTypeMengeZuDictKeyFieldMemberInfoAusScatenscpaicerOderBerecne(GbsAst.GetType());

				if (null != MengeZuDictEntryKeyMemberInfo)
				{
					foreach (var MemberInfo in MengeZuDictEntryKeyMemberInfo)
					{
						var MemberWert = MemberInfo.Value.Getter(GbsAst) as SictAuswertPyObj32Zuusctand;

						if (null == MemberWert)
						{
							continue;
						}

						bool tBool;

						if (!(Zait <= MemberWert.AktualisLezteZait))
						{
							MemberWert.Aktualisiire(AusProzesLeeser, out tBool, Zait);
						}
						else
						{
						}
					}
				}
			}

			var ZiilAstInfo = GbsAst.AstInfo;

			if (null != GbsAst && null != ZiilAstInfo)
			{
				{
					var GbsAstTypeObjektKlas = GbsAst.TypeObjektKlas;

					if (null != GbsAstTypeObjektKlas)
					{
						ZiilAstInfo.PyObjTypName = GbsAstTypeObjektKlas.tp_name;
					}
				}

				_16_Erwaiterung_Manuel(
					Zait,
					AusProzesLeeser,
					ProzesAuswertZuusctand,
					GbsAst,
					ZiilAstInfo);

				{
					var DictEntryValue = GbsAst.DictEntryName;

					var DictEntryValueAlsStr = DictEntryValue as SictAuswertPyObj32StrZuusctand;

					if (null != DictEntryValueAlsStr)
					{
						ZiilAstInfo.Name = DictEntryValueAlsStr.WertString;
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntryHint;

					var DictEntryValueAlsStr = DictEntryValue as SictAuswertPyObj32StrZuusctand;

					if (null != DictEntryValueAlsStr)
					{
						ZiilAstInfo.Hint = DictEntryValueAlsStr.WertString;
					}

					var DictEntryValueAlsUnicode = DictEntryValue as SictAuswertPyObj32UnicodeZuusctand;

					if (null != DictEntryValueAlsUnicode)
					{
						ZiilAstInfo.Hint = DictEntryValueAlsUnicode.WertString;
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntryWindowId;

					var DictEntryValueAlsStr = DictEntryValue as SictAuswertPyObj32StrZuusctand;

					if (null != DictEntryValueAlsStr)
					{
						ZiilAstInfo.WindowID = DictEntryValueAlsStr.WertString;
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntryCapacitorLevel;

					var DictEntryValueAlsFloat = DictEntryValue as SictAuswertPyObj32Float64Zuusctand;

					if (null != DictEntryValueAlsFloat)
					{
						ZiilAstInfo.CapacitorLevel = (float)DictEntryValueAlsFloat.WertFloat64;
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntryShieldLevel;

					var DictEntryValueAlsFloat = DictEntryValue as SictAuswertPyObj32Float64Zuusctand;

					if (null != DictEntryValueAlsFloat)
					{
						ZiilAstInfo.ShieldLevel = (float)DictEntryValueAlsFloat.WertFloat64;
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntryArmorLevel;

					var DictEntryValueAlsFloat = DictEntryValue as SictAuswertPyObj32Float64Zuusctand;

					if (null != DictEntryValueAlsFloat)
					{
						ZiilAstInfo.ArmorLevel = (float)DictEntryValueAlsFloat.WertFloat64;
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntryStructureLevel;

					var DictEntryValueAlsFloat = DictEntryValue as SictAuswertPyObj32Float64Zuusctand;

					if (null != DictEntryValueAlsFloat)
					{
						ZiilAstInfo.StructureLevel = (float)DictEntryValueAlsFloat.WertFloat64;
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntrySpeed;

					var DictEntryValueAlsFloat = DictEntryValue as SictAuswertPyObj32Float64Zuusctand;

					if (null != DictEntryValueAlsFloat)
					{
						ZiilAstInfo.Speed = (float)DictEntryValueAlsFloat.WertFloat64;
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntryText;

					if (null != DictEntryValue)
					{
					}

					var DictEntryValueAlsStr = DictEntryValue as SictAuswertPyObj32StrZuusctand;
					var DictEntryValueAlsUnicode = DictEntryValue as SictAuswertPyObj32UnicodeZuusctand;

					if (null != DictEntryValueAlsStr)
					{
						ZiilAstInfo.Text = DictEntryValueAlsStr.WertString;
					}
					else
					{
						if (null != DictEntryValueAlsUnicode)
						{
							ZiilAstInfo.Text = DictEntryValueAlsUnicode.WertString;
						}
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntrySetText;

					if (null != DictEntryValue)
					{
					}

					var DictEntryValueAlsStr = DictEntryValue as SictAuswertPyObj32StrZuusctand;
					var DictEntryValueAlsUnicode = DictEntryValue as SictAuswertPyObj32UnicodeZuusctand;

					string Text = null;

					if (null != DictEntryValueAlsStr)
					{
						Text = DictEntryValueAlsStr.WertString;
					}
					else
					{
						if (null != DictEntryValueAlsUnicode)
						{
							Text = DictEntryValueAlsUnicode.WertString;
						}
						else
						{
							//	2015.07.10 beobactet in WindowChatChannel in eveMemberCountCont

							var DictEntryValueAlsInt = DictEntryValue as SictAuswertPyObj32Int32Zuusctand;

							if (null != DictEntryValueAlsInt)
							{
								Text = DictEntryValueAlsInt.WertInt32.ToString();
							}
						}
					}

					ZiilAstInfo.SetText = Text;
				}

				{
					var DictEntryValue = GbsAst.DictEntryCaption;

					var DictEntryValueAlsStr = DictEntryValue as SictAuswertPyObj32StrZuusctand;
					var DictEntryValueAlsUnicode = DictEntryValue as SictAuswertPyObj32UnicodeZuusctand;

					if (null != DictEntryValueAlsStr)
					{
						ZiilAstInfo.Caption = DictEntryValueAlsStr.WertString;
					}
					else
					{
						if (null != DictEntryValueAlsUnicode)
						{
							ZiilAstInfo.Caption = DictEntryValueAlsUnicode.WertString;
						}
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntryLinkText;

					if (null != DictEntryValue)
					{
					}

					var DictEntryValueAlsStr = DictEntryValue as SictAuswertPyObj32StrZuusctand;
					var DictEntryValueAlsUnicode = DictEntryValue as SictAuswertPyObj32UnicodeZuusctand;

					if (null != DictEntryValueAlsStr)
					{
						ZiilAstInfo.LinkText = DictEntryValueAlsStr.WertString;
					}
					else
					{
						if (null != DictEntryValueAlsUnicode)
						{
							ZiilAstInfo.LinkText = DictEntryValueAlsUnicode.WertString;
						}
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntryLastState;

					if (null != DictEntryValue)
					{
					}

					var DictEntryValueAlsFloat = DictEntryValue as SictAuswertPyObj32Float64Zuusctand;

					if (null != DictEntryValueAlsFloat)
					{
						ZiilAstInfo.LastStateFloat = DictEntryValueAlsFloat.WertFloat64;
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntryLastSetCapacitor;

					var DictEntryValueAlsFloat = DictEntryValue as SictAuswertPyObj32Float64Zuusctand;

					if (null != DictEntryValueAlsFloat)
					{
						ZiilAstInfo.LastSetCapacitorFloat = DictEntryValueAlsFloat.WertFloat64;
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntryLastValue;

					if (null != DictEntryValue)
					{
					}

					var DictEntryValueAlsFloat = DictEntryValue as SictAuswertPyObj32Float64Zuusctand;

					if (null != DictEntryValueAlsFloat)
					{
						ZiilAstInfo.LastValueFloat = DictEntryValueAlsFloat.WertFloat64;
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntryRotation;

					if (null != DictEntryValue)
					{
					}

					var DictEntryValueAlsFloat = DictEntryValue as SictAuswertPyObj32Float64Zuusctand;

					if (null != DictEntryValueAlsFloat)
					{
						ZiilAstInfo.RotationFloat = DictEntryValueAlsFloat.WertFloat64;
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntryMinimized;

					var DictEntryValueAlsBool = DictEntryValue as SictAuswertPyObj32BoolZuusctand;

					if (null != DictEntryValueAlsBool)
					{
						ZiilAstInfo.Minimized = DictEntryValueAlsBool.WertBool;
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntryIsModal;

					var DictEntryValueAlsBool = DictEntryValue as SictAuswertPyObj32BoolZuusctand;

					if (null != DictEntryValueAlsBool)
					{
						ZiilAstInfo.isModal = DictEntryValueAlsBool.WertBool;
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntryOverviewEntryIsSelected ?? GbsAst.DictEntryTreeViewEntryIsSelected;

					var DictEntryValueAlsBool = DictEntryValue as SictAuswertPyObj32BoolZuusctand;

					if (null != DictEntryValueAlsBool)
					{
						ZiilAstInfo.isSelected = DictEntryValueAlsBool.WertBool;
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntryTexturePath;

					var DictEntryValueAlsStr = DictEntryValue as SictAuswertPyObj32StrZuusctand;

					if (null != DictEntryValueAlsStr)
					{
						ZiilAstInfo.texturePath = DictEntryValueAlsStr.WertString;
					}
				}

				bool tBool;

				{
					//	Berecnung Color

					var GbsAstPyObjDictEntryColor = GbsAst.DictEntryColor as SictAuswertPyObj32ColorZuusctand;

					if (null != GbsAstPyObjDictEntryColor)
					{
						var GbsAstPyObjDictEntryColorDictObj = GbsAstPyObjDictEntryColor.DictObj;

						if (null != GbsAstPyObjDictEntryColorDictObj)
						{
							if (!(Zait <= GbsAstPyObjDictEntryColorDictObj.AktualisLezteZait))
							{
								GbsAstPyObjDictEntryColorDictObj.Aktualisiire(
									AusProzesLeeser,
									out tBool,
									Zait);
							}

							MemberAusDictEntryScteleSicerAktuel(
								GbsAstPyObjDictEntryColor,
								AusProzesLeeser,
								ProzesAuswertZuusctand,
								Zait,
								null,
								true);

							var ColorDictEntryA = GbsAstPyObjDictEntryColor.DictEntryA;
							var ColorDictEntryR = GbsAstPyObjDictEntryColor.DictEntryR;
							var ColorDictEntryG = GbsAstPyObjDictEntryColor.DictEntryG;
							var ColorDictEntryB = GbsAstPyObjDictEntryColor.DictEntryB;

							if (null != ColorDictEntryA ||
								null != ColorDictEntryR ||
								null != ColorDictEntryG ||
								null != ColorDictEntryB)
							{
								ZiilAstInfo.ColorAMili = (int?)WertMiliAusPyObj(ColorDictEntryA);
								ZiilAstInfo.ColorRMili = (int?)WertMiliAusPyObj(ColorDictEntryR);
								ZiilAstInfo.ColorGMili = (int?)WertMiliAusPyObj(ColorDictEntryG);
								ZiilAstInfo.ColorBMili = (int?)WertMiliAusPyObj(ColorDictEntryB);
							}
						}
					}
				}

				{
					var DictEntryValue = GbsAst.DictEntryBackgroundList;

					var DictEntryValueScpez = DictEntryValue as SictAuswertPyObj32PyOderUiChildrenList;

					if (null != DictEntryValueScpez)
					{
						DictEntryValueScpez.Aktualisiire(AusProzesLeeser, out tBool, Zait);

						var GbsAstDictEntryChildrenDict = DictEntryValueScpez.DictObj;

						if (null != GbsAstDictEntryChildrenDict)
						{
							GbsAstDictEntryChildrenDict.Aktualisiire(AusProzesLeeser, out tBool, Zait);
						}

						MemberAusDictEntryScteleSicerAktuel(
							DictEntryValueScpez,
							AusProzesLeeser,
							ProzesAuswertZuusctand,
							Zait,
							null,
							true);

						var DictEntryBackgroundListDictEntryListChildrenObj = DictEntryValueScpez.DictEntryListChildrenObj as SictAuswertPyObj32ListZuusctand;

						if (null != DictEntryBackgroundListDictEntryListChildrenObj)
						{
							DictEntryBackgroundListDictEntryListChildrenObj.ListeItemAnzaalScrankeMax = 0x10;

							DictEntryBackgroundListDictEntryListChildrenObj.Aktualisiire(AusProzesLeeser, out tBool, Zait);

							var BackgroundList = new List<GbsAstInfo>();

							foreach (var BackgroundAdr in DictEntryBackgroundListDictEntryListChildrenObj.ListeItemRef.EmptyIfNull())
							{
								var BackgroundObj = ObjektFürHerkunftAdreseErscteleOderAusScatescpaicer
										<SictAuswertPyObj32GbsAstZuusctand>(
										BackgroundAdr,
										AusProzesLeeser,
										ProzesAuswertZuusctand,
										Zait);

								BackgroundObj.Aktualisiire(AusProzesLeeser, out tBool, Zait);

								var BackgroundObjDictObj = BackgroundObj.DictObj;

								if (null == BackgroundObjDictObj)
								{
									continue;
								}

								BackgroundObjDictObj.Aktualisiire(AusProzesLeeser, out tBool, Zait);

								AstInfoScteleSicerAktuelOoneListeChild(BackgroundObj, AusProzesLeeser, ProzesAuswertZuusctand, Zait, null);

								BackgroundList.Add(BackgroundObj.AstInfo);
							}

							ZiilAstInfo.BackgroundList = BackgroundList.ToArray();
						}
					}
				}
			}
		}

		/// <summary>
		/// Funktioon arbaitet mit Annaame das Dict beraits aktualisiirt wurde.
		/// </summary>
		/// <param name="AusProzesLeeser"></param>
		public void MemberAusDictEntryScteleSicerAktuel(
			SictAuswertPyObj32MitBaiPlus8RefDictZuusctand pyObjMitDict,
			IMemoryReader AusProzesLeeser,
			Optimat.EveOnline.SictProzesAuswertZuusctand ProzesAuswertZuusctand,
			Int64 Zait,
			Int64? ObjektErhaltungBeginZaitScrankeMin = null,
			bool MemberAktualisiire = false)
		{
			if (null == AusProzesLeeser)
			{
				return;
			}

			if (null == pyObjMitDict)
			{
				return;
			}

			var VorherDictListeEntry = DictZuPyObjPropagatioonDictEntryNaacMemberLezteListeDictEntry.TryGetValueOrDefault(pyObjMitDict);

			var DictObj = pyObjMitDict.DictObj;

			var DictListeEntry = (null == DictObj) ? null : DictObj.ListeDictEntry;

			var DictListeEntryUnverändert = false;

			if (VorherDictListeEntry.Wert == DictListeEntry)
			{
				DictListeEntryUnverändert = true;
			}
			else
			{
				if (null != VorherDictListeEntry.Wert && null != DictListeEntry)
				{
					DictListeEntryUnverändert = DictListeEntry.SequenceEqual(VorherDictListeEntry.Wert);
				}
			}

			if (DictListeEntryUnverändert)
			{
				if (null != DictListeEntry)
				{
				}
			}

			bool AktualisiireSol = false;

			if (VorherDictListeEntry.Zait <= pyObjMitDict.VonDictEntryMemberAktualisatioonNootwendigLezteZait)
			{
				AktualisiireSol = true;
			}

			{
				/*
				 * 2013.11.09
				 * Test aktualisatioon unbedingt.
				 * */
				AktualisiireSol = true;
			}

			if (!DictListeEntryUnverändert || AktualisiireSol)
			{
				DictZuPyObjPropagatioonDictEntryNaacMemberLezteListeDictEntry[pyObjMitDict] = new WertZuZaitpunktStruct<SictPyDictEntry32[]>(
					DictListeEntry, Zait);

				ScraibeDictEntryNaacMember(
					pyObjMitDict,
					DictObj,
					AusProzesLeeser,
					ProzesAuswertZuusctand,
					Zait,
					ObjektErhaltungBeginZaitScrankeMin,
					MemberAktualisiire);
			}
		}

		static public IEnumerable<KeyValuePair<InGbsPfaad, GbsAstInfo[]>>
			MengeGbsAstSuuceNaacPfaad(IEnumerable<InGbsPfaad> MengeNaacGbsAstPfaad,
			int ProcessId,
			EveOnline.SictProzesAuswertZuusctand GbsSuuceWurzel)
		{
			var MengeNaacGbsAstPfaadArray = MengeNaacGbsAstPfaad?.ToArray();

			if (null == MengeNaacGbsAstPfaadArray)
			{
				return null;
			}

			if (null == GbsSuuceWurzel)
			{
				return null;
			}

			var SuuceListePfaadUndSuuceWurzelAdrese =
				MengeNaacGbsAstPfaadArray
				.Select((NaacGbsAstPfaad) => new KeyValuePair<InGbsPfaad, Int64?>(NaacGbsAstPfaad, NaacGbsAstPfaad.WurzelAstAdrese))
				.Where((PfaadUndSuuceWurzelAdrese) => PfaadUndSuuceWurzelAdrese.Value.HasValue)
				.Select((PfaadUndSuuceWurzelAdrese) => new KeyValuePair<InGbsPfaad, Int64>(PfaadUndSuuceWurzelAdrese.Key, PfaadUndSuuceWurzelAdrese.Value.Value))
				.ToList();

			var SuuceMengeWurzelAdrese =
				SuuceListePfaadUndSuuceWurzelAdrese
				.Select((PfaadUndSuuceWurzelAdrese) => PfaadUndSuuceWurzelAdrese.Value)
				.ToArray();

			var MengeAstSuuceFortsazHerkunftAdrese = new List<Int64>();

			foreach (var SuucePfaadUndSuuceWurzelAdrese in SuuceListePfaadUndSuuceWurzelAdrese)
			{
				var SuucePfaad = SuucePfaadUndSuuceWurzelAdrese.Key;

				if (null == SuucePfaad)
				{
					continue;
				}

				var PfaadListeAstAdrese = SuucePfaad.ListeAstAdrese;

				if (null == PfaadListeAstAdrese)
				{
					continue;
				}

				MengeAstSuuceFortsazHerkunftAdrese.AddRange(PfaadListeAstAdrese);
			}

			GbsAstInfo[] MengeGbsWurzelInfo = null;

			if (!SuuceMengeWurzelAdrese.IsNullOrEmpty())
			{
				var ScnapscusAuswert =
					new Optimat.EveOnline.SictProzesAuswertZuusctandScpezGbsBaum(
						new ProcessMemoryReader(ProcessId),
						GbsSuuceWurzel,
						1111,
						11111,
						null,
						SuuceMengeWurzelAdrese,
						MengeAstSuuceFortsazHerkunftAdrese);

				ScnapscusAuswert.BerecneScrit();

				MengeGbsWurzelInfo = ScnapscusAuswert.MengeGbsWurzelInfo;
			}

			var ListeGbsAstPfaadMitGbsAstBlatInfo = new KeyValuePair<InGbsPfaad, GbsAstInfo[]>[MengeNaacGbsAstPfaadArray.Length];

			for (int NaacGbsAstPfaadIndex = 0; NaacGbsAstPfaadIndex < MengeNaacGbsAstPfaadArray.Length; NaacGbsAstPfaadIndex++)
			{
				var NaacGbsAstPfaad = MengeNaacGbsAstPfaadArray[NaacGbsAstPfaadIndex];

				var ListeAstAdrese = NaacGbsAstPfaad.ListeAstAdrese;

				GbsAstInfo[] PfaadListeAst = null;

				try
				{
					var InNaacSuuceListeIndex =
						SuuceListePfaadUndSuuceWurzelAdrese.FindIndex((Kandidaat) => Kandidaat.Key == NaacGbsAstPfaad);

					var PfaadWurzel =
						(null == MengeGbsWurzelInfo) ? null :
						MengeGbsWurzelInfo.ElementAtOrDefault(InNaacSuuceListeIndex);

					if (null == PfaadWurzel)
					{
						continue;
					}

					if (ListeAstAdrese.IsNullOrEmpty())
					{
						PfaadListeAst = new GbsAstInfo[] { PfaadWurzel };
					}

					var PfaadBlatAdrese = ListeAstAdrese.LastOrDefault();

					PfaadListeAst =
						PfaadWurzel.SuuceFlacMengeAstMitPfaadFrüheste(KandidaatBlat => KandidaatBlat.PyObjAddress == PfaadBlatAdrese);
				}
				finally
				{
					ListeGbsAstPfaadMitGbsAstBlatInfo[NaacGbsAstPfaadIndex] = new KeyValuePair<InGbsPfaad, GbsAstInfo[]>(NaacGbsAstPfaad, PfaadListeAst);
				}
			}

			return ListeGbsAstPfaadMitGbsAstBlatInfo;
		}
	}

	public class SictZuMemberAusDictEntryInfo
	{
		readonly public System.Reflection.MemberInfo MemberInfo;
		readonly public Fasterflect.MemberSetter Setter;
		readonly public Fasterflect.MemberGetter Getter;

		public SictZuMemberAusDictEntryInfo(
			System.Reflection.MemberInfo MemberInfo,
			Fasterflect.MemberSetter Setter,
			Fasterflect.MemberGetter Getter)
		{
			this.MemberInfo = MemberInfo;
			this.Setter = Setter;
			this.Getter = Getter;
		}
	}
}
