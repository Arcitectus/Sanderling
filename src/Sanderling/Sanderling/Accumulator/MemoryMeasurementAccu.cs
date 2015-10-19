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

		readonly List<ShipUiModule> ShipUiModule = new List<ShipUiModule>();

		IEnumerable<Accumulation.IShipUiModule> Accumulation.IMemoryMeasurement.ShipUiModule => ShipUiModule;

		public void Accumulate(FromProcessMeasurement<Parse.IMemoryMeasurement> MemoryMeasurementAtTime)
		{
			var MemoryMeasurement = MemoryMeasurementAtTime?.Wert;

            var ShipUi = MemoryMeasurement?.ShipUi;

			var SetModuleInstantNotAssigned =
				ShipUi?.Module?.Select(Module => Module.AsAccuInstant(ShipUi).AsIntervalInt64(MemoryMeasurementAtTime))
				?.Distribute(
				MemoryMeasurement,
				ShipUiModule);

			foreach (var ModuleInstantNotAssigned in SetModuleInstantNotAssigned.EmptyIfNull())
			{
				ShipUiModule.Add(new ShipUiModule(++EntityIdLast, ModuleInstantNotAssigned));
			}

			if (MemoryMeasurement?.IsDocked ?? false)
			{
				this.ShipUiModule?.Clear();
			}
		}
	}
}
