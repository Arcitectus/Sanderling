namespace Sanderling.Interface.MemoryStruct
{
	public class MarketItemTypeDetails : UIElement
	{
		public MarketItemTypeDetailsMarketData MarketData;

		public MarketItemTypeDetails()
		{
		}

		public MarketItemTypeDetails(IUIElement Base)
			: base(Base)
		{
		}
	}

	public class MarketItemTypeDetailsMarketData : UIElement
	{
		public IListViewAndControl SellerView;

		public IListViewAndControl BuyerView;

		public MarketItemTypeDetailsMarketData()
		{
		}

		public MarketItemTypeDetailsMarketData(IUIElement Base)
			: base(Base)
		{
		}
	}

	public class WindowRegionalMarket : Window
	{
		public TabGroup LeftTabGroup;

		public IListViewAndControl QuickbarView;

		public TabGroup RightTabGroup;

		public MarketItemTypeDetails SelectedItemTypeDetails;

		public WindowRegionalMarket(IWindow Base)
			:
			base(Base)
		{
		}

		public WindowRegionalMarket()
		{
		}
	}

	public class MarketOrderEntry : ListEntry
	{
		public MarketOrderEntry(IListEntry Base)
			:
			base(Base)
		{
		}

		public MarketOrderEntry()
		{
		}
	}

	public class WindowMarketAction : Window
	{
		public WindowMarketAction(IWindow Base)
			:
			base(Base)
		{
		}

		public WindowMarketAction()
		{
		}
	}

	public class WindowItemSell : Window
	{
		public WindowItemSell(IWindow Base)
			:
			base(Base)
		{
		}

		public WindowItemSell()
		{
		}
	}

}
