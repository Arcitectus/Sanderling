using System.Linq;

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

		override public IUIElement RegionInteraction => LabelText?.Largest();

		public string Text => LabelText?.Where(label => 0 < label?.Text?.Trim()?.Length)?.OrderByCenterVerticalDown()?.FirstOrDefault()?.Text;

		public TreeViewEntry()
		{
		}

		public TreeViewEntry(IUIElement @base)
			:
			base(@base)
		{
		}
	}
}
