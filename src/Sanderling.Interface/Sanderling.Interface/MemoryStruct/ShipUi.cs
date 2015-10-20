using Bib3;
using Bib3.Geometrik;
using System;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IShipUiModule : IUIElement
	{
		bool? ModuleButtonVisible { get; }

		IObjectIdInMemory ModuleButtonIconTexture { get; }

		string ModuleButtonQuantity { get; }

		bool RampActive { get; }

		int? RampRotationMilli { get; }

		bool? HiliteVisible { get; }

		bool? GlowVisible { get; }

		bool? BusyVisible { get; }

	}

	public interface IShipUiTimer : IContainer
	{
		string Name { get; }
	}

	public interface IShipUi
	{
		IUIElement Center { get; }

		/// <summary>
		/// Displays information about current maneuver ("Orbiting", "Warping",....)
		/// The lower Label contains the target and distance or target distance if applicable.
		/// </summary>
		IUIElementText[] Indication { get; }

		IShipHitpointsAndEnergy HitpointsAndEnergy { get; }

		IUIElementText SpeedLabel { get; }

		ShipUiEWarElement[] EWarElement { get; }

		IUIElement ButtonSpeed0 { get; }

		IUIElement ButtonSpeedMax { get; }

		IShipUiModule[] Module { get; }

		IUIElementText[] Readout { get; }

		Int64? SpeedMilli { get; }

		IShipUiTimer[] Timer { get; }
	}

	public class ShipUi : UIElement, IShipUi, ICloneable
	{
		public IUIElement Center { set; get; }

		public IUIElementText[] Indication { set; get; }

		public IShipHitpointsAndEnergy HitpointsAndEnergy { set; get; }

		public IUIElementText SpeedLabel { set; get; }

		public ShipUiEWarElement[] EWarElement { set; get; }

		public IUIElement ButtonSpeed0 { set; get; }

		public IUIElement ButtonSpeedMax { set; get; }

		public IShipUiModule[] Module { set; get; }

		public IUIElementText[] Readout { set; get; }

		public Int64? SpeedMilli { set; get; }

		public IShipUiTimer[] Timer { set; get; }

		public ShipUi()
		{
		}

		public ShipUi(IUIElement Base)
			:
			base(Base)
		{
		}

		public ShipUi Copy() => this.CopyByPolicyMemoryMeasurement();

		public object Clone() => Copy();
	}

	public class ShipUiTarget : UIElement, IUIElement
	{
		public IUIElementText[] LabelText;

		public bool? Active;

		public IShipHitpointsAndEnergy Hitpoints;

		/// <summary>
		/// click here to activate (input focus) or open a menu.
		/// </summary>
		public IUIElement RegionInteractionElement;

		override public IUIElement RegionInteraction => RegionInteractionElement?.WithRegionSizeBoundedMaxPivotAtCenter(new Vektor2DInt(40, 40));

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
			IUIElement Base)
			:
			base(Base)
		{
		}
	}


	public class ShipUiEWarElement : UIElement
	{
		public string EWarType;

		public IObjectIdInMemory IconTexture;

		public ShipUiEWarElement()
		{
		}

		public ShipUiEWarElement(IUIElement Base)
			:
			base(Base)
		{
		}
	}

	public class ShipUiModule : UIElement, IShipUiModule
	{
		public bool? ModuleButtonVisible { set; get; }

		public IObjectIdInMemory ModuleButtonIconTexture { set; get; }

		public string ModuleButtonQuantity { set; get; }

		public bool RampActive { set; get; }

		public int? RampRotationMilli { set; get; }

		public bool? HiliteVisible { set; get; }

		public bool? GlowVisible { set; get; }

		public bool? BusyVisible { set; get; }

		public override IUIElement RegionInteraction => this.WithRegionSizeBoundedMaxPivotAtCenter(new Vektor2DInt(16, 16));

		public ShipUiModule()
		{
		}

		public ShipUiModule(IUIElement Base)
			:
			base(Base)
		{
		}
	}

	public class ShipUiTargetAssignedGroup : UIElement
	{
		public IObjectIdInMemory IconTexture;

		public ShipUiTargetAssignedGroup()
		{
		}

		public ShipUiTargetAssignedGroup(IUIElement Base)
			:
			base(Base)
		{
		}
	}

	public class ShipUiTimer : Container, IShipUiTimer
	{
		public string Name { set; get; }

		public ShipUiTimer()
		{
		}

		public ShipUiTimer(IUIElement Base)
			:
			base(Base)
		{
		}
	}

}
