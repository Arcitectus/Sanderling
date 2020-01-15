using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanderling.MemoryReading.Python
{
	/// <summary>
	/// Offsets from https://docs.python.org/2/c-api/typeobj.html
	/// </summary>
	public class PyTypeObject : PyObject
	{
		const int Offset_tp_name = 12;

		readonly public UInt32? tp_name;

		readonly public string tp_name_Val;

		public PyTypeObject(
			Int64 BaseAddress,
			IMemoryReader MemoryReader)
			:
			base(
			BaseAddress,
			MemoryReader)
		{
			tp_name = MemoryReader.ReadUInt32(BaseAddress + Offset_tp_name);

			if (tp_name < int.MaxValue)
			{
				tp_name_Val = MemoryReader.ReadStringAsciiNullTerminated(tp_name.Value, 0x100);
			}
		}

		/// <summary>
		/// the enumerated set contains all addresses of Python Objects of the type with the given <paramref name="tp_name"/>.
		/// 
		/// the addresses are only filtered for appropriate ob_type.
		/// </summary>
		/// <param name="MemoryReader"></param>
		/// <param name="tp_name"></param>
		/// <returns></returns>
		static public IEnumerable<UInt32> EnumeratePossibleAddressesOfInstancesOfPythonTypeFilteredByObType(
			IMemoryReader MemoryReader,
			string tp_name)
		{
			var CandidatesTypeObjectType = PyTypeObject.FindCandidatesTypeObjectTypeAddress(MemoryReader);

			var TypeObjectType = PyTypeObject.TypeObjectAddressesFilterByTpName(CandidatesTypeObjectType, MemoryReader, "type").FirstOrDefault();

			if (null == TypeObjectType)
			{
				yield break;
			}

			//	finds candidate Addresses for Objects of type "type" with only requiring them to have a appropriate value for ob_type.
			var TypeCandidateAddressesPlus4 =
				MemoryReader.AddressesHoldingValue32Aligned32((UInt32)TypeObjectType.BaseAddress, 0, Int32.MaxValue)
				.ToArray();

			var TypeCandidateAddresses =
				TypeCandidateAddressesPlus4
				.Select((Address) => (UInt32)(Address - 4))
				.ToArray();

			var TypeObjectsWithProperName =
				PyTypeObject.TypeObjectAddressesFilterByTpName(TypeCandidateAddresses, MemoryReader, tp_name)
				.ToArray();

			foreach (var TypeObject in TypeObjectsWithProperName)
			{
				//	finds candidate Addresses for Objects of type tp_name with only requiring them to have a appropriate value for ob_type.

				foreach (var CandidateForObjectOfTypeAddressPlus4 in MemoryReader.AddressesHoldingValue32Aligned32((UInt32)TypeObject.BaseAddress, 0, Int32.MaxValue))
				{
					yield return (UInt32)(CandidateForObjectOfTypeAddressPlus4 - 4);
				}
			}
		}

		/// <summary>
		/// enumerates the subset of Addresses that satisfy this condition:
		/// +interpreted as the Address of a Python Type Object, the tp_name of this Object Equals <paramref name="tp_name"/>
		/// </summary>
		/// <param name="TypeObjectAddresses"></param>
		/// <param name="MemoryReader"></param>
		/// <param name="tp_name"></param>
		/// <returns></returns>
		static public IEnumerable<PyTypeObject> TypeObjectAddressesFilterByTpName(
			IEnumerable<UInt32> TypeObjectAddresses,
			IMemoryReader MemoryReader,
			string tp_name)
		{
			if (null == TypeObjectAddresses || null == MemoryReader)
			{
				yield break;
			}

			foreach (var CandidateTypeAddress in TypeObjectAddresses)
			{
				var PyType = new PyTypeObject(CandidateTypeAddress, MemoryReader);

				if (!string.Equals(PyType.tp_name_Val, tp_name))
				{
					continue;
				}

				yield return PyType;
			}
		}

		/// <summary>
		/// enumerates all Addresses that satisfy this condition:
		/// +interpreted as the Address of a Python Type Object, the Member tp_type points to the Object itself.
		/// 
		/// instead of reusing the PyObject class, this method uses a more specialized implementation to achieve lower runtime cost.
		/// 
		/// the method assumes the objects to be 32Bit aligned.
		/// </summary>
		/// <param name="MemoryReader"></param>
		/// <returns></returns>
		static public IEnumerable<UInt32> FindCandidatesTypeObjectTypeAddress(
			IMemoryReader MemoryReader)
		{
			var CandidatesAddress = new List<UInt32>();

			for (Int64 BlockAddress = 0; BlockAddress < int.MaxValue; BlockAddress += Static.Specialisation_RuntimeCost_BlockSize)
			{
				var BlockValues32 = MemoryReader.ReadArray<UInt32>(BlockAddress, Static.Specialisation_RuntimeCost_BlockSize);

				if (null == BlockValues32)
				{
					continue;
				}

				for (int InBlockIndex = 0; InBlockIndex < BlockValues32.Length; InBlockIndex++)
				{
					var CandidatePointerInBlockAddress = InBlockIndex * 4;

					var CandidatePointerAddress = BlockAddress + CandidatePointerInBlockAddress;

					var CandidatePointer = BlockValues32[InBlockIndex];

					if (CandidatePointerAddress == Offset_ob_type + CandidatePointer)
					{
						yield return CandidatePointer;
					}
				}
			}
		}
	}
}
