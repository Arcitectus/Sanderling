using BotEngine.Interface;
using Sanderling.Parse;
using System;

namespace Sanderling.Interface
{
	public class MemoryMeasurementEvaluation
	{
		public MemoryStruct.IMemoryMeasurement MemoryMeasurement { private set; get; }

		public Parse.IMemoryMeasurement MemoryMeasurementParsed { private set; get; }

		public Exception MemoryMeasurementParseException { private set; get; }

		public Accumulation.IMemoryMeasurement MemoryMeasurementAccumulation { private set; get; }

		public Exception MemoryMeasurementAccuException { private set; get; }

		public MemoryMeasurementEvaluation()
		{
		}

		public MemoryMeasurementEvaluation(
			FromProcessMeasurement<MemoryStruct.IMemoryMeasurement> MemoryMeasurement,
			Accumulator.MemoryMeasurementAccumulator MemoryMeasurementAccu = null)
		{
			this.MemoryMeasurement = MemoryMeasurement?.Wert;

			try
			{
				MemoryMeasurementParsed = MemoryMeasurement?.Wert?.Parse();
			}
			catch (Exception Exception)
			{
				MemoryMeasurementParseException = Exception;
			}

			if (null == MemoryMeasurement)
			{
				return;
			}

			try
			{
				MemoryMeasurementAccu = MemoryMeasurementAccu ?? new Accumulator.MemoryMeasurementAccumulator();

				MemoryMeasurementAccu.Accumulate(MemoryMeasurement?.MapValue(t => MemoryMeasurementParsed));

				this.MemoryMeasurementAccumulation = MemoryMeasurementAccu;
			}
			catch (Exception Exception)
			{
				MemoryMeasurementAccuException = Exception;
			}
		}
	}

}
