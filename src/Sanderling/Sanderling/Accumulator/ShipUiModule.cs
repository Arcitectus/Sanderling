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

		protected override void Accumulated(PropertyGenTimespanInt64<Accumulation.IShipUiModuleAndContext> Instant, Parse.IMemoryMeasurement Shared)
		{
			base.Accumulated(Instant, Shared);

			var ModuleButtonTooltip = Shared?.ModuleButtonTooltip;

			if ((Instant?.Value?.Module?.HiliteVisible ?? false) &&
				(Instant?.Value?.Location).HasValue &&
				null != ModuleButtonTooltip)
			{
				var TooltipWithTimespan = ModuleButtonTooltip.WithTimespanInt64(Instant);

				var PreviousTooltip = ListTooltip?.LastOrDefault();

				ListTooltip.Enqueue(TooltipWithTimespan);
				ListTooltip.ListeKürzeBegin(4);

				var TooltipLast = TooltipWithTimespan;

				var PreviousInstant = InstantWithAgeStepCount(1);

				if ((PreviousInstant?.Value?.Module?.HiliteVisible ?? false) &&
					PreviousInstant?.Value?.Module?.ModuleButtonIconTexture?.Id == Instant?.Value?.Module?.ModuleButtonIconTexture?.Id &&
					PreviousTooltip?.Begin == PreviousInstant?.Begin)
				{
					//	It seems that data read from module tooltips is corrupted frequently.
					//	To alleviate this problem, tooltips in consecutive measurements are compared and a heuristic is applied to guess which one is the best to pick to be kept.
					//	To benefit from this, a script generates multiple measurements while a tooltip is open for the module.
					TooltipLast = new[] { TooltipLast, PreviousTooltip }.BestRead(inst => inst?.Value);
				}

				this.TooltipLast = TooltipLast;
			}

		}

		/// <summary>
		/// score by distance to last seen Instant.
		/// </summary>
		/// <param name="Instant"></param>
		/// <param name="Shared"></param>
		/// <returns></returns>
		public override int Score(Accumulation.IShipUiModuleAndContext Instant, Parse.IMemoryMeasurement Shared)
		{
			return (int)(10 - ((Instant?.Location - NotDefaultLastInstant?.Value?.Location)?.Length() ?? int.MaxValue));
		}

		ShipUiModule()
		{ }

		public ShipUiModule(
			Int64 Id,
			PropertyGenTimespanInt64<Accumulation.IShipUiModuleAndContext> Instant)
				: base(Id, Instant)
		{
			HistoryLengthMax = 2;
		}

	}
}
