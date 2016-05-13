using System.Collections.Generic;
using System.Linq;

namespace Sanderling.Interface.MemoryStruct
{
	/// <summary>
	/// A menu can be opened for some UIElements by rightclicking on them.
	/// </summary>
	public interface IMenu : IUIElement
	{
		IEnumerable<IMenuEntry> Entry { get; }
	}

	public interface IMenuEntry : IUIElementText, IContainer
	{
		bool? HighlightVisible { get; }
	}

	public class Menu : UIElement, IMenu
	{
		public IMenuEntry[] Entry { set; get; }

		IEnumerable<IMenuEntry> IMenu.Entry => Entry;

		public Menu(IUIElement @base)
			:
			base(@base)
		{
		}

		public Menu()
		{
		}
	}

	public class MenuEntry : Container, IMenuEntry
	{
		public bool? HighlightVisible { set; get; }

		public MenuEntry()
			:
			this(null)
		{
		}

		public MenuEntry(IUIElement @base)
			:
			base(@base)
		{
		}

		public string Text =>
			LabelText?.Select(label => label?.Text)?.OrderByDescending(text => text?.Length ?? -1)?.FirstOrDefault();
	}
}
