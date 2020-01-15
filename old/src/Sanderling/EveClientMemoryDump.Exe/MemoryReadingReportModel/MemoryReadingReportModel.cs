using System;

namespace EveClientMemoryDump.MemoryReadingReportModel
{
	public class Root
	{
		public UINode UITreeRoot;
	}

	public class PythonObject
	{
		public Int64? PyTypeObjAddress;

		public string PyTypeName;

		public Int64 BaseAddress;

		public object ValueMappedToClrObj;
	}

	public class Dict : PythonObject
	{
		public DictEntry[] SlotsFiltered;
	}

	public class DictEntry
	{
		public PythonObject Key;

		public PythonObject Value;

		public int? SlotIndex;

		public DictEntry WithSlotIndex(int? slotIndex) =>
			new DictEntry
			{
				Key = Key,
				Value = Value,
				SlotIndex = slotIndex,
			};
	}

	public class UINode : PythonObject
	{
		public Dict Dict;

		public UINode[] Children;

		public int ChildrenTransitiveCount;
	}
}
