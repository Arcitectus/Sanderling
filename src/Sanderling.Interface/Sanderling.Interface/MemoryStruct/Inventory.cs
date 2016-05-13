namespace Sanderling.Interface.MemoryStruct
{
	public interface IInventory : IUIElement
	{
		/// <summary>
		/// this contains the items in the inventory if the view is set to "list".
		/// </summary>
		IListViewAndControl ListView { get; }
    }

	public class Inventory : UIElement, IInventory
	{
		public IListViewAndControl ListView { set; get; }

		public Inventory()
			:
			this(null)
		{
		}

		public Inventory(IUIElement @base)
			:
			base(@base)
		{
		}
	}

	public interface IWindowInventory : IWindow
	{
		ITreeViewEntry[] LeftTreeListEntry { get; }

		IScroll LeftTreeViewportScroll { get; }

		/// <summary>
		/// 2015.09.01:
		/// "<url=localsvc:service=inv&method=OnBreadcrumbTextClicked&linkNum=0&windowID1=InventorySpace&windowID2=None><color=#55FFFFFF>ShipName (ShipType) > </color></url>Drone Bay"
		/// </summary>
		IUIElementText SelectedRightInventoryPathLabel { get; }

		IInventory SelectedRightInventory { get; }

		IUIElementText SelectedRightInventoryCapacity { get; }

		/// <summary>
		/// control how the items are displayed.
		/// ("Icons"/"Details"/"List")
		/// </summary>
		ISprite[] SelectedRightControlViewButton { get; }

		IUIElementInputText SelectedRightFilterTextBox { get; }

		IUIElement SelectedRightFilterButtonClear { get; }
	}

	public class WindowInventory : Window, IWindowInventory
	{
		public ITreeViewEntry[] LeftTreeListEntry { set; get; }

		public IScroll LeftTreeViewportScroll { set; get; }

		public IUIElementText SelectedRightInventoryPathLabel { set; get; }

		public IInventory SelectedRightInventory { set; get; }

		public IUIElementText SelectedRightInventoryCapacity { set; get; }

		public ISprite[] SelectedRightControlViewButton { set; get; }

		public IUIElementInputText SelectedRightFilterTextBox { set; get; }

		public IUIElement SelectedRightFilterButtonClear { set; get; }

		public int? SelectedRightItemDisplayedCount { set; get; }

		public int? SelectedRightItemFilteredCount { set; get; }

		public WindowInventory()
			:
			this(null)
		{
		}

		public WindowInventory(IWindow @base)
			:
			base(@base)
		{
		}
	}
}
