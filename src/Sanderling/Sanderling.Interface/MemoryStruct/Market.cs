namespace Sanderling.Interface.MemoryStruct
{
	public class MarketItemTypeDetails : UIElement
	{
		public MarketItemTypeDetailsMarketData MarketData;

		public MarketItemTypeDetails()
		{
		}

		public MarketItemTypeDetails(IUIElement @base)
			: base(@base)
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

		public MarketItemTypeDetailsMarketData(IUIElement @base)
			: base(@base)
		{
		}
	}

	public class WindowRegionalMarket : Window
	{
		public TabGroup LeftTabGroup;

		public IListViewAndControl QuickbarView;

		public TabGroup RightTabGroup;

		public MarketItemTypeDetails SelectedItemTypeDetails;

		public MarketMyOrders MyOrders;

		public WindowRegionalMarket(IWindow @base)
			:
			base(@base)
		{
		}

		public WindowRegionalMarket()
		{
		}
	}

	public class MarketMyOrders : Container
	{
		public IListViewAndControl SellOrderView;

		public IListViewAndControl BuyOrderView;

		public MarketMyOrders(IContainer @base)
			:
			base(@base)
		{
		}

		public MarketMyOrders()
		{
		}
	}

	public class MarketOrderEntry : ListEntry
	{
		public MarketOrderEntry(IListEntry @base)
			:
			base(@base)
		{
		}

		public MarketOrderEntry()
		{
		}
	}

	public class WindowMarketAction : Window
	{
		public WindowMarketAction(IWindow @base)
			:
			base(@base)
		{
		}

		public WindowMarketAction()
		{
		}
	}

	public class WindowItemSell : Window
	{
		public WindowItemSell(IWindow @base)
			:
			base(@base)
		{
		}

		public WindowItemSell()
		{
		}
	}

}
