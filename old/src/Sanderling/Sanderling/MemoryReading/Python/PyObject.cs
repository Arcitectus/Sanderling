using System;

namespace Sanderling.MemoryReading.Python
{
	/// <summary>
	/// Offsets from https://docs.python.org/2/c-api/structures.html
	/// </summary>
	public class PyObject
	{
		public const int Offset_ob_refcnt = 0;
		public const int Offset_ob_type = 4;

		readonly public Int64 BaseAddress;

		readonly public UInt32? ob_refcnt;

		readonly public UInt32? ob_type;

		public PyTypeObject TypeObject;

		public PyObject(
			Int64 BaseAddress,
			IMemoryReader MemoryReader)
		{
			this.BaseAddress = BaseAddress;

			ob_refcnt = MemoryReader.ReadUInt32(BaseAddress + Offset_ob_refcnt);

			ob_type = MemoryReader.ReadUInt32(BaseAddress + Offset_ob_type);
		}

		public PyTypeObject LoadType(IPythonMemoryReader MemoryReader)
		{
			if (ob_type.HasValue)
			{
				TypeObject = MemoryReader.TypeFromAddress(ob_type.Value);
			}

			return TypeObject;
		}

		static public string TypeNameForObjectWithAddress(
			UInt32 ObjectAddress,
			IPythonMemoryReader MemoryReader)
		{
			var Object = new PyObject(ObjectAddress, MemoryReader);

			Object.LoadType(MemoryReader);

			var ObjectTypeObject = Object.TypeObject;

			if (null == ObjectTypeObject)
			{
				return null;
			}

			return ObjectTypeObject.tp_name_Val;
		}
	}

	public class PyObjectVar	: PyObject
	{
		readonly public UInt32? ob_size;

		public const int Offset_ob_size = 8;

		public PyObjectVar(
			Int64 BaseAddress,
			IMemoryReader MemoryReader)
			:
			base(BaseAddress, MemoryReader)
		{
			ob_size = MemoryReader.ReadUInt32(BaseAddress + Offset_ob_size);
		}
	}
}
