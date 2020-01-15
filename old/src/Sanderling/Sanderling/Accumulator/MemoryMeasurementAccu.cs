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

		public void Accumulate(FromProcessMeasurement<Parse.IMemoryMeasurement> memoryMeasurementAtTime)
		{
			if (null == memoryMeasurementAtTime)
				return;

			var MemoryMeasurement = memoryMeasurementAtTime?.Value;

			var ShipUi = MemoryMeasurement?.ShipUi;

			var SetModuleInstantNotAssigned =
				ShipUi?.Module?.Select(module => module.AsAccuInstant(ShipUi).WithTimespanInt64(memoryMeasurementAtTime))
				?.Distribute(
				MemoryMeasurement,
				InternShipUiModule);

			foreach (var ModuleInstantNotAssigned in SetModuleInstantNotAssigned.EmptyIfNull())
			{
				InternShipUiModule.Add(new ShipUiModule(++EntityIdLast, ModuleInstantNotAssigned));
			}

			InternShipUiModule?.Where(module => !(memoryMeasurementAtTime?.End - module?.LastInstant?.End < ModuleInvisibleDurationMax))?.ToArray()
				?.ForEach(module => InternShipUiModule.Remove(module));

			if (MemoryMeasurement?.IsDocked ?? false)
			{
				InternShipUiModule?.Clear();
			}
		}
	}
}
