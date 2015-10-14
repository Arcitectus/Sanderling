using System;
using Bib3;

namespace Sanderling.Interface.MemoryStruct
{
	public interface ITreeViewEntry : IContainer, ISelectable, IExpandable, IUIElementText
	{
		ITreeViewEntry[] Child { get; }
	}

	public class TreeViewEntry : Container, ITreeViewEntry
	{
		public ITreeViewEntry[] Child { set; get; }

		public bool? IsSelected { set; get; }

		public bool? IsExpanded { set; get; }

		public IUIElement ExpandToggleButton { set; get; }

		override public OrtogoonInt? RegionInteraction => LabelText?.Largest()?.Region;

		public string Text => LabelText?.Largest()?.Text;

		public TreeViewEntry()
		{
		}

		public TreeViewEntry(IUIElement Base)
			:
			base(Base)
		{
		}
	}

}
