using Bib3;
using MemoryStruct = Sanderling.Interface.MemoryStruct;
using Sanderling.Parse;
using System;
using Bib3.Geometrik;
using System.Collections.Generic;
using System.Linq;
using Sanderling.Accumulation;

namespace Sanderling.Accumulator
{
	public class ShipUiModuleAndContext : Accumulation.IShipUiModuleAndContext
	{
		public MemoryStruct.IShipUiModule Module { set; get; }

		public Vektor2DInt? Location { set; get; }

		public ShipUiModuleAndContext()
		{
		}
	}

	public class ShipUiModule : EntityScoring<Accumulation.IShipUiModuleAndContext, Parse.IMemoryMeasurement>, Accumulation.IShipUiModule
	{
		readonly Queue<PropertyGenTimespanInt64<IModuleButtonTooltip>> ListTooltip = new Queue<PropertyGenTimespanInt64<IModuleButtonTooltip>>();

		public PropertyGenTimespanInt64<IModuleButtonTooltip> TooltipLast { private set; get; }

		public MemoryStruct.IObjectIdInMemory RepresentedMemoryObject => RepresentedInstant;

		public MemoryStruct.IShipUiModule RepresentedInstant => LastInstant?.Value?.Module;

		public bool? ModuleButtonVisible => RepresentedInstant?.ModuleButtonVisible;

		public MemoryStruct.IObjectIdInMemory ModuleButtonIconTexture => RepresentedInstant?.ModuleButtonIconTexture;

		public string ModuleButtonQuantity => RepresentedInstant?.ModuleButtonQuantity;

		public bool RampActive => RepresentedInstant?.RampActive ?? false;

		public int? RampRotationMilli => RepresentedInstant?.RampRotationMilli;

		public bool? HiliteVisible => RepresentedInstant?.HiliteVisible;

		public bool? GlowVisible => RepresentedInstant?.GlowVisible;

		public bool? BusyVisible => RepresentedInstant?.BusyVisible;

		public RectInt Region => RepresentedInstant?.Region ?? RectInt.Empty;

		public int? InTreeIndex => RepresentedInstant?.InTreeIndex;

		public MemoryStruct.IUIElement RegionInteraction => RepresentedInstant?.RegionInteraction;

		public int? ChildLastInTreeIndex => RepresentedInstant?.ChildLastInTreeIndex;

		public bool? OverloadOn => RepresentedInstant?.OverloadOn;

		protected override void Accumulated(PropertyGenTimespanInt64<IShipUiModuleAndContext> instant, Parse.IMemoryMeasurement otherUIElements)
		{
			base.Accumulated(instant, otherUIElements);

			var moduleButtonTooltip = otherUIElements?.ModuleButtonTooltip;

			if ((instant?.Value?.Module?.HiliteVisible ?? false) &&
				(instant?.Value?.Location).HasValue &&
				(moduleButtonTooltip?.LabelText?.Any() ?? false))
			{
				var tooltipWithTimespan = moduleButtonTooltip.WithTimespanInt64(instant);

				var previousTooltip = ListTooltip?.LastOrDefault();

				ListTooltip.Enqueue(tooltipWithTimespan);
				ListTooltip.ListeKürzeBegin(4);

				var tooltipLast = tooltipWithTimespan;

				var previousInstant = InstantWithAgeStepCount(1);

				if ((previousInstant?.Value?.Module?.HiliteVisible ?? false) &&
					previousInstant?.Value?.Module?.ModuleButtonIconTexture?.Id == instant?.Value?.Module?.ModuleButtonIconTexture?.Id &&
					previousTooltip?.Begin == previousInstant?.Begin)
				{
					//	It seems that data read from module tooltips is corrupted frequently.
					//	To alleviate this problem, tooltips in consecutive measurements are compared and a heuristic is applied to guess which one is the best to pick to be kept.
					//	To benefit from this, a script generates multiple measurements while a tooltip is open for the module.
					tooltipLast = new[] { tooltipLast, previousTooltip }.BestRead(inst => inst?.Value);
				}

				TooltipLast = tooltipLast;
			}
		}

		/// <summary>
		/// score by distance to last seen Instant.
		/// </summary>
		/// <param name="instant"></param>
		/// <param name="shared"></param>
		/// <returns></returns>
		public override int Score(Accumulation.IShipUiModuleAndContext instant, Parse.IMemoryMeasurement shared)
		{
			return (int)(10 - ((instant?.Location - NotDefaultLastInstant?.Value?.Location)?.Length() ?? int.MaxValue));
		}

		ShipUiModule()
		{ }

		public ShipUiModule(
			Int64 id,
			PropertyGenTimespanInt64<Accumulation.IShipUiModuleAndContext> instant)
				: base(id, instant)
		{
			HistoryLengthMax = 2;
		}

	}
}
