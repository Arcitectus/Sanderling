namespace Sanderling.Interface.MemoryStruct
{
	public class MarketItemTypeDetails : UIElement
	{
		public MarketItemTypeDetailsMarketData MarketData;

		public MarketItemTypeDetails()
		{
		}

		public MarketItemTypeDetails(UIElement Base)
			: base(Base)
		{
		}
	}

	public class MarketItemTypeDetailsMarketData : UIElement
	{
		public ListViewAndControl SellerView;

		public ListViewAndControl BuyerView;

		public MarketItemTypeDetailsMarketData()
		{
		}

		public MarketItemTypeDetailsMarketData(UIElement Base)
			: base(Base)
		{
		}
	}

	public class WindowRegionalMarket : Window
	{
		public TabGroup LeftTabGroup;

		public ListViewAndControl QuickbarView;

		public TabGroup RightTabGroup;

		public MarketItemTypeDetails SelectedItemTypeDetails;

		public WindowRegionalMarket(Window Base)
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
		public MarketOrderEntry(ListEntry Base)
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
		public WindowMarketAction(Window Base)
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
		public WindowItemSell(Window Base)
			:
			base(Base)
		{
		}

		public WindowItemSell()
		{
		}
	}

}
