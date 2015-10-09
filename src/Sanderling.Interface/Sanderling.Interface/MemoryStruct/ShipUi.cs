using System;

namespace Sanderling.Interface.MemoryStruct
{

	public class ShipUi : UIElement, ICloneable
	{
		public UIElement Center;

		public ShipUiIndication Indication;

		public ShipHitpointsAndEnergy Hitpoints;

		public UIElementText SpeedLabel;

		public ShipUiEWarElement[] EWarElement;

		public UIElement ButtonSpeed0;

		public UIElement ButtonSpeedMax;

		public ShipUiModule[] Module;

		public UIElementText[] Readout;

		public Int64? SpeedMilli;

		public ShipUiTimer[] Timer;

		public ShipUi()
		{
		}

		public ShipUi(UIElement Base)
			:
			base(Base)
		{
		}

		public ShipUi Copy() => this.CopyByPolicyMemoryMeasurement();

		public object Clone()
		{
			return Copy();
		}
	}

	public class ShipUiTimer : UIElement
	{
		public string Name;

		public UIElementText[] Label;

		public ShipUiTimer()
		{
		}

		public ShipUiTimer(UIElement Base)
			:
			base(Base)
		{
		}
	}

	public class ShipUiTarget : UIElement
	{
		public UIElementText[] LabelText;

		public bool? Active;

		public ShipHitpointsAndEnergy Hitpoints;

		/// <summary>
		/// click here to activate (input focus) or open a menu.
		/// </summary>
		public UIElement RegionInteraction;

		/// <summary>
		/// e.g. groups of modules or drones assigned to this target.
		/// </summary>
		public ShipUiTargetAssignedGroup[] Assigned;

		public ShipUiTarget()
			:
			this(null)
		{
		}

		public ShipUiTarget(
			UIElement Base)
			:
			base(Base)
		{
		}
	}


	public class ShipUiEWarElement : UIElement
	{
		public string EWarType;

		public ObjectIdInMemory IconTexture;

		public ShipUiEWarElement()
		{
		}
	}


	public class ShipUiIndication : UIElement
	{
		public UIElementText[] ListLabelString;

		public ShipUiIndication()
		{
		}

		public ShipUiIndication(UIElement Base)
			:
			base(Base)
		{
		}
	}

	public class ShipUiModule : UIElement
	{
		public bool? ModuleButtonVisible;

		public ObjectIdInMemory ModuleButtonIconTexture;

		public string ModuleButtonQuantity;

		public bool RampActive;

		public int? RampRotationMilli;

		public bool? HiliteVisible;

		public bool? GlowVisible;

		public bool? BusyVisible;

		public ShipUiModule()
		{
		}

		public ShipUiModule(UIElement Base)
			:
			base(Base)
		{
		}
	}

	public class ShipUiTargetAssignedGroup : UIElement
	{
		public ObjectIdInMemory IconTexture;

		public ShipUiTargetAssignedGroup()
		{
		}

		public ShipUiTargetAssignedGroup(UIElement Base)
			:
			base(Base)
		{
		}
	}

}
