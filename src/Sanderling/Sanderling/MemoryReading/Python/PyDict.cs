using System;
using System.Linq;

namespace Sanderling.MemoryReading.Python
{
	public class PyDictEntry
	{
		readonly public Int64 BaseAddress;

		readonly public UInt32? me_hash;

		readonly public UInt32? me_key;

		readonly public UInt32? me_value;

		readonly public PyStr KeyAsStr;

		public string KeyStr
		{
			get
			{
				var KeyAsStr = this.KeyAsStr;

				if (null == KeyAsStr)
				{
					return null;
				}

				return KeyAsStr.String;
			}
		}

		public PyDictEntry(
			Int64 BaseAddress,
			IMemoryReader MemoryReader)
		{
			this.BaseAddress = BaseAddress;

			var Array = MemoryReader.ReadArray<UInt32>(BaseAddress, 12);

			if (null == Array)
			{
				return;
			}

			if (0 < Array.Length)
			{
				me_hash = Array[0];
			}
			if (1 < Array.Length)
			{
				me_key = Array[1];

				KeyAsStr = new PyStr(me_key.Value, MemoryReader);
			}

			if (2 < Array.Length)
			{
				me_value = Array[2];
			}
		}
	}

	/// <summary>
	/// Offsets from https://github.com/python/cpython/blob/2.7/Include/dictobject.h and https://github.com/python/cpython/blob/2.7/Objects/dictobject.c
	/// </summary>
	public class PyDict : PyObject
	{
		public const int Offset_ma_fill = 8;
		public const int Offset_ma_used = 12;
		public const int Offset_ma_mask = 16;
		public const int Offset_ma_table = 20;

		readonly public UInt32? ma_fill;

		readonly public UInt32? ma_used;

		readonly public UInt32? ma_mask;

		readonly public UInt32? ma_table;

		readonly public UInt32? SlotsCount;

		readonly public PyDictEntry[] Slots;

		public PyDict(
			Int64 BaseAddress,
			IMemoryReader MemoryReader,
			int? SlotsCountMax	= null)
			:
			base(BaseAddress, MemoryReader)
		{
			ma_fill = MemoryReader.ReadUInt32(BaseAddress + Offset_ma_fill);
			ma_used = MemoryReader.ReadUInt32(BaseAddress + Offset_ma_used);
			ma_mask = MemoryReader.ReadUInt32(BaseAddress + Offset_ma_mask);
			ma_table = MemoryReader.ReadUInt32(BaseAddress + Offset_ma_table);

			SlotsCount = ma_mask + 1;

			if (ma_table.HasValue && SlotsCount.HasValue)
			{
				var SlotsToReadCount = (int)Math.Min(SlotsCountMax ?? int.MaxValue, SlotsCount.Value);

				Slots =
					Enumerable.Range(0, SlotsToReadCount)
					.Select((SlotIndex) => new PyDictEntry(ma_table.Value + SlotIndex * 12, MemoryReader))
					.ToArray();
			}
		}

		public PyDictEntry EntryForKeyStr(string KeyStr)
		{
			var Slots = this.Slots;

			if (null == Slots)
			{
				return null;
			}

			foreach (var Slot in Slots)
			{
				if (null == Slot)
				{
					continue;
				}

				if (string.Equals(Slot.KeyStr, KeyStr))
				{
					return Slot;
				}
			}

			return null;
		}
	}
}
