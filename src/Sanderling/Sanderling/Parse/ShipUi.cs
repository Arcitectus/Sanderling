using System;
using System.Linq;
using Bib3.Geometrik;
using MemoryStruct = Sanderling.Interface.MemoryStruct;
using BotEngine.Common;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.Parse
{
	public interface IShipUiTarget : MemoryStruct.IShipUiTarget
	{
		Int64? DistanceMin { get; }

		Int64? DistanceMax { get; }

		string[] TextRow { get; }
	}

	public class ShipUiTarget : IShipUiTarget
	{
		MemoryStruct.IShipUiTarget Raw;

		public Int64? DistanceMin { set; get; }

		public Int64? DistanceMax { set; get; }

		public string[] TextRow { set; get; }

		public MemoryStruct.ShipUiTargetAssignedGroup[] Assigned => Raw?.Assigned;

		public int? ChildLastInTreeIndex => Raw?.ChildLastInTreeIndex;

		public MemoryStruct.IShipHitpointsAndEnergy Hitpoints => Raw?.Hitpoints;

		public long Id => Raw?.Id ?? 0;

		public int? InTreeIndex => Raw?.InTreeIndex;

		public bool? IsSelected => Raw?.IsSelected;

		public MemoryStruct.IUIElementText[] LabelText => Raw?.LabelText;

		public RectInt Region => Raw?.Region ?? RectInt.Empty;

		public MemoryStruct.IUIElement RegionInteraction => Raw?.RegionInteraction;

		ShipUiTarget()
		{
		}

		public ShipUiTarget(MemoryStruct.IShipUiTarget Raw)
		{
			this.Raw = Raw;

			if (null == Raw)
			{
				return;
			}

			var TextRow =
				Raw?.LabelText?.OrderByCenterVerticalDown()
				?.Select(LabelText => LabelText?.Text?.RemoveXmlTag())
				?.ToArray();

			var DistanceMinMax = TextRow?.LastOrDefault()?.DistanceParseMinMaxKeyValue();

			DistanceMin = DistanceMinMax?.Key;
			DistanceMax = DistanceMinMax?.Value;

			this.TextRow = TextRow?.Reverse()?.Skip(1)?.Reverse()?.ToArray();
		}
	}

	static public class ShipUiExtension
	{
		static public IShipUiTarget Parse(this MemoryStruct.IShipUiTarget ShipUiTarget) =>
			null == ShipUiTarget ? null : new ShipUiTarget(ShipUiTarget);
	}
}
