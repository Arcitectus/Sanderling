using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Sanderling.Interface.MemoryStruct;
using BotEngine.Common;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindowInventoryPrimary : SictAuswertGbsWindow
	{
		new static public WindowInventory BerecneFürWindowAst(
			SictGbsAstInfoSictAuswert windowAst)
		{
			if (null == windowAst)
				return null;

			var WindowAuswert = new SictAuswertGbsWindowInventoryPrimary(windowAst);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisScpez;
		}

		public SictGbsAstInfoSictAuswert AstMainContainerMainDividerCont
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert LinxScrollContainerTreeAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert LinxTreeBehältnisAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert[] LinxTreeBehältnisListeEntryAst
		{
			private set;
			get;
		}

		public SictAuswertTreeViewEntry[] LinxTreeBehältnisListeEntryAuswert
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstMainContainerMainRightCont
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstMainContainerMainRightContTopRight1
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstMainContainerMainRightContTopRight1SubCaptionCont
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstMainContainerMainRightContTopRight1SubCaptionLabel
		{
			private set;
			get;
		}

		public string AuswaalReczObjektPfaadSictString
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AuswaalReczInventorySictAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert[] AuswaalReczInventorySictMengeButtonAst
		{
			private set;
			get;
		}

		public IUIElement[] AuswaalReczInventorySictMengeButton
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AuswaalReczInventoryAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AuswaalReczTop2Ast
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AuswaalReczCapacityGaugeAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AuswaalReczCapacityGaugeLabelAst
		{
			private set;
			get;
		}

		public string AuswaalReczCapacityGaugeLabelText
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AuswaalReczFilterEditAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AuswaalReczFilterEditButtonClearAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AuswaalReczFilterEditAingaabeTextZiilAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AuswaalReczFilterEditLabelAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert RightContBottomAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert RightContNumItemsLabelAst
		{
			private set;
			get;
		}

		public string RightContNumItemsLabelText
		{
			private set;
			get;
		}

		public SictAuswertGbsInventory AuswaalReczInventoryAuswert
		{
			private set;
			get;
		}

		public int? AuswaalReczMengeItemAbgebildetAnzaal
		{
			private set;
			get;
		}

		public int? AuswaalReczMengeItemFilteredAnzaal
		{
			private set;
			get;
		}

		public WindowInventory ErgeebnisScpez
		{
			private set;
			get;
		}

		public SictAuswertGbsWindowInventoryPrimary(SictGbsAstInfoSictAuswert astFensterInventoryPrimary)
			:
			base(astFensterInventoryPrimary)
		{
		}

		/// <summary>
		/// 2014.05.07	Bsp:
		/// "9 (61 filtered) Items"
		/// </summary>
		static public string BottomRightNumItemsLabelTextRegexPattern = "(\\d+)\\s*(|\\([^\\)]+\\))\\s*Items";

		static public int? AusBottomRightNumItemsLabelTextExtraktItemAnzaal(
			string bottomRightNumItemsLabelText,
			out int? filteredAnzaal)
		{
			filteredAnzaal = null;

			if (null == bottomRightNumItemsLabelText)
				return null;

			var Match = Regex.Match(bottomRightNumItemsLabelText, BottomRightNumItemsLabelTextRegexPattern, RegexOptions.IgnoreCase);

			if (!Match.Success)
				return null;

			var ItemAnzaal = int.Parse(Match.Groups[1].Value.Trim());

			var GrupeFilteredString = Match.Groups[2].Value;

			if (null != GrupeFilteredString)
			{
				if (0 < GrupeFilteredString.Length)
				{
					var GrupeFilteredAnzaalMatch = Regex.Match(GrupeFilteredString, "(\\d+)\\s*filtered", RegexOptions.IgnoreCase);

					if (!GrupeFilteredAnzaalMatch.Success)
						return null;

					filteredAnzaal = int.Parse(GrupeFilteredAnzaalMatch.Groups[1].Value);
				}
			}

			return ItemAnzaal;
		}

		static public string[] AuswaalReczObjektPfaadListeAstBerecneAusPfaadSictString(string pfaadSictString) =>
			pfaadSictString
			?.Split(new string[] { ">" }, StringSplitOptions.RemoveEmptyEntries)
			?.Select(ast => ast.Trim())
			?.Where(ast => !ast.IsNullOrEmpty())
			?.ToArray();

		override public void Berecne()
		{
			base.Berecne();

			if (null == base.Ergeebnis)
				return;

			AstMainContainerMainDividerCont =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMainContainerMain, (kandidaat) => "dividerCont".EqualsIgnoreCase(kandidaat.Name), 2, 1);

			/*
			 * 2013.10.20
			 * Scainbar mit Patch 2013.10.20 Rubicon Änderung von "ScrollContainerCore" naac "ScrollContainer"
			 * */
			LinxScrollContainerTreeAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMainContainerMainDividerCont, (kandidaat) =>
					("ScrollContainerCore".EqualsIgnoreCase(kandidaat.PyObjTypName) ||
					"ScrollContainer".EqualsIgnoreCase(kandidaat.PyObjTypName)) &&
					"tree".EqualsIgnoreCase(kandidaat.Name),
					3, 1);

			LinxTreeBehältnisAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				LinxScrollContainerTreeAst, (kandidaat) =>
					"ContainerAutoSize".EqualsIgnoreCase(kandidaat.PyObjTypName) &&
					"mainCont".EqualsIgnoreCase(kandidaat.Name),
					3, 1);

			/*
			 * 2014.06.21
			 * 
			 * Anpasung an 2014.07.21 Crius:
			 * Sictung Ast mit PyObjTypName = "TreeViewEntryInventory"
			 * 
			LinxTreeBehältnisListeEntryAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAst(
				LinxTreeBehältnisAst, (Kandidaat) => string.Equals("TreeViewEntry", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
				null, 2, 1, true);
			 * */

			LinxTreeBehältnisListeEntryAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAst(
				LinxTreeBehältnisAst, (kandidaat) =>
					Regex.Match(kandidaat.PyObjTypName ?? "", "TreeViewEntry", RegexOptions.IgnoreCase).Success,
				null, 2, 1, true);

			var LinxTreeContainerScrollAst =
				LinxScrollContainerTreeAst;

			//	2015.09.07:	Name = "clipCont"
			var ClipperAst =
				LinxTreeContainerScrollAst
				?.SuuceFlacMengeAstFrüheste(k => k.PyObjTypNameIsContainer() && k.NameMatchesRegexPatternIgnoreCase("clip"));

			//	2015.09.07:	Name = "handleCont"
			var ScrollHandleBoundAst =
				LinxTreeContainerScrollAst
				?.SuuceFlacMengeAstFrüheste(k => k.PyObjTypNameIsContainer() && k.NameMatchesRegexPatternIgnoreCase("handle"));

			//	2015.09.07:	PyObjTypName = "ScrollHandle"
			var ScrollHandleAst =
				LinxTreeContainerScrollAst
				?.SuuceFlacMengeAstFrüheste(k => k.PyObjTypNameMatchesRegexPatternIgnoreCase("Scrollhandle"));

			var LinxTreeViewScroll =
				((ClipperAst?.SictbarMitErbe ?? false) ||
				(ScrollHandleBoundAst?.SictbarMitErbe ?? false) ||
				(ScrollHandleAst?.SictbarMitErbe ?? false)) ?
				new Scroll(LinxTreeContainerScrollAst.AlsUIElementFalsUnglaicNullUndSictbar())
				{
					Clipper = ClipperAst?.AlsUIElementFalsUnglaicNullUndSictbar(),
					ScrollHandleBound = ScrollHandleBoundAst?.AlsUIElementFalsUnglaicNullUndSictbar(),
					ScrollHandle = ScrollHandleAst?.AlsUIElementFalsUnglaicNullUndSictbar(),
				} : null;

			LinxTreeBehältnisListeEntryAuswert =
				LinxTreeBehältnisListeEntryAst
				?.Select((ast) =>
					{
						var Auswert = new SictAuswertTreeViewEntry(ast);
						Auswert.Berecne();
						return Auswert;
					}).ToArray();

			AstMainContainerMainRightCont =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMainContainerMain, (kandidaat) => string.Equals("rightCont", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			AstMainContainerMainRightContTopRight1 =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMainContainerMainRightCont, (kandidaat) => string.Equals("topRightcont1", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			AstMainContainerMainRightContTopRight1SubCaptionCont =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMainContainerMainRightContTopRight1, (kandidaat) => string.Equals("subCaptionCont", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 3, 1);

			AstMainContainerMainRightContTopRight1SubCaptionLabel =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMainContainerMainRightContTopRight1SubCaptionCont, (kandidaat) => string.Equals("Label", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 3, 1);

			AuswaalReczInventorySictAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMainContainerMainRightCont, (kandidaat) =>
					string.Equals("InvContViewBtns", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase) ||
					Regex.Match(kandidaat.PyObjTypName ?? "", "InvContViewBtns", RegexOptions.IgnoreCase).Success,
					6, 1);

			AuswaalReczInventorySictMengeButtonAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAst(
				AuswaalReczInventorySictAst,
				(kandidaat) => string.Equals("ButtonIcon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 3, 1, true);

			AuswaalReczInventorySictMengeButton =
				AuswaalReczInventorySictMengeButtonAst
				?.Select((ast) => ast.AlsUIElementFalsUnglaicNullUndSictbar())
				?.ToArray();

			var SelectedRightControlViewButton =
				AuswaalReczInventorySictMengeButtonAst
				?.Select(ast => ast?.MengeChildAstTransitiiveHüle()?.OfType<SictGbsAstInfoSictAuswert>().GröösteSpriteAst()?.AlsSprite())
				?.WhereNotDefault()
				?.OrdnungLabel()
				?.ToArray();

			AuswaalReczInventoryAst =
				AstMainContainerMainRightCont?.SuuceFlacMengeAst(c =>
				c != AstMainContainerMainRightCont && null != c?.SuuceFlacMengeAstFrüheste(candidateScroll => candidateScroll?.PyObjTypNameIsScroll() ?? false, 3))
				?.GröösteAst();

			AuswaalReczTop2Ast =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMainContainerMainRightCont, (kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("topRightCont2", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					1, 1);

			AuswaalReczCapacityGaugeAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AuswaalReczTop2Ast, (kandidaat) =>
					string.Equals("InvContCapacityGauge", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
					1, 1);

			AuswaalReczCapacityGaugeLabelAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AuswaalReczCapacityGaugeAst, (kandidaat) =>
					string.Equals("EveLabelSmall", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) &&
					string.Equals("capacityText", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					1, 1);

			AuswaalReczFilterEditAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AuswaalReczTop2Ast, (kandidaat) =>
					string.Equals("SinglelineEdit", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
					3, 1);

			AuswaalReczFilterEditButtonClearAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AuswaalReczFilterEditAst, (kandidaat) =>
					string.Equals("ButtonIcon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
					3, 1);

			AuswaalReczFilterEditLabelAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AuswaalReczFilterEditAst, (kandidaat) =>
					string.Equals("edittext", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase) &&
					AuswertGbs.Glob.GbsAstTypeIstLabel(kandidaat),
					3, 1);

			AuswaalReczFilterEditAingaabeTextZiilAst =
				(null == AuswaalReczFilterEditLabelAst) ? null :
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AuswaalReczFilterEditAst, (kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					null != Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrühesteMitHerkunftAdrese(kandidaat, AuswaalReczFilterEditLabelAst.HerkunftAdrese, 3),
					3, 1);

			RightContBottomAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMainContainerMainRightCont, (kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("bottomRightcont", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					3, 1);

			var RightContBottomMengeLabelAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAst(
				RightContBottomAst,
				(kandidaat) => AuswertGbs.Glob.GbsAstTypeIstLabel(kandidaat) && true == kandidaat.SictbarMitErbe);

			if (null != RightContBottomMengeLabelAst)
			{
				foreach (var RightContBottomLabelAst in RightContBottomMengeLabelAst)
				{
					var LabelText = RightContBottomLabelAst.LabelText();

					if (null == LabelText)
						continue;

					var LabelTextMiinusXmlTag = LabelText.RemoveXmlTag();

					int? FilteredAnzaal;

					var ItemAnzaal = AusBottomRightNumItemsLabelTextExtraktItemAnzaal(LabelTextMiinusXmlTag, out FilteredAnzaal);

					if (ItemAnzaal.HasValue)
					{
						RightContNumItemsLabelAst = RightContBottomLabelAst;
						RightContNumItemsLabelText = LabelText;

						AuswaalReczMengeItemAbgebildetAnzaal = ItemAnzaal;
						AuswaalReczMengeItemFilteredAnzaal = FilteredAnzaal;
					}
				}
			}

			if (null != AuswaalReczCapacityGaugeLabelAst)
				AuswaalReczCapacityGaugeLabelText = AuswaalReczCapacityGaugeLabelAst.LabelText();

			if (null != AuswaalReczInventoryAst)
			{
				AuswaalReczInventoryAuswert = new SictAuswertGbsInventory(AuswaalReczInventoryAst);
				AuswaalReczInventoryAuswert.Berecne();
			}

			if (null != AstMainContainerMainRightContTopRight1SubCaptionLabel)
				AuswaalReczObjektPfaadSictString = AstMainContainerMainRightContTopRight1SubCaptionLabel.SetText.RemoveXmlTag();

			var LinxTreeListeEntry =
				LinxTreeBehältnisListeEntryAuswert
				?.Select(auswert => auswert.Ergeebnis)
				?.WhereNotDefault()
				?.ToArray();

			var AuswaalReczInventory = AuswaalReczInventoryAuswert?.Ergeebnis;

			var AuswaalReczFilterAingaabeTextZiil =
				AuswaalReczFilterEditAingaabeTextZiilAst.AlsUIElementFalsUnglaicNullUndSictbar();

			var AuswaalReczFilterButtonClear =
				AuswaalReczFilterEditButtonClearAst.AlsUIElementFalsUnglaicNullUndSictbar();

			var AuswaalReczFilterText =
				(null == AuswaalReczFilterEditLabelAst) ? null :
				AuswaalReczFilterEditLabelAst.LabelText();

			var AuswaalReczObjektPfaadListeAst = AuswaalReczObjektPfaadListeAstBerecneAusPfaadSictString(AuswaalReczObjektPfaadSictString);

			var AuswaalReczFilterTextBox = null == AuswaalReczFilterAingaabeTextZiil ? null : new UIElementInputText(AuswaalReczFilterAingaabeTextZiil) { Text = AuswaalReczFilterText };

			ErgeebnisScpez = new WindowInventory(base.Ergeebnis)
			{
				LeftTreeListEntry = LinxTreeListeEntry,
				LeftTreeViewportScroll = LinxTreeViewScroll,
				SelectedRightInventoryPathLabel = AstMainContainerMainRightContTopRight1SubCaptionLabel.AsUIElementTextIfTextNotEmpty(),
				SelectedRightInventory = AuswaalReczInventory,
				SelectedRightInventoryCapacity = AuswaalReczCapacityGaugeAst?.ExtraktMengeLabelString()?.FirstOrDefault(),
				SelectedRightControlViewButton = SelectedRightControlViewButton,
				SelectedRightFilterTextBox = AuswaalReczFilterTextBox,
				SelectedRightFilterButtonClear = AuswaalReczFilterButtonClear,
				SelectedRightItemDisplayedCount = AuswaalReczMengeItemAbgebildetAnzaal,
				SelectedRightItemFilteredCount = AuswaalReczMengeItemFilteredAnzaal,
			};
		}
	}
}
