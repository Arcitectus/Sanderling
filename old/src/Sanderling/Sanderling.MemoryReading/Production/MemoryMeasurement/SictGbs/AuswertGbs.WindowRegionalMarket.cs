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
			UINodeInfoInTree windowAst)
		{
			if (null == windowAst)
				return null;

			var WindowAuswert = new SictAuswertGbsWindowRegionalMarket(windowAst);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisScpez;
		}

		public WindowRegionalMarket ErgeebnisScpez;

		public SictAuswertGbsWindowRegionalMarket(UINodeInfoInTree windowNode)
			:
			base(windowNode)
		{
		}

		static public MarketOrderEntry MarketOrderEntryKonstrukt(
			UINodeInfoInTree entryAst,
			IColumnHeader[] listeScrollHeader,
			RectInt? regionConstraint)
		{
			if (!(entryAst?.VisibleIncludingInheritance ?? false))
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
				WindowNode.ListPathToNodeFromSubtreeBreadthFirst(k => Regex.Match(k.LabelText() ?? "", "iron charge L", RegexOptions.IgnoreCase).Success);

			var ListePfaadZuEntryInDetailsSellers =
				WindowNode.ListPathToNodeFromSubtreeBreadthFirst(k => Regex.Match(k.LabelText() ?? "", "motsu VII - Moon 6", RegexOptions.IgnoreCase).Success);

			var ListePfaadZuEntryInDetailsBuyers =
				WindowNode.ListPathToNodeFromSubtreeBreadthFirst(k => Regex.Match(k.LabelText() ?? "", "Moon 10 - CONCORD", RegexOptions.IgnoreCase).Success);


			var MengeTabControlAst =
				WindowNode?.MatchingNodesFromSubtreeBreadthFirst(k => Regex.Match(k.PyObjTypName ?? "", "TabGroup", RegexOptions.IgnoreCase).Success)?.ToArray();

			var MengeScrollAst =
				WindowNode?.MatchingNodesFromSubtreeBreadthFirst(k => k.PyObjTypNameIsScroll())?.ToArray();

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
				AstMainContainerMain?.FirstMatchingNodeFromSubtreeBreadthFirst(k => k.PyObjTypNameIsContainer() && Regex.Match(k.Name ?? "", "details", RegexOptions.IgnoreCase).Success);

			var ReczDetailsMarketDataContainerAst =
				ReczDetailsContainerAst?.FirstMatchingNodeFromSubtreeBreadthFirst(k => Regex.Match(k.PyObjTypName ?? "", "MarketData", RegexOptions.IgnoreCase).Success);

			var SellersViewportAst =
				ReczDetailsMarketDataContainerAst?.FirstMatchingNodeFromSubtreeBreadthFirst(k => k.PyObjTypNameIsScroll() && Regex.Match(k.Name ?? "", "buy", RegexOptions.IgnoreCase).Success);

			var BuyersViewportAst =
				ReczDetailsMarketDataContainerAst?.FirstMatchingNodeFromSubtreeBreadthFirst(k => k.PyObjTypNameIsScroll() && Regex.Match(k.Name ?? "", "sell", RegexOptions.IgnoreCase).Success);

			var setOrdersNode =
				AstMainContainerMain?.MatchingNodesFromSubtreeBreadthFirst(k => Regex.Match(k.PyObjTypName ?? "", "MarketOrder", RegexOptions.IgnoreCase).Success);

			var MyOrdersAst =
				setOrdersNode?.FirstOrDefault(node => !(node?.Name?.RegexMatchSuccessIgnoreCase("corp") ?? false));

			var MyOrdersSellingScrollNode =
				MyOrdersAst?.FirstMatchingNodeFromSubtreeBreadthFirst(k => k.PyObjTypNameIsScroll() && k.NameMatchesRegexPatternIgnoreCase("sell"));

			var MyOrdersBuyingScrollNode =
				MyOrdersAst?.FirstMatchingNodeFromSubtreeBreadthFirst(k => k.PyObjTypNameIsScroll() && k.NameMatchesRegexPatternIgnoreCase("buy"));

			var LinxTabGroupAuswert = new SictAuswertGbsTabGroup(LinxTabControlAst);
			var ReczTabGroupAuswert = new SictAuswertGbsTabGroup(ReczTabControlAst);

			LinxTabGroupAuswert.Berecne();
			ReczTabGroupAuswert.Berecne();

			var QuickbarScrollAuswert = new SictAuswertGbsListViewport<IListEntry>(QuickbarViewportAst, SictAuswertGbsListViewport<IListEntry>.ListEntryKonstruktSctandard);
			QuickbarScrollAuswert.Read();

			var DetailsMarketDataSellersScrollAuswert = new SictAuswertGbsListViewport<IListEntry>(SellersViewportAst, SictAuswertGbsWindowRegionalMarket.MarketOrderEntryKonstrukt);
			var DetailsMarketDataBuyersScrollAuswert = new SictAuswertGbsListViewport<IListEntry>(BuyersViewportAst, SictAuswertGbsWindowRegionalMarket.MarketOrderEntryKonstrukt);

			DetailsMarketDataSellersScrollAuswert.Read();
			DetailsMarketDataBuyersScrollAuswert.Read();

			var DetailsUIElement = ReczDetailsContainerAst.AsUIElementIfVisible();
			var MarketDataUIElement = ReczDetailsMarketDataContainerAst.AsUIElementIfVisible();

			var MyOrdersContainer = MyOrdersAst.AlsContainer();

			var MyOrdersSellingScrollAuswert = new SictAuswertGbsListViewport<IListEntry>(MyOrdersSellingScrollNode, SictAuswertGbsWindowRegionalMarket.MarketOrderEntryKonstrukt);
			var MyOrdersBuyingScrollAuswert = new SictAuswertGbsListViewport<IListEntry>(MyOrdersBuyingScrollNode, SictAuswertGbsWindowRegionalMarket.MarketOrderEntryKonstrukt);

			MyOrdersSellingScrollAuswert.Read();
			MyOrdersBuyingScrollAuswert.Read();

			var SelectedItemTypeDetailsMarketData = null == MarketDataUIElement ? null : new MarketItemTypeDetailsMarketData(MarketDataUIElement)
			{
				SellerView = DetailsMarketDataSellersScrollAuswert?.Result,
				BuyerView = DetailsMarketDataBuyersScrollAuswert?.Result,
			};

			var MyOrders = null == MyOrdersContainer ? null :
				new MarketMyOrders(MyOrdersContainer)
				{
					SellOrderView = MyOrdersSellingScrollAuswert?.Result,
					BuyOrderView = MyOrdersBuyingScrollAuswert?.Result,
				};

			var SelectedItemTypeDetails = null == DetailsUIElement ? null : new MarketItemTypeDetails(DetailsUIElement) { MarketData = SelectedItemTypeDetailsMarketData };

			this.ErgeebnisScpez = new WindowRegionalMarket(BaseErgeebnis)
			{
				LeftTabGroup = LinxTabGroupAuswert?.Ergeebnis,
				RightTabGroup = ReczTabGroupAuswert?.Ergeebnis,

				QuickbarView = QuickbarScrollAuswert?.Result,
				SelectedItemTypeDetails = SelectedItemTypeDetails,

				MyOrders = MyOrders,
			};
		}
	}
}
