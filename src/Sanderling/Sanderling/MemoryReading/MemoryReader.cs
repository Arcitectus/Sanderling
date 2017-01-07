using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sanderling.MemoryReading
{
	public class MemoryReaderModuleInfo
	{
		readonly public string ModuleName;

		readonly public Int64 BaseAddress;

		public MemoryReaderModuleInfo(
			string ModuleName,
			Int64 BaseAddress)
		{
			this.ModuleName = ModuleName;
			this.BaseAddress = BaseAddress;
		}
	}
	public interface IMemoryReader
	{
		byte[] ReadBytes(Int64 Address, int BytesCount);

		MemoryReaderModuleInfo[] Modules();
	}

	/// <summary>
	/// extension methods for IMemoryReader.
	/// </summary>
	static public class MemoryReaderExtension
	{
		static public UInt32? ReadPointerPath32(
			this	IMemoryReader MemoryReader,
			KeyValuePair<string,	UInt32[]>		RootModuleNameAndListOffset)
		{
			return ReadPointerPath32(MemoryReader,	RootModuleNameAndListOffset.Key, RootModuleNameAndListOffset.Value);
		}

		static public UInt32? ReadPointerPath32(
			this	IMemoryReader MemoryReader,
			string RootModuleName,
			UInt32[] ListOffset)
		{
			if (null == MemoryReader)
			{
				return null;
			}

			if (null == ListOffset)
			{
				return null;
			}

			if (ListOffset.Length < 1)
			{
				return null;
			}

			UInt32 RootModuleOffset = 0;

			if (null != RootModuleName)
			{
				var Modules = MemoryReader.Modules();

				if (null == Modules)
				{
					return null;
				}

				var RootModule = Modules.FirstOrDefault((Module) => string.Equals(Module.ModuleName,	RootModuleName, StringComparison.InvariantCultureIgnoreCase));

				if (null == RootModule)
				{
					return null;
				}

				RootModuleOffset = (UInt32)RootModule.BaseAddress;
			}

			var CurrentAddress = RootModuleOffset;

			for (int NodeIndex = 0; NodeIndex < ListOffset.Length - 1; NodeIndex++)
			{
				var NodeOffset = ListOffset[NodeIndex];

				CurrentAddress += NodeOffset;

				var NodePointer = MemoryReader.ReadUInt32(CurrentAddress);

				if (!NodePointer.HasValue)
				{
					return null;
				}

				CurrentAddress = NodePointer.Value;
			}

			CurrentAddress += ListOffset.LastOrDefault();

			return CurrentAddress;
		}
		static public UInt32? ReadUInt32(
			this	IMemoryReader MemoryReader,
			Int64 Address)
		{
			if (null == MemoryReader)
			{
				return null;
			}

			var Bytes = MemoryReader.ReadBytes(Address, 4);

			if (null == Bytes)
			{
				return null;
			}

			if (Bytes.Length < 4)
			{
				return null;
			}

			return BitConverter.ToUInt32(Bytes, 0);
		}

		static public string ReadStringAsciiNullTerminated(
			this	IMemoryReader MemoryReader,
			Int64 Address,
			int LengthMax = 0x1000)
		{
			if (null == MemoryReader)
			{
				return null;
			}

			var Bytes = MemoryReader.ReadBytes(Address, LengthMax);

			if (null == Bytes)
			{
				return null;
			}

			var BytesNullTerminated = Bytes.TakeWhile((Byte) => 0 != Byte).ToArray();

			return Encoding.ASCII.GetString(BytesNullTerminated);
		}

		static public T[] ReadArray<T>(
			this	IMemoryReader MemoryReader,
			Int64 Address,
			int NumberOfBytes)
			where T : struct
		{
			if (null == MemoryReader)
			{
				return null;
			}

			var BytesRead = MemoryReader.ReadBytes(Address, NumberOfBytes);

			if (null == BytesRead)
			{
				return null;
			}

			var ElementSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));

			var NumberOfElements = (BytesRead.Length - 1) / ElementSize + 1;

			var Array = new T[NumberOfElements];

			Buffer.BlockCopy(BytesRead, 0, Array, 0, BytesRead.Length);

			return Array;
		}

		/// <summary>
		/// enumerates all Addresses which are aligned to 32Bits and hold the value <paramref name="SearchedValue"/>.
		/// </summary>
		/// <param name="MemoryReader"></param>
		/// <param name="SearchedValue"></param>
		/// <param name="SearchedRegionBegin"></param>
		/// <param name="SearchedRegionEnd"></param>
		/// <returns></returns>
		static public IEnumerable<Int64> AddressesHoldingValue32Aligned32(
			this	IMemoryReader MemoryReader,
			UInt32 SearchedValue,
			Int64 SearchedRegionBegin,
			Int64 SearchedRegionEnd)
		{
			if (null == MemoryReader)
			{
				yield break;
			}

			var FirstBlockAddress =
				(SearchedRegionBegin / Static.Specialisation_RuntimeCost_BlockSize) * Static.Specialisation_RuntimeCost_BlockSize;

			var LastBlockAddress =
				(SearchedRegionEnd / Static.Specialisation_RuntimeCost_BlockSize) * Static.Specialisation_RuntimeCost_BlockSize;

			for (Int64 BlockAddress = FirstBlockAddress; BlockAddress <= LastBlockAddress; BlockAddress += Static.Specialisation_RuntimeCost_BlockSize)
			{
				var BlockValues = MemoryReader.ReadArray<UInt32>(BlockAddress, Static.Specialisation_RuntimeCost_BlockSize);

				if (null == BlockValues)
				{
					continue;
				}

				for (int InBlockIndex = 0; InBlockIndex < BlockValues.Length; InBlockIndex++)
				{
					var Address = BlockAddress + (InBlockIndex * 4);

					if (Address < SearchedRegionBegin ||
						SearchedRegionEnd < Address)
					{
						continue;
					}

					if (SearchedValue == BlockValues[InBlockIndex])
					{
						yield return Address;
					}
				}
			}
		}
	}
}
