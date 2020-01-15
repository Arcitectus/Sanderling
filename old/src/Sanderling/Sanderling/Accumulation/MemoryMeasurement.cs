using System.Collections.Generic;

namespace Sanderling.Accumulation
{
	/// <summary>
	/// Data collected and connected from multiple measurements.
	/// </summary>
	public interface IMemoryMeasurement
	{
		IEnumerable<IShipUiModule> ShipUiModule { get; }
	}
}
