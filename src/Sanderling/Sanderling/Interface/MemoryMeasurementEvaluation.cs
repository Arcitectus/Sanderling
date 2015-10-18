using Sanderling.Parse;
using System;

namespace Sanderling.Interface
{
	public class MemoryMeasurementEvaluation
	{
		public MemoryStruct.IMemoryMeasurement MemoryMeasurement { private set; get; }

		public Parse.IMemoryMeasurement MemoryMeasurementParsed { private set; get; }

		public Exception MemoryMeasurementParseException { private set; get; }

		public MemoryMeasurementEvaluation()
		{
		}

		public MemoryMeasurementEvaluation(MemoryStruct.IMemoryMeasurement MemoryMeasurement)
		{
			this.MemoryMeasurement = MemoryMeasurement;

			try
			{
				MemoryMeasurementParsed = MemoryMeasurement.Parse();
			}
			catch (Exception Exception)
			{
				MemoryMeasurementParseException = Exception;
			}
		}
	}

}
