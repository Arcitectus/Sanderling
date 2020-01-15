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

	public interface IWindowDroneView : IWindow
	{
		IListViewAndControl ListView { get; }
	}

	public class WindowDroneView : Window, IWindowDroneView
	{
		public IListViewAndControl ListView { set; get; }

		public WindowDroneView(IWindow @base)
			:
			base(@base)
		{
		}

		public WindowDroneView()
		{
		}
	}

	public class DroneViewEntryItem : DroneViewEntry, IDroneViewEntryItem
	{
		public IShipHitpointsAndEnergy Hitpoints { set; get; }

		public DroneViewEntryItem(IListEntry @base)
			:
			base(@base)
		{
		}

		public DroneViewEntryItem()
		{
		}
	}


	public class DroneViewEntryGroup : DroneViewEntry
	{
		public IUIElementText Caption;

		public DroneViewEntryGroup(IListEntry @base)
			:
			base(@base)
		{
		}

		public DroneViewEntryGroup()
		{
		}
	}

	public class DroneViewEntry : ListEntry
	{
		public DroneViewEntry(IListEntry @base)
			:
			base(@base)
		{
		}

		public DroneViewEntry()
		{
		}
	}

}
