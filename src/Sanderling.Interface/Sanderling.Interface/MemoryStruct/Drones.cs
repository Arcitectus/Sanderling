using System.Collections.Generic;
using System.Linq;

namespace Sanderling.Interface.MemoryStruct
{
	public class WindowDroneView : Window
	{
		public ListViewAndControl ListView;

		public WindowDroneView(Window Base)
			:
			base(Base)
		{
		}

		public WindowDroneView()
		{
		}
	}

	/// <summary>
	/// Name of the drone and status/quantity can be found in the label, e.g.:
	/// "Hobgoblin I ( <color=0xFF00FF00>Idle</color> )"
	/// </summary>
	public class DroneViewEntryItem : DroneViewEntry
	{
		public ShipHitpointsAndEnergy Hitpoints;

		public DroneViewEntryItem(ListEntry Base)
			:
			base(Base)
		{
		}

		public DroneViewEntryItem()
		{
		}
	}


	public class DroneViewEntryGroup : DroneViewEntry
	{
		public UIElementText Caption;

		public DroneViewEntryGroup(ListEntry Base)
			:
			base(Base)
		{
		}

		public DroneViewEntryGroup()
		{
		}
	}

	public class DroneViewEntry : ListEntry
	{
		public DroneViewEntry(ListEntry Base)
			:
			base(Base)
		{
		}

		public DroneViewEntry()
		{
		}
	}

}
