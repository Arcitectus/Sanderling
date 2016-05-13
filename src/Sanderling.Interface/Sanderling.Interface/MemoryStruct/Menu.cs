using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanderling.Interface.MemoryStruct
{
	/// <summary>
	/// A menu can be opened for some UIElements by rightclicking on them.
	/// </summary>
	public interface IMenu : IUIElement
	{
		IMenuEntry[] Entry { get; }
	}

	public interface IMenuEntry : IUIElementText
	{
		bool? HighlightVisible { get; }
    }

	public class Menu : UIElement, IMenu
	{
		public IMenuEntry[] Entry { set; get; }

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
