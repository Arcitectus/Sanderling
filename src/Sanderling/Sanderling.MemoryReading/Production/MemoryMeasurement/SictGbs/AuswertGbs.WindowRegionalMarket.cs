using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Sanderling.Interface.MemoryStruct;
using BotEngine.Common;
using Bib3.Geometrik;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindowRegionalMarket : SictAuswertGbsWindow
	{
		new static public WindowRegionalMarket BerecneFürWindowAst(
			SictGbsAstInfoSictAuswert windowAst)
		{
			if (null == windowAst)
				return null;

			var WindowAuswert = new SictAuswertGbsWindowRegionalMarket(windowAst);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisScpez;
		}

		public WindowRegionalMarket ErgeebnisScpez;

		public SictAuswertGbsWindowRegionalMarket(SictGbsAstInfoSictAuswert windowNode)
			:
			base(windowNode)
		{
		}

		static public MarketOrderEntry MarketOrderEntryKonstrukt(
			SictGbsAstInfoSictAuswert entryAst,
			IColumnHeader[] listeScrollHeader,
			RectInt? regionConstraint)
		{
			if (!(entryAst?.SictbarMitErbe ?? false))
				return null;

			var ChildTransitive = entryAst.MengeChildAstTransitiiveHüle()?.ToArray();

			var ListEntryAuswert = new SictAuswertGbsListEntry(entryAst, listeScrollHeader, regionConstraint, ListEntryTrenungZeleTypEnum.InLabelTab);

			ListEntryAuswert.Berecne();

			var ListEntry = ListEntryAuswert.ErgeebnisListEntry;

			if (null == ListEntry)
				return null;

			return new MarketOrderEntry(ListEntry);
		}

		override public void Berecne()
		{
			base.Berecne();

			var BaseErgeebnis = base.Ergeebnis;

			if (null == BaseErgeebnis)
				return;

			var ListePfaadZuEntryInQuickbar =
				WindowNode.SuuceFlacMengeAstMitPfaad(k => Regex.Match(k.LabelText() ?? "", "iron charge L", RegexOptions.IgnoreCase).Success);

			var ListePfaadZuEntryInDetailsSellers =
				WindowNode.SuuceFlacMengeAstMitPfaad(k => Regex.Match(k.LabelText() ?? "", "motsu VII - Moon 6", RegexOptions.IgnoreCase).Success);

			var ListePfaadZuEntryInDetailsBuyers =
				WindowNode.SuuceFlacMengeAstMitPfaad(k => Regex.Match(k.LabelText() ?? "", "Moon 10 - CONCORD", RegexOptions.IgnoreCase).Success);


			var MengeTabControlAst =
				WindowNode?.SuuceFlacMengeAst(k => Regex.Match(k.PyObjTypName ?? "", "TabGroup", RegexOptions.IgnoreCase).Success)?.ToArray();

			var MengeScrollAst =
				WindowNode?.SuuceFlacMengeAst(k => k.PyObjTypNameIsScroll())?.ToArray();

			var LinxTabControlAst =
				MengeTabControlAst
				?.OrderBy(k => k.LaagePlusVonParentErbeLaageA())
				?.FirstOrDefault();

			var ReczTabControlAst =
				MengeTabControlAst
				?.Except(LinxTabControlAst.Yield())
				?.OrderByDescending(k => k.LaagePlusVonParentErbeLaageA())
				?.FirstOrDefault();

			var QuickbarViewportAst =
				MengeScrollAst
				?.OrderBy(k => k.LaagePlusVonParentErbeLaageA())
				?.FirstOrDefault();

			var ReczDetailsContainerAst =
				AstMainContainerMain?.SuuceFlacMengeAstFrüheste(k => k.PyObjTypNameIsContainer() && Regex.Match(k.Name ?? "", "details", RegexOptions.IgnoreCase).Success);

			var ReczDetailsMarketDataContainerAst =
				ReczDetailsContainerAst?.SuuceFlacMengeAstFrüheste(k => Regex.Match(k.PyObjTypName ?? "", "MarketData", RegexOptions.IgnoreCase).Success);

			var SellersViewportAst =
				ReczDetailsMarketDataContainerAst?.SuuceFlacMengeAstFrüheste(k => k.PyObjTypNameIsScroll() && Regex.Match(k.Name ?? "", "buy", RegexOptions.IgnoreCase).Success);

			var BuyersViewportAst =
				ReczDetailsMarketDataContainerAst?.SuuceFlacMengeAstFrüheste(k => k.PyObjTypNameIsScroll() && Regex.Match(k.Name ?? "", "sell", RegexOptions.IgnoreCase).Success);

			var setOrdersNode =
				AstMainContainerMain?.SuuceFlacMengeAst(k => Regex.Match(k.PyObjTypName ?? "", "MarketOrder", RegexOptions.IgnoreCase).Success);

			var MyOrdersAst =
				setOrdersNode?.FirstOrDefault(node => !(node?.Name?.RegexMatchSuccessIgnoreCase("corp") ?? false));

			var MyOrdersSellingScrollNode =
				MyOrdersAst?.SuuceFlacMengeAstFrüheste(k => k.PyObjTypNameIsScroll() && k.NameMatchesRegexPatternIgnoreCase("sell"));

			var MyOrdersBuyingScrollNode =
				MyOrdersAst?.SuuceFlacMengeAstFrüheste(k => k.PyObjTypNameIsScroll() && k.NameMatchesRegexPatternIgnoreCase("buy"));

			var LinxTabGroupAuswert = new SictAuswertGbsTabGroup(LinxTabControlAst);
			var ReczTabGroupAuswert = new SictAuswertGbsTabGroup(ReczTabControlAst);

			LinxTabGroupAuswert.Berecne();
			ReczTabGroupAuswert.Berecne();

			var QuickbarScrollAuswert = new SictAuswertGbsListViewport<IListEntry>(QuickbarViewportAst, SictAuswertGbsListViewport<IListEntry>.ListEntryKonstruktSctandard);
			QuickbarScrollAuswert.Berecne();

			var DetailsMarketDataSellersScrollAuswert = new SictAuswertGbsListViewport<IListEntry>(SellersViewportAst, SictAuswertGbsWindowRegionalMarket.MarketOrderEntryKonstrukt);
			var DetailsMarketDataBuyersScrollAuswert = new SictAuswertGbsListViewport<IListEntry>(BuyersViewportAst, SictAuswertGbsWindowRegionalMarket.MarketOrderEntryKonstrukt);

			DetailsMarketDataSellersScrollAuswert.Berecne();
			DetailsMarketDataBuyersScrollAuswert.Berecne();

			var DetailsUIElement = ReczDetailsContainerAst.AlsUIElementFalsUnglaicNullUndSictbar();
			var MarketDataUIElement = ReczDetailsMarketDataContainerAst.AlsUIElementFalsUnglaicNullUndSictbar();

			var MyOrdersContainer = MyOrdersAst.AlsContainer();

			var MyOrdersSellingScrollAuswert = new SictAuswertGbsListViewport<IListEntry>(MyOrdersSellingScrollNode, SictAuswertGbsWindowRegionalMarket.MarketOrderEntryKonstrukt);
			var MyOrdersBuyingScrollAuswert = new SictAuswertGbsListViewport<IListEntry>(MyOrdersBuyingScrollNode, SictAuswertGbsWindowRegionalMarket.MarketOrderEntryKonstrukt);

			MyOrdersSellingScrollAuswert.Berecne();
			MyOrdersBuyingScrollAuswert.Berecne();

			var SelectedItemTypeDetailsMarketData = null == MarketDataUIElement ? null : new MarketItemTypeDetailsMarketData(MarketDataUIElement)
			{
				SellerView = DetailsMarketDataSellersScrollAuswert?.Ergeebnis,
				BuyerView = DetailsMarketDataBuyersScrollAuswert?.Ergeebnis,
			};

			var MyOrders = null == MyOrdersContainer ? null :
				new MarketMyOrders(MyOrdersContainer)
				{
					SellOrderView = MyOrdersSellingScrollAuswert?.Ergeebnis,
					BuyOrderView = MyOrdersBuyingScrollAuswert?.Ergeebnis,
				};

			var SelectedItemTypeDetails = null == DetailsUIElement ? null : new MarketItemTypeDetails(DetailsUIElement) { MarketData = SelectedItemTypeDetailsMarketData };

			this.ErgeebnisScpez = new WindowRegionalMarket(BaseErgeebnis)
			{
				LeftTabGroup = LinxTabGroupAuswert?.Ergeebnis,
				RightTabGroup = ReczTabGroupAuswert?.Ergeebnis,

				QuickbarView = QuickbarScrollAuswert?.Ergeebnis,
				SelectedItemTypeDetails = SelectedItemTypeDetails,

				MyOrders = MyOrders,
			};
		}
	}
}
