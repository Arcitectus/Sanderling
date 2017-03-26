using System;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Sanderling.Interface.MemoryStruct;
using BotEngine.Common;
using System.Collections.Generic;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsAgr
	{
		/// <summary>
		/// 2014.07.29	Bsp:	"Version: 8.47.821895"
		/// </summary>
		static readonly string VersionLabelRegexPattern = Regex.Escape("Version:") + "\\s*([\\s\\d\\.]+)";

		readonly public UINodeInfoInTree GbsBaumWurzel;

		public UINodeInfoInTree AstLayerMenu
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstLayerMain
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstLayerModal
		{
			private set;
			get;
		}

		public UINodeInfoInTree LayerSystemmenuAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree LayerLoginAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree LayerSystemmenuSysmenuAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstLayerModalModal
		{
			private set;
			get;
		}

		public string VersionString
		{
			private set;
			get;
		}

		public SictAuswertGbsSystemMenu SystemmenuAuswert
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] AstLayerModalMengeKandidaatWindow
		{
			private set;
			get;
		}

		public UINodeInfoInTree LayerHintAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree ModuleButtonHintAst
		{
			private set;
			get;
		}

		public SictAuswertGbsModuleButtonTooltip ModuleButtonTooltipAuswert
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstLayerUtilmenu
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstLayerAbovemain
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstSidePanels
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] MengeKandidaatMenuAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] MengeKandidaatAbovemainMessageAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] MengeKandidaatAbovemainPanelEveMenuAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] MengeKandidaatAbovemainPanelGroupAst
		{
			private set;
			get;
		}

		public SictAuswertGbsPanelEveMenu[] MengeKandidaatAbovemainPanelEveMenuAuswert
		{
			private set;
			get;
		}

		public SictAuswertGbsPanelGroup[] MengeKandidaatAbovemainPanelGroupAuswert
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] LayerMainMengeKandidaatWindowAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] MengeKandidaatWindowAst
		{
			private set;
			get;
		}

		public Window[] MengeWindow
		{
			private set;
			get;
		}

		public IContainer[] Utilmenu
		{
			private set;
			get;
		}

		public UINodeInfoInTree LayerShipUiAst
		{
			private set;
			get;
		}

		public SictAuswertGbsShipUi LayerShipUiAstAuswert
		{
			private set;
			get;
		}

		public SictAuswertGbsLayerTarget AuswertLayerTarget
		{
			private set;
			get;
		}

		public SictAuswertGbsSidePanels AuswertSidePanels
		{
			private set;
			get;
		}

		public IMemoryMeasurement AuswertErgeebnis
		{
			private set;
			get;
		}

		public SictAuswertGbsAgr(
			UINodeInfoInTree GbsBaumWurzel)
		{
			this.GbsBaumWurzel = GbsBaumWurzel;
		}

		public void Berecne(int? sessionDurationRemaining)
		{
			if (null == GbsBaumWurzel)
			{
				return;
			}

			AstSidePanels =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				GbsBaumWurzel,
				(Kandidaat) => string.Equals("SidePanels", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase));

			LayerShipUiAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				GbsBaumWurzel,
				(Kandidaat) => string.Equals("ShipUI", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
				5, 1);

			var FensterLayerTarget =
				Optimat.EveOnline.AuswertGbs.Extension
				.FirstMatchingNodeFromSubtreeBreadthFirst(GbsBaumWurzel, (Kandidaat) => string.Equals("l_target", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase));

			var WindowOverviewAst =
				Optimat.EveOnline.AuswertGbs.Extension
				.FirstMatchingNodeFromSubtreeBreadthFirst(GbsBaumWurzel, (Kandidaat) => string.Equals("OverView", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase));

			AstLayerMenu =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				GbsBaumWurzel, (Kandidaat) =>
					string.Equals("LayerCore", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) &&
					string.Equals("l_menu", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			AstLayerMain =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				GbsBaumWurzel, (Kandidaat) =>
					string.Equals("LayerCore", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) &&
					string.Equals("l_main", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			AstLayerModal =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				GbsBaumWurzel, (Kandidaat) =>
					string.Equals("LayerCore", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) &&
					string.Equals("l_modal", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			LayerSystemmenuAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				GbsBaumWurzel, (Kandidaat) =>
					string.Equals("SystemMenu", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) &&
					string.Equals("l_systemmenu", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			LayerSystemmenuSysmenuAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				LayerSystemmenuAst, (Kandidaat) =>
					Kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("sysmenu", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			LayerLoginAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				GbsBaumWurzel, (Kandidaat) =>
					string.Equals("Login", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) &&
					string.Equals("l_login", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					3, 1);

			var InflightLayer =
				GbsBaumWurzel?.FirstMatchingNodeFromSubtreeBreadthFirst(c =>
					(c?.PyObjTypName?.RegexMatchSuccessIgnoreCase("Layer") ?? false) &&
					(c?.Name?.RegexMatchSuccessIgnoreCase("inflight") ?? false));

			var InflightBracketLayer =
				InflightLayer?.FirstMatchingNodeFromSubtreeBreadthFirst(c =>
					(c?.PyObjTypName?.RegexMatchSuccessIgnoreCase("Layer") ?? false) &&
					(c?.Name?.RegexMatchSuccessIgnoreCase("bracket") ?? false));

			var setInflightBracket =
				InflightBracketLayer
				?.MatchingNodesFromSubtreeBreadthFirst(c => c?.PyObjTypName?.RegexMatchSuccessIgnoreCase("InSpaceBracket") ?? false, null, null, null, true)
				?.Select(bracketNode => bracketNode?.AsInSpaceBracket())
				?.ToArrayIfNotEmpty();

			var LayerSystemmenuAstMengeKandidaatLabelVersionAst =
				Optimat.EveOnline.AuswertGbs.Extension.MatchingNodesFromSubtreeBreadthFirst(
				LayerSystemmenuAst, (Kandidaat) =>
					Optimat.EveOnline.AuswertGbs.Glob.GbsAstTypeIstLabel(Kandidaat), null, 3, 1)
				.ConcatNullable(
				Optimat.EveOnline.AuswertGbs.Extension.MatchingNodesFromSubtreeBreadthFirst(
				LayerLoginAst, (Kandidaat) =>
					Optimat.EveOnline.AuswertGbs.Glob.GbsAstTypeIstLabel(Kandidaat), null, 3, 1))
				?.ToArray();

			if (null != LayerSystemmenuAst)
			{
				SystemmenuAuswert = new SictAuswertGbsSystemMenu(LayerSystemmenuAst);

				SystemmenuAuswert.Berecne();
			}

			if (null != LayerSystemmenuAstMengeKandidaatLabelVersionAst)
			{
				foreach (var KandidaatLabelVersionAst in LayerSystemmenuAstMengeKandidaatLabelVersionAst)
				{
					var Text = KandidaatLabelVersionAst.LabelText();

					if (null == Text)
					{
						continue;
					}

					var Match = Regex.Match(Text ?? "", VersionLabelRegexPattern, RegexOptions.IgnoreCase);

					if (!Match.Success)
					{
						continue;
					}

					VersionString = Match.Groups[1].Value;
					break;
				}
			}

			AstLayerModalMengeKandidaatWindow =
				Optimat.EveOnline.AuswertGbs.Extension.MatchingNodesFromSubtreeBreadthFirst(
				AstLayerModal, (Kandidaat) =>
					Regex.Match(Kandidaat.PyObjTypName ?? "", "MessageBox", RegexOptions.IgnoreCase).Success ||
					Regex.Match(Kandidaat.PyObjTypName ?? "", "HybridWindow", RegexOptions.IgnoreCase).Success ||
					Regex.Match(Kandidaat.PyObjTypName ?? "", "PopupWnd", RegexOptions.IgnoreCase).Success,
					null, 3, 1);

			LayerHintAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				GbsBaumWurzel, (Kandidaat) =>
					string.Equals("LayerCore", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) &&
					string.Equals("l_hint", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			var SetTooltipNode =
				LayerHintAst?.MatchingNodesFromSubtreeBreadthFirst(k => k?.PyObjTypNameMatchesRegexPattern("TooltipGeneric") ?? false);

			var SetTooltip =
				SetTooltipNode?.Select(TooltipNode => TooltipNode?.AlsContainer())?.WhereNotDefault()?.ToArrayIfNotEmpty();

			ModuleButtonHintAst =
			   Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
			   GbsBaumWurzel, (Kandidaat) =>
				   string.Equals("ModuleButtonTooltip", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) &&
				   (true == Kandidaat.VisibleIncludingInheritance),
			   2, 1);

			AstLayerUtilmenu =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				GbsBaumWurzel, (Kandidaat) =>
					string.Equals("LayerCore", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) &&
					string.Equals("l_utilmenu", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			AstLayerAbovemain =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				GbsBaumWurzel, (Kandidaat) =>
					string.Equals("LayerCore", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) &&
					string.Equals("l_abovemain", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			MengeKandidaatAbovemainMessageAst =
				Optimat.EveOnline.AuswertGbs.Extension.MatchingNodesFromSubtreeBreadthFirst(
				AstLayerAbovemain, (Kandidaat) =>
					string.Equals("Message", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 3, 1);

			MengeKandidaatAbovemainPanelEveMenuAst =
				Optimat.EveOnline.AuswertGbs.Extension.MatchingNodesFromSubtreeBreadthFirst(
				AstLayerAbovemain, (Kandidaat) =>
					string.Equals("PanelEveMenu", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 3, 1);

			MengeKandidaatAbovemainPanelGroupAst =
				Optimat.EveOnline.AuswertGbs.Extension.MatchingNodesFromSubtreeBreadthFirst(
				AstLayerAbovemain, (Kandidaat) =>
					string.Equals("PanelGroup", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 3, 1);

			LayerMainMengeKandidaatWindowAst =
				Optimat.EveOnline.AuswertGbs.Extension.MatchingNodesFromSubtreeBreadthFirst(
				AstLayerMain, (Kandidaat) => true == Kandidaat.VisibleIncludingInheritance &&
					(null != Kandidaat.Caption || null != Kandidaat.WindowID),
				null,
				2,
				1,
				true);

			MengeKandidaatWindowAst =
				Bib3.Glob.ListeEnumerableAgregiirt(
				new UINodeInfoInTree[][]{
					LayerMainMengeKandidaatWindowAst,
					AstLayerModalMengeKandidaatWindow,
				})
				?.WhereNotDefault()
				?.ToArray();

			MengeWindow =
				MengeKandidaatWindowAst
				?.Select((WindowAst) => Optimat.EveOnline.AuswertGbs.Glob.WindowBerecneScpezTypFürGbsAst(WindowAst))
				?.WhereNotDefault()
				?.ToArray();

			MengeKandidaatMenuAst =
				Optimat.EveOnline.AuswertGbs.Extension.MatchingNodesFromSubtreeBreadthFirst(
				AstLayerMenu, (Kandidaat) =>
					Regex.Match(Kandidaat.PyObjTypName ?? "", "DropDownMenu", RegexOptions.IgnoreCase).Success,
					null, 2, 1);

			if (null != ModuleButtonHintAst)
			{
				ModuleButtonTooltipAuswert = new SictAuswertGbsModuleButtonTooltip(ModuleButtonHintAst);
				ModuleButtonTooltipAuswert.Berecne();
			}

			if (null != MengeKandidaatAbovemainPanelEveMenuAst)
			{
				MengeKandidaatAbovemainPanelEveMenuAuswert =
					MengeKandidaatAbovemainPanelEveMenuAst
					.Select((GbsAst) =>
						{
							var Auswert = new SictAuswertGbsPanelEveMenu(GbsAst);
							Auswert.Berecne();
							return Auswert;
						}).ToArray();
			}

			if (null != MengeKandidaatAbovemainPanelGroupAst)
			{
				MengeKandidaatAbovemainPanelGroupAuswert =
					MengeKandidaatAbovemainPanelGroupAst
					.Select((GbsAst) =>
						{
							var Auswert = new SictAuswertGbsPanelGroup(GbsAst);
							Auswert.Berecne();
							return Auswert;
						}).ToArray();
			}

			var MengeKandidaatUtilmenu =
				(null == AstLayerUtilmenu) ? null :
				Optimat.EveOnline.AuswertGbs.Extension.MatchingNodesFromSubtreeBreadthFirst(
				GbsBaumWurzel, (Kandidaat) =>
					(true == Kandidaat.VisibleIncludingInheritance) &&
					string.Equals("UtilMenu", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, null, 1);

			Utilmenu = new[] { AstLayerUtilmenu.AlsUtilmenu() }.WhereNotDefault()?.ToArrayIfNotEmpty();

			AuswertSidePanels = new SictAuswertGbsSidePanels(AstSidePanels);
			AuswertSidePanels.Berecne();

			LayerShipUiAstAuswert = new SictAuswertGbsShipUi(LayerShipUiAst);
			LayerShipUiAstAuswert.Berecne();

			AuswertLayerTarget = new SictAuswertGbsLayerTarget(FensterLayerTarget);
			AuswertLayerTarget.Berecne();

			var AuswertSidePanelsAuswertPanelCurrentSystem = AuswertSidePanels.AuswertPanelCurrentSystem;
			var AuswertSidePanelsAuswertPanelRoute = AuswertSidePanels.AuswertPanelRoute;
			var AuswertSidePanelsAstInfoPanelMissions = AuswertSidePanels.AuswertPanelMissions;

			var Ergeebnis = new MemoryMeasurement
			{
				SessionDurationRemaining = sessionDurationRemaining,
			};

			Ergeebnis.UserDefaultLocaleName = BotEngine.WinApi.Kernel32.GetUserDefaultLocaleName();

			if (null != SystemmenuAuswert)
			{
				Ergeebnis.SystemMenu = SystemmenuAuswert.Ergeebnis;
			}

			var AstLayerMainGrööse = AstLayerMain?.Grööse;

			if (AstLayerMainGrööse.HasValue)
			{
				Ergeebnis.ScreenSize = new Bib3.Geometrik.Vektor2DInt((int)AstLayerMainGrööse.Value.A, (int)AstLayerMainGrööse.Value.B);
			}

			if (null != ModuleButtonTooltipAuswert)
			{
				Ergeebnis.ModuleButtonTooltip = ModuleButtonTooltipAuswert.Ergeebnis;
			}

			Ergeebnis.Menu =
				MengeKandidaatMenuAst?.Select(SictAuswertGbsMenu.ReadMenu)?.WhereNotDefault()?.ToArrayIfNotEmpty();

			Ergeebnis.AbovemainMessage =
				MengeKandidaatAbovemainMessageAst
				?.Select((GbsAst) => GbsAst?.LargestLabelInSubtree()?.AsUIElementTextIfTextNotEmpty())
				?.Where(Label => 0 < Label?.Text?.Length)
				?.ToArrayIfNotEmpty();

			if (null != MengeKandidaatAbovemainPanelEveMenuAuswert)
			{
				Ergeebnis.AbovemainPanelEveMenu =
					MengeKandidaatAbovemainPanelEveMenuAuswert
					.Select((Kandidaat) => Kandidaat.Ergeebnis)
					.Where((Kandidaat) => null != Kandidaat)
					?.ToArrayIfNotEmpty();
			}

			if (null != MengeKandidaatAbovemainPanelGroupAuswert)
			{
				Ergeebnis.AbovemainPanelGroup =
					MengeKandidaatAbovemainPanelGroupAuswert
					.Select((Kandidaat) => Kandidaat.Ergeebnis)
					.Where((Kandidaat) => null != Kandidaat)
					?.ToArrayIfNotEmpty();
			}

			IWindowStation WindowStationLobby = null;
			IWindowOverview WindowOverview = null;
			IWindowInventory[] MengeWindowInventory = null;

			if (null != MengeWindow)
			{
				var MengeWindowStack =
					MengeWindow
					.OfType<WindowStack>()
					.ToArray();

				var MengeWindowStackWindow =
					MengeWindowStack
					.Select((WindowStack) => WindowStack.TabSelectedWindow)
					.Where((Window) => null != Window)
					.ToArray();

				var MengeWindowMitAusWindowStackWindow =
					MengeWindow
					.ConcatNullable(MengeWindowStackWindow)
					?.ToArrayIfNotEmpty();

				var MengeWindowOverView =
					MengeWindowMitAusWindowStackWindow?.OfType<IWindowOverview>().ToArrayIfNotEmpty();

				var MengeWindowChatChannel =
					MengeWindowMitAusWindowStackWindow?.OfType<WindowChatChannel>().ToArrayIfNotEmpty();

				var MengeWindowSelectedItemView =
					MengeWindowMitAusWindowStackWindow?.OfType<IWindowSelectedItemView>().ToArrayIfNotEmpty();

				var MengeWindowPeopleAndPlaces =
					MengeWindowMitAusWindowStackWindow?.OfType<WindowPeopleAndPlaces>().ToArrayIfNotEmpty();

				var MengeWindowDroneView =
					MengeWindowMitAusWindowStackWindow?.OfType<IWindowDroneView>().ToArrayIfNotEmpty();

				var MengeWindowFittingWindow =
					MengeWindowMitAusWindowStackWindow?.OfType<WindowShipFitting>().ToArrayIfNotEmpty();

				var MengeWindowFittingMgmt =
					MengeWindowMitAusWindowStackWindow?.OfType<WindowFittingMgmt>().ToArrayIfNotEmpty();

				var MengeWindowStationLobby =
					MengeWindowMitAusWindowStackWindow?.OfType<IWindowStation>().ToArrayIfNotEmpty();

				var MengeWindowSurveyScanView =
					MengeWindowMitAusWindowStackWindow?.OfType<IWindowSurveyScanView>().ToArrayIfNotEmpty();

				MengeWindowInventory =
					MengeWindowMitAusWindowStackWindow?.OfType<IWindowInventory>().ToArrayIfNotEmpty();

				var MengeWindowAgentDialogue =
					MengeWindowMitAusWindowStackWindow?.OfType<WindowAgentDialogue>().ToArrayIfNotEmpty();

				var MengeWindowAgentBrowser =
					MengeWindowMitAusWindowStackWindow?.OfType<WindowAgentBrowser>().ToArrayIfNotEmpty();

				var MengeWindowTelecom =
					MengeWindowMitAusWindowStackWindow?.OfType<WindowTelecom>().ToArrayIfNotEmpty();

				var MengeWindowRegionalMarket =
					MengeWindowMitAusWindowStackWindow?.OfType<WindowRegionalMarket>().ToArrayIfNotEmpty();

				var MengeWindowMarketAction =
					MengeWindowMitAusWindowStackWindow?.OfType<WindowMarketAction>().ToArrayIfNotEmpty();

				var MengeWindowItemSell =
					MengeWindowMitAusWindowStackWindow?.OfType<WindowItemSell>().ToArrayIfNotEmpty();

				Ergeebnis.WindowStack = MengeWindowStack.ToArrayIfNotEmpty();

				WindowOverview = MengeWindowOverView?.FirstOrDefault();
				WindowStationLobby = MengeWindowStationLobby?.FirstOrDefault();

				Ergeebnis.WindowOverview = MengeWindowOverView.ToArrayIfNotEmpty();
				Ergeebnis.WindowChatChannel = MengeWindowChatChannel.ToArrayIfNotEmpty();
				Ergeebnis.WindowSelectedItemView = MengeWindowSelectedItemView.ToArrayIfNotEmpty();
				Ergeebnis.WindowPeopleAndPlaces = MengeWindowPeopleAndPlaces.ToArrayIfNotEmpty();
				Ergeebnis.WindowDroneView = MengeWindowDroneView.ToArrayIfNotEmpty();
				Ergeebnis.WindowShipFitting = MengeWindowFittingWindow.ToArrayIfNotEmpty();
				Ergeebnis.WindowFittingMgmt = MengeWindowFittingMgmt.ToArrayIfNotEmpty();
				Ergeebnis.WindowStation = MengeWindowStationLobby.ToArrayIfNotEmpty();
				Ergeebnis.WindowSurveyScanView = MengeWindowSurveyScanView.ToArrayIfNotEmpty();

				Ergeebnis.WindowInventory = MengeWindowInventory;
				Ergeebnis.WindowAgentDialogue = MengeWindowAgentDialogue;
				Ergeebnis.WindowAgentBrowser = MengeWindowAgentBrowser;

				Ergeebnis.WindowTelecom = MengeWindowTelecom;

				Ergeebnis.WindowRegionalMarket = MengeWindowRegionalMarket;
				Ergeebnis.WindowMarketAction = MengeWindowMarketAction;
				Ergeebnis.WindowItemSell = MengeWindowItemSell;

				Ergeebnis.WindowProbeScanner = MengeWindowMitAusWindowStackWindow?.OfType<IWindowProbeScanner>().ToArrayIfNotEmpty();

				var MengeWindowSonstige =
					MengeWindow
					.Except(
					new IEnumerable<IWindow>[]{
						MengeWindowStack,
						MengeWindowOverView,
						MengeWindowChatChannel,
						MengeWindowPeopleAndPlaces,
						MengeWindowSelectedItemView,
						MengeWindowSurveyScanView,
						MengeWindowDroneView,
						MengeWindowFittingWindow,
						MengeWindowFittingMgmt,
						MengeWindowStationLobby,
						MengeWindowInventory,
						MengeWindowAgentDialogue,
						MengeWindowAgentBrowser,
						MengeWindowTelecom,
						MengeWindowRegionalMarket,
						MengeWindowMarketAction,
						MengeWindowItemSell,
						Ergeebnis.WindowProbeScanner,
					}.ConcatNullable())
					.ToArrayIfNotEmpty();

				Ergeebnis.WindowOther = MengeWindowSonstige;
			}

			Ergeebnis.VersionString = VersionString;

			Ergeebnis.Neocom = AuswertSidePanels?.Neocom;

			Ergeebnis.InfoPanelCurrentSystem = AuswertSidePanelsAuswertPanelCurrentSystem?.ErgeebnisScpez;
			Ergeebnis.InfoPanelRoute = AuswertSidePanelsAuswertPanelRoute?.ErgeebnisScpez;
			Ergeebnis.InfoPanelMissions = AuswertSidePanelsAstInfoPanelMissions?.ErgeebnisScpez;

			var InfoPanelButtonCurrentSystem = AuswertSidePanels.InfoPanelButtonLocationInfoAst.AsUIElementIfVisible();
			var InfoPanelButtonRoute = AuswertSidePanels.InfoPanelButtonRouteAst.AsUIElementIfVisible();
			var InfoPanelButtonMissions = AuswertSidePanels.InfoPanelButtonMissionAst.AsUIElementIfVisible();

			Ergeebnis.InfoPanelButtonCurrentSystem = InfoPanelButtonCurrentSystem;
			Ergeebnis.InfoPanelButtonRoute = InfoPanelButtonRoute;
			Ergeebnis.InfoPanelButtonMissions = InfoPanelButtonMissions;
			Ergeebnis.InfoPanelButtonIncursions = AuswertSidePanels?.InfoPanelButtonIncursionsAst?.AsUIElementIfVisible();

			Ergeebnis.Utilmenu = Utilmenu;

			Ergeebnis.ShipUi = LayerShipUiAstAuswert.Ergeebnis;
			Ergeebnis.Target = AuswertLayerTarget?.SetTarget?.ToArrayIfNotEmpty();

			Ergeebnis.Tooltip = SetTooltip;

			Ergeebnis.InflightBracket = setInflightBracket;

			this.AuswertErgeebnis = Ergeebnis;
		}
	}
}
