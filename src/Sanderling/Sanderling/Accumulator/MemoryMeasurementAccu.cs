using Bib3;
using BotEngine.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanderling.Accumulator
{
	public class MemoryMeasurementAccumulator : Accumulation.IMemoryMeasurement
	{
		Int64 EntityIdLast = 0;

		int ModuleInvisibleDurationMax = 10000;

		readonly List<ShipUiModule> InternShipUiModule = new List<ShipUiModule>();

		public IEnumerable<Accumulation.IShipUiModule> ShipUiModule => InternShipUiModule;

		public void Accumulate(FromProcessMeasurement<Parse.IMemoryMeasurement> MemoryMeasurementAtTime)
		{
			if (null == MemoryMeasurementAtTime)
				return;

			var MemoryMeasurement = MemoryMeasurementAtTime?.Value;

			var ShipUi = MemoryMeasurement?.ShipUi;

			var SetModuleInstantNotAssigned =
				ShipUi?.Module?.Select(Module => Module.AsAccuInstant(ShipUi).WithTimespanInt64(MemoryMeasurementAtTime))
				?.Distribute(
				MemoryMeasurement,
				InternShipUiModule);

			foreach (var ModuleInstantNotAssigned in SetModuleInstantNotAssigned.EmptyIfNull())
			{
				InternShipUiModule.Add(new ShipUiModule(++EntityIdLast, ModuleInstantNotAssigned));
			}

			InternShipUiModule?.Where(Module => !(MemoryMeasurementAtTime?.End - Module?.LastInstant?.End < ModuleInvisibleDurationMax))?.ToArray()
				?.ForEach(Module => InternShipUiModule.Remove(Module));

			if (MemoryMeasurement?.IsDocked ?? false)
			{
				InternShipUiModule?.Clear();
			}
		}
	}
}
