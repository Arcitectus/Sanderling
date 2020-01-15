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

		bool? OverloadOn { get; }
	}

	public interface IShipUiTimer : IContainer
	{
		string Name { get; }
	}

	public interface IShipUi : IContainer
	{
		IUIElement Center { get; }

		/// <summary>
		/// Displays information about current maneuver ("Orbiting", "Warping",....)
		/// The lower Label contains the target and distance or target distance if applicable.
		/// </summary>
		IContainer Indication { get; }

		IShipHitpointsAndEnergy HitpointsAndEnergy { get; }

		IUIElementText SpeedLabel { get; }

		ShipUiEWarElement[] EWarElement { get; }

		IUIElement ButtonSpeed0 { get; }

		IUIElement ButtonSpeedMax { get; }

		IShipUiModule[] Module { get; }

		IUIElementText[] Readout { get; }

		Int64? SpeedMilli { get; }

		IShipUiTimer[] Timer { get; }

		ISquadronsUI SquadronsUI { get; }
	}

	public interface IShipUiTarget : IUIElement, ISelectable
	{
		IUIElementText[] LabelText { get; }

		IShipHitpointsAndEnergy Hitpoints { get; }

		/// <summary>
		/// Each element in this set represents at least one object (e.g. drone or module) assigned to this target.
		/// </summary>
		ShipUiTargetAssignedGroup[] Assigned { get; }
	}

	public class ShipUi : Container, IShipUi, ICloneable
	{
		public IUIElement Center { set; get; }

		public IContainer Indication { set; get; }

		public IShipHitpointsAndEnergy HitpointsAndEnergy { set; get; }

		public IUIElementText SpeedLabel { set; get; }

		public ShipUiEWarElement[] EWarElement { set; get; }

		public IUIElement ButtonSpeed0 { set; get; }

		public IUIElement ButtonSpeedMax { set; get; }

		public IShipUiModule[] Module { set; get; }

		public IUIElementText[] Readout { set; get; }

		public Int64? SpeedMilli { set; get; }

		public IShipUiTimer[] Timer { set; get; }

		public ISquadronsUI SquadronsUI { set; get; }

		public ShipUi()
		{
		}

		public ShipUi(IUIElement @base)
			:
			base(@base)
		{
		}

		public ShipUi Copy() => this.CopyByPolicyMemoryMeasurement();

		public object Clone() => Copy();
	}

	public class ShipUiTarget : UIElement, IShipUiTarget
	{
		public IUIElementText[] LabelText { set; get; }

		public bool? IsSelected { set; get; }

		public IShipHitpointsAndEnergy Hitpoints { set; get; }

		public IUIElement RegionInteractionElement { set; get; }

		override public IUIElement RegionInteraction => RegionInteractionElement?.WithRegionSizeBoundedMaxPivotAtCenter(new Vektor2DInt(40, 40));

		public ShipUiTargetAssignedGroup[] Assigned { set; get; }

		public ShipUiTarget()
			:
			this(null)
		{
		}

		public ShipUiTarget(
			IUIElement @base)
			:
			base(@base)
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

		public ShipUiEWarElement(IUIElement @base)
			:
			base(@base)
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

		public bool? OverloadOn { set; get; }

		public override IUIElement RegionInteraction => this.WithRegionSizeBoundedMaxPivotAtCenter(new Vektor2DInt(16, 16));

		public ShipUiModule()
		{
		}

		public ShipUiModule(IUIElement @base)
			:
			base(@base)
		{
		}
	}

	public class ShipUiTargetAssignedGroup : UIElement
	{
		public IObjectIdInMemory IconTexture;

		public ShipUiTargetAssignedGroup()
		{
		}

		public ShipUiTargetAssignedGroup(IUIElement @base)
			:
			base(@base)
		{
		}
	}

	public class ShipUiTimer : Container, IShipUiTimer
	{
		public string Name { set; get; }

		public ShipUiTimer()
		{
		}

		public ShipUiTimer(IUIElement @base)
			:
			base(@base)
		{
		}
	}
}
