using Sanderling.MemoryReading.Python;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EveClientMemoryDump.Exe
{
	static public class MemoryReadingReportBuilder
	{
		const int dictSlotCountUpperBound = 0x400;

		static MemoryReadingReportModel.PythonObject BuildReport(PyObject pyObj, IPythonMemoryReader reader)
		{
			pyObj.LoadType(reader);

			var specific = PyObjSpecific(pyObj, reader);

			object valueMappedToClrObj =
				(specific as PyStr)?.String;

			return new MemoryReadingReportModel.PythonObject
			{
				BaseAddress = pyObj.BaseAddress,
				PyTypeObjAddress = pyObj.ob_type,
				PyTypeName = pyObj?.TypeObject?.tp_name_Val,
				ValueMappedToClrObj = valueMappedToClrObj,
			};
		}

		static KeyValuePair<string, Func<Int64, IPythonMemoryReader, PyObject>>[] SetTypeNameAndConstructor =
			new[]
			{
				new KeyValuePair<string, Func<Int64, IPythonMemoryReader, PyObject>>("str", (address, reader) => new PyStr(address, reader)),
			};

		static PyObject PyObjSpecific(PyObject pyObj, IPythonMemoryReader reader)
		{
			if (pyObj == null)
				return null;

			var constructor =
				SetTypeNameAndConstructor?.FirstOrDefault(typeNameAndConstructor => typeNameAndConstructor.Key == pyObj?.TypeObject?.tp_name_Val).Value;

			return constructor?.Invoke(pyObj.BaseAddress, reader);
		}

		static MemoryReadingReportModel.PythonObject BuildReportForPyObjectFromAddress(Int64 pyObjAddress, IPythonMemoryReader reader) =>
			pyObjAddress == 0 ? null :
			BuildReport(new PyObject(pyObjAddress, reader), reader);

		static MemoryReadingReportModel.DictEntry BuildReport(PyDictEntry dictEntry, IPythonMemoryReader reader)
		{
			if (dictEntry == null)
				return null;

			return new MemoryReadingReportModel.DictEntry
			{
				Key = BuildReportForPyObjectFromAddress(dictEntry.me_key ?? 0, reader),
				Value = BuildReportForPyObjectFromAddress(dictEntry.me_value ?? 0, reader),
			};
		}

		static MemoryReadingReportModel.Dict BuildReport(PyDict dict, IPythonMemoryReader reader)
		{
			if (dict == null)
				return null;

			dict.LoadType(reader);

			var slotsFiltered =
				dict?.Slots
				?.Take(dictSlotCountUpperBound)
				?.Select((slot, index) => BuildReport(slot, reader)?.WithSlotIndex(index))
				?.Where(slotReport => slotReport?.Key != null || slotReport?.Value != null)
				?.ToArray();

			return new MemoryReadingReportModel.Dict
			{
				BaseAddress = dict.BaseAddress,
				PyTypeObjAddress = dict.ob_type,
				PyTypeName = dict.TypeObject?.tp_name_Val,
				SlotsFiltered = slotsFiltered,
			};
		}

		static MemoryReadingReportModel.UINode BuildReport(UITreeNode node, IPythonMemoryReader reader)
		{
			node.LoadType(reader);
			node.LoadDict(reader);
			node.LoadChildren(reader);

			var immediateChildrenReports = node.children?.Select(childNode => BuildReport(childNode, reader))?.ToArray();

			return new MemoryReadingReportModel.UINode
			{
				BaseAddress = node.BaseAddress,
				PyTypeObjAddress = node.ob_type,
				PyTypeName = node.TypeObject?.tp_name_Val,
				Dict = BuildReport(node.Dict, reader),
				Children = immediateChildrenReports,
				ChildrenTransitiveCount =
					(immediateChildrenReports?.Length + immediateChildrenReports?.Select(immediateChild => immediateChild?.ChildrenTransitiveCount ?? 0)?.Sum()) ?? 0,
			};
		}

		static public MemoryReadingReportModel.Root BuildReport(IPythonMemoryReader reader)
		{
			var uiRoot = Sanderling.MemoryReading.EveOnline.UIRoot(reader);

			return new MemoryReadingReportModel.Root
			{
				UITreeRoot = BuildReport(uiRoot, reader),
			};
		}
	}
}
