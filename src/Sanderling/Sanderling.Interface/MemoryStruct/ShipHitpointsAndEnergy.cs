namespace Sanderling.Interface.MemoryStruct
{
	public interface IShipHitpointsAndEnergy
	{
		int? Struct { get; }
		int? Armor { get; }
		int? Shield { get; }
		int? Capacitor { get; }
	}

	public class ShipHitpointsAndEnergy : IShipHitpointsAndEnergy
	{
		public int? Struct { set; get; }
		public int? Armor { set; get; }
		public int? Shield { set; get; }
		public int? Capacitor { set; get; }
	}
}
