using System;

namespace Sanderling.Interface.MemoryStruct
{
	public class Inventory : UIElement
	{
		/// <summary>
		/// this contains the items in the inventory if the view is set to "list".
		/// </summary>
		public ListViewAndControl ListView;

		public Inventory()
			:
			this(null)
		{
		}

		public Inventory(UIElement Base)
			:
			base(Base)
		{
		}
	}

	public class WindowInventory : Window
	{
		public TreeViewEntry[] LeftTreeListEntry;

		public Scroll LeftTreeViewportScroll;

		/// <summary>
		/// 2015.09.01:
		/// "<url=localsvc:service=inv&method=OnBreadcrumbTextClicked&linkNum=0&windowID1=InventorySpace&windowID2=None><color=#55FFFFFF>ShipName (ShipType) > </color></url>Drone Bay"
		/// </summary>
		public UIElementText SelectedRightInventoryPathLabel;

		public Inventory SelectedRightInventory;

		public UIElementText SelectedRightInventoryCapacity;

		/// <summary>
		/// control how the items are displayed.
		/// ("Icons"/"Details"/"List")
		/// </summary>
		public Sprite[] SelectedRightControlViewButton;

		public UIElementInputText SelectedRightFilterTextBox;

		public UIElement SelectedRightFilterButtonClear;

		public int? SelectedRightItemDisplayedCount;

		public int? SelectedRightItemFilteredCount;

		public WindowInventory()
			:
			this(null)
		{
		}

		public WindowInventory(Window Base)
			:
			base(Base)
		{
		}
	}

}
