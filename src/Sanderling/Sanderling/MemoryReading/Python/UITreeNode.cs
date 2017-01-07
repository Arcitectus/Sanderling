using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanderling.MemoryReading.Python
{
	public class PyObjectWithRefToDictAt8 : PyObject
	{
		public const int Offset_dict = 8;

		readonly public UInt32? ref_dict;

		public PyDict Dict
		{
			private set;
			get;
		}

		public PyObjectWithRefToDictAt8(
			Int64 BaseAddress,
			IMemoryReader MemoryReader)
			:
			base(BaseAddress, MemoryReader)
		{
			ref_dict = MemoryReader.ReadUInt32(BaseAddress + Offset_dict);
		}

		public PyDict LoadDict(
			IPythonMemoryReader MemoryReader)
		{
			if (ref_dict.HasValue)
			{
				Dict = new PyDict(ref_dict.Value, MemoryReader,	0x1000);
			}

			return Dict;
		}
	}

	public class PyChildrenList : PyObjectWithRefToDictAt8
	{
		PyDictEntry DictEntryChildren;

		PyList ChildrenList;

		public UITreeNode[] children
		{
			private set;
			get;
		}

		public PyChildrenList(
			Int64 BaseAddress,
			IMemoryReader MemoryReader)
			:
			base(BaseAddress, MemoryReader)
		{
		}

		public UITreeNode[] LoadChildren(
			IPythonMemoryReader MemoryReader)
		{
			var Dict = this.Dict;

			if (null != Dict)
			{
				DictEntryChildren = Dict.EntryForKeyStr("_childrenObjects");
			}

			if (null != DictEntryChildren)
			{
				if (DictEntryChildren.me_value.HasValue)
				{
					ChildrenList = new PyList(DictEntryChildren.me_value.Value, MemoryReader);
				}
			}

			if (null != ChildrenList)
			{
				var Items = ChildrenList.Items;

				if (null != Items)
				{
					children = Items.Select((ChildAddress) => new UITreeNode(ChildAddress, MemoryReader)).ToArray();
				}
			}

			return children;
		}
	}

	public class UITreeNode : PyObjectWithRefToDictAt8
	{
		PyDictEntry DictEntryChildren;

		PyChildrenList ChildrenList;

		public UITreeNode[] children
		{
			private set;
			get;
		}

		public UITreeNode(
			Int64 BaseAddress,
			IMemoryReader MemoryReader)
			:
			base(BaseAddress, MemoryReader)
		{
		}

		public UITreeNode[] LoadChildren(
			IPythonMemoryReader MemoryReader)
		{
			var Dict = this.Dict;

			if (null != Dict)
			{
				DictEntryChildren = Dict.EntryForKeyStr("children");
			}

			if (null != DictEntryChildren)
			{
				if (DictEntryChildren.me_value.HasValue)
				{
					ChildrenList = new PyChildrenList(DictEntryChildren.me_value.Value, MemoryReader);

					ChildrenList.LoadDict(MemoryReader);

					ChildrenList.LoadChildren(MemoryReader);
				}
			}

			if (null != ChildrenList)
			{
				children = ChildrenList.children;
			}

			return children;
		}

		public IEnumerable<UITreeNode> EnumerateChildrenTransitive(
			IPythonMemoryReader MemoryReader,
			int? DepthMax = null)
		{
			if (DepthMax <= 0)
			{
				yield break;
			}

			this.LoadDict(MemoryReader);

			this.LoadChildren(MemoryReader);

			var children = this.children;

			if (null == children)
			{
				yield break;
			}

			foreach (var child in children)
			{
				yield return child;

				foreach (var childChild in child.EnumerateChildrenTransitive(MemoryReader, DepthMax - 1))
				{
					yield return childChild;
				}
			}
		}
	}
}
