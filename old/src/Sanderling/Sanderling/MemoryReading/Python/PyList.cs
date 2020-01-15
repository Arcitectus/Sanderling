using System;

namespace Sanderling.MemoryReading.Python
{
	/// <summary>
	/// Offsets from https://github.com/python/cpython/blob/2.7/Include/listobject.h and https://github.com/python/cpython/blob/2.7/Objects/listobject.c
	/// </summary>
	public class PyList : PyObjectVar
	{
		public const int Offset_ob_item	= 12;

		readonly public UInt32? ob_item;

		readonly public UInt32[] Items;

		public PyList(
			Int64 BaseAddress,
			IMemoryReader MemoryReader)
			:
			base(BaseAddress, MemoryReader)
		{
			ob_item = MemoryReader.ReadUInt32(BaseAddress + Offset_ob_item);

			if(ob_item.HasValue	&&	ob_size.HasValue)
			{
				Items = MemoryReader.ReadArray<UInt32>(ob_item.Value, (int)ob_size.Value * 4);
			}
		}
	}
}
