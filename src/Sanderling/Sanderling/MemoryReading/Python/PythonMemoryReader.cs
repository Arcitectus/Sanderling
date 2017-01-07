using System;
using System.Collections.Generic;

namespace Sanderling.MemoryReading.Python
{
	public interface IPythonMemoryReader : IMemoryReader
	{
		PyTypeObject TypeFromAddress(UInt32 TypeObjectAddress);
	}

	/// <summary>
	/// caches Type Objects.
	/// </summary>
	public	class PythonMemoryReader : IPythonMemoryReader
	{
		readonly IMemoryReader MemoryReader;

		readonly Dictionary<UInt32, PyTypeObject> CacheTypeObject = new Dictionary<UInt32, PyTypeObject>();

		public	PythonMemoryReader(
			IMemoryReader MemoryReader)
		{
			this.MemoryReader = MemoryReader;
		}

		PyTypeObject IPythonMemoryReader.TypeFromAddress(uint TypeObjectAddress)
		{
			PyTypeObject TypeObject;

			if(CacheTypeObject.TryGetValue(TypeObjectAddress, out	TypeObject))
			{
				return TypeObject;
			}

			TypeObject = new PyTypeObject(TypeObjectAddress, MemoryReader);

			CacheTypeObject[TypeObjectAddress] = TypeObject;

			return TypeObject;
		}

		byte[] IMemoryReader.ReadBytes(long Address, int BytesCount)
		{
			return MemoryReader.ReadBytes(Address, BytesCount);
		}

		MemoryReaderModuleInfo[] IMemoryReader.Modules()
		{
			return MemoryReader.Modules();
		}
	}
}
