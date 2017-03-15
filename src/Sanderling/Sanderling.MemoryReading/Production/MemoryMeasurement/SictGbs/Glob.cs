using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using BotEngine;
using Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;

namespace Optimat.EveOnline.AuswertGbs
{
	public interface IAusGbsAstExtraktor
	{
		IUIElement Extrakt(SictGbsAstInfoSictAuswert gbsAst);
	}

	static public class Glob
	{
		static public Regex RegexGbsAstPyObjTypNameContainer = new Regex("Container", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		static public Regex RegexGbsAstPyObjTypNameIcon = new Regex("Icon", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		static public Regex RegexGbsAstPyObjTypNameButton = new Regex("Button|StationServiceBtn", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		static public bool PyObjTypNameMatchesRegex(
			this GbsAstInfo node,
			Regex regex)
		{
			var PyObjTypName = node?.PyObjTypName;

			if (null == PyObjTypName)
				return false;

			return regex?.Match(PyObjTypName)?.Success ?? false;
		}

		static public bool PyObjTypNameMatchesRegexPattern(
			this GbsAstInfo uiNode,
			string regexPattern,
			RegexOptions regexOptions = RegexOptions.None) =>
			uiNode.PyObjTypNameMatchesRegex(regexPattern.AlsRegex(regexOptions));

		static public bool PyObjTypNameMatchesRegexPatternIgnoreCase(
			this GbsAstInfo uiNode,
			string regexPattern,
			RegexOptions regexOptions = RegexOptions.None) =>
			uiNode.PyObjTypNameMatchesRegexPattern(regexPattern, RegexOptions.IgnoreCase | regexOptions);

		static public bool PyObjTypNameIsContainer(
			this GbsAstInfo uiNode) =>
			PyObjTypNameMatchesRegex(uiNode, RegexGbsAstPyObjTypNameContainer);

		static public bool PyObjTypNameIsIcon(
			this GbsAstInfo uiNode) =>
			PyObjTypNameMatchesRegex(uiNode, RegexGbsAstPyObjTypNameIcon);

		static public bool PyObjTypNameIsButton(
			this GbsAstInfo uiNode) =>
			PyObjTypNameMatchesRegex(uiNode, RegexGbsAstPyObjTypNameButton);

		/// <summary>
		/// Examples: "Sprite", "GlowSprite"
		/// </summary>
		static Regex PyTypeNameSpriteRegex = "sprite".AlsRegexIgnoreCaseCompiled();

		static public bool PyObjTypNameIsSprite(
			this GbsAstInfo uiNode) =>
			uiNode.PyObjTypNameMatchesRegex(PyTypeNameSpriteRegex);

		/// <summary>
		/// 2015.07.10
		/// Chat.Channel"userlist": PyObjTypName = "BasicDynamicScroll"
		/// </summary>
		/// <param name="uiNode"></param>
		/// <returns></returns>
		static public bool PyObjTypNameIsScroll(
			this SictGbsAstInfoSictAuswert uiNode) =>
			string.Equals("BasicDynamicScroll".ToLowerInvariant(), uiNode?.PyObjTypName?.ToLowerInvariant()) ||
			string.Equals("Scroll".ToLowerInvariant(), uiNode?.PyObjTypName?.ToLowerInvariant());

		static public bool PyObjTypNameEqualsIgnoreCase(
			this SictGbsAstInfoSictAuswert uiNode,
			string typeName) =>
			string.Equals(typeName, uiNode?.PyObjTypName, StringComparison.InvariantCultureIgnoreCase);

		static public bool NameEqualsIgnoreCase(
			this SictGbsAstInfoSictAuswert uiNode,
			string name) =>
			string.Equals(name, uiNode?.Name, StringComparison.InvariantCultureIgnoreCase);

		static public bool NameMatchesRegex(
			this SictGbsAstInfoSictAuswert uiNode,
			Regex regex) =>
			regex.Match(uiNode?.Name ?? "").Success;

		static public bool NameMatchesRegexPattern(
			this SictGbsAstInfoSictAuswert uiNode,
			string regexPattern,
			RegexOptions regexOptions = RegexOptions.None) =>
			uiNode.NameMatchesRegex(new Regex(regexPattern, regexOptions));

		static public bool NameMatchesRegexPatternIgnoreCase(
			this SictGbsAstInfoSictAuswert uiNode,
			string regexPattern,
			RegexOptions regexOptions = RegexOptions.None) =>
			uiNode.NameMatchesRegexPattern(regexPattern, RegexOptions.IgnoreCase | regexOptions);

		static public bool GbsAstTypeIstEveIcon(SictGbsAstInfoSictAuswert uiNode) =>
			string.Equals("icon", uiNode?.PyObjTypName, StringComparison.InvariantCultureIgnoreCase);

		static public bool GbsAstTypeIstSprite(SictGbsAstInfoSictAuswert uiNode)
		{
			var PyObjTypName = uiNode?.PyObjTypName;

			if (PyObjTypName.IsNullOrEmpty())
				return false;

			return Regex.Match(PyObjTypName, "Sprite", RegexOptions.IgnoreCase).Success;
		}

		static public bool GbsAstTypeIstLabel(this SictGbsAstInfoSictAuswert node)
		{
			if (null == (node?.Text ?? node?.SetText ?? node?.LinkText))
				return false;   //	2014.09.07	was kaine Text enthalt werd nit als Label klasifiziirt (in Raster wääre es in Scnapscus nit als Label erkenbar).

			var PyObjTypName = node?.PyObjTypName;

			if (null == PyObjTypName)
				return false;

			var Match = Regex.Match(PyObjTypName, "label", RegexOptions.IgnoreCase);

			return Match.Success;
		}

		static public bool GbsAstTypeIstEveLabel(this SictGbsAstInfoSictAuswert uiNode) =>
			uiNode?.PyObjTypName?.StartsWith("EveLabel", StringComparison.InvariantCultureIgnoreCase) ?? false;

		static public bool GbsAstTypeIstEveCaption(SictGbsAstInfoSictAuswert gbsAst) =>
			gbsAst?.PyObjTypName?.StartsWith("EveCaption", StringComparison.InvariantCultureIgnoreCase) ?? false;

		static public ObjectIdInt64 VonGbsAstObjektMitBezaicnerInt(SictGbsAstInfoSictAuswert gbsAst)
		{
			return new ObjectIdInt64(gbsAst?.HerkunftAdrese ?? -1);
		}

		static public RectInt? FläceAusGbsAstInfoMitVonParentErbe(
			SictGbsAstInfoSictAuswert gbsAstInfo)
		{
			if (null == gbsAstInfo)
				return null;

			var Grööse = gbsAstInfo.Grööse;
			var MiteLaage = gbsAstInfo.LaagePlusVonParentErbeLaage() + (Grööse * 0.5);

			if (!Grööse.HasValue || !MiteLaage.HasValue)
				return null;

			return RectInt.FromCenterAndSize(
				MiteLaage.Value.AlsVektor2DInt(),
				Grööse.Value.AlsVektor2DInt());
		}

		static public KeyValuePair<Func<SictGbsAstInfoSictAuswert, Window>, string[]>[]
			MengeZuFunktioonWindowAuswertMengeStringWindowTypFilter =
			new KeyValuePair<Func<SictGbsAstInfoSictAuswert, Window>, string[]>[]{

					new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
						SictAuswertGbsMessageBox.BerecneFürWindowAst,   new string[]{   "MessageBox"}),

					new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
						SictAuswertGbsHybridWindow.BerecneFürWindowAst, new string[]{   "HybridWindow"}),

				//	2014.07.27	Beobact verwexlung mit: "PyObjTypName": "SovereigntyOverviewWnd"
				//	2014.10.26	Beobact verwexlung mit: "PyObjTypName":	"OverviewSettings"
				//	2014.10.26	Beobact verwexlung mit: "PyObjTypName":	"ImportOverviewWindow"
				new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
						//	2014.10.26	SictAuswertGbsWindowOverview.BerecneFürWindowAst,	new	string[]{	"(?<!Sov.*)OverView"}),
						SictAuswertGbsWindowOverview.BerecneFürWindowAst,   new string[]{   "(?<!Sov.*)(?<!Imp.*)OverView(?!.*Set)"}),

				new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
					SictAuswertGbsWindowSelectedItem.BerecneFürWindowAst,   new string[]{   "selecteditemview", "ActiveItem"}),

				new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
					SictAuswertGbsWindowInventoryPrimary.BerecneFürWindowAst,   new string[]{   "Inventory", "ShipCargo", "StationItem", "StationShip",}),

				new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
					SictAuswertGbsWindowStation.BerecneFürWindowAst,   new string[]{   "Lobby"}),

				new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
					SictAuswertGbsWindowAgentDialogue.BerecneFürWindowAst,  new string[]{   "AgentDialogueWindow"}),

				new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
					SictAuswertGbsWindowStack.BerecneFürWindowAst,  new string[]{   "WindowStack"}),

				new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
					SictAuswertGbsWindowDroneView.BerecneFürWindowAst,  new string[]{   "DroneView"}),

				new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
					SictAuswertGbsWindowPeopleAndPlaces.BerecneFürWindowAst, new string[]{   "AddressBookWindow"}),

				new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
					SictAuswertGbsWindowSurveyScanView.BerecneFürWindowAst, new string[]{   "SurveyScanView"}),

				new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
					SictAuswertGbsWindowShipFitting.BerecneFürWindowAst,  new string[]{   "FittingWindow"}),

				new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
					SictAuswertGbsWindowFittingMgmt.BerecneFürWindowAst,    new string[]{   "FittingMgmt"}),

				new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
					SictAuswertGbsWindowTelecom.BerecneFürWindowAst,    new string[]{   "Telecom"}),

				//	ChatWindow Stack
				new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
					SictAuswertGbsWindowStack.BerecneFürWindowAst,    new string[]{   "LSCStack"}),

				new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
					SictAuswertGbsWindowChatChannel.BerecneFürWindowAst,    new string[]{   "Channel"}),

				new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
					SictAuswertGbsWindowRegionalMarket.BerecneFürWindowAst,    new string[]{   "RegionalMarket"}),

				new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
					SictAuswertGbsWindowMarketAction.BerecneFürWindowAst,    new string[]{   "MarketActionWindow"}),

				new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
					SictAuswertGbsWindowItemSell.BerecneFürWindowAst,    new string[]{   "SellItems"}),

				new KeyValuePair<Func<SictGbsAstInfoSictAuswert,    Window>,    string[]>(
					SictAuswertGbsWindowProbeScanner.BerecneFürWindowAst, new string[]{   "ProbeScannerWindow"}),
		};

		static public Window WindowBerecneScpezTypFürGbsAst(
			SictGbsAstInfoSictAuswert kandidaatWindowNode)
		{
			var KandidaatPyObjTypName = kandidaatWindowNode?.PyObjTypName;

			if (null == KandidaatPyObjTypName)
				return null;

			foreach (var zuFunktioonWindowAuswertMengeStringWindowTypFilter in MengeZuFunktioonWindowAuswertMengeStringWindowTypFilter)
			{
				if (!(zuFunktioonWindowAuswertMengeStringWindowTypFilter.Value?.Any((stringWindowTypFilter) => Regex.Match(KandidaatPyObjTypName, stringWindowTypFilter, RegexOptions.IgnoreCase).Success) ?? false))
					continue;

				return zuFunktioonWindowAuswertMengeStringWindowTypFilter.Key?.Invoke(kandidaatWindowNode);
			}

			return SictAuswertGbsWindow.BerecneFürWindowAst(kandidaatWindowNode);
		}
	}
}
