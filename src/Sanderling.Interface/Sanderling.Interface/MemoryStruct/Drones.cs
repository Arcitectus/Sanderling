using System.Collections.Generic;
using System.Linq;

namespace Sanderling.Interface.MemoryStruct
{
	/// <summary>
	/// Name of the drone and status/quantity can be found in the label, e.g.:
	/// "Hobgoblin I ( <color=0xFF00FF00>Idle</color> )"
	/// </summary>
	public interface IDroneViewEntryItem : IListEntry
	{
		IShipHitpointsAndEnergy Hitpoints { get; }
	}

	public interface IWindowDroneView
	{
		IListViewAndControl ListView { get; }
	}

	public class WindowDroneView : Window, IWindowDroneView
	{
		public IListViewAndControl ListView { set; get; }

		public WindowDroneView(IWindow Base)
			:
			base(Base)
		{
		}

		public WindowDroneView()
		{
		}
	}

	public class DroneViewEntryItem : DroneViewEntry, IDroneViewEntryItem
	{
		public IShipHitpointsAndEnergy Hitpoints { set; get; }

		public DroneViewEntryItem(IListEntry Base)
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
		public IUIElementText Caption;

		public DroneViewEntryGroup(IListEntry Base)
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
		public DroneViewEntry(IListEntry Base)
			:
			base(Base)
		{
		}

		public DroneViewEntry()
		{
		}
	}

}
