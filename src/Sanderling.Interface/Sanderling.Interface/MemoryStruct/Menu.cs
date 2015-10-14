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

		public Menu(IUIElement Base)
			:
			base(Base)
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

		public MenuEntry(IUIElement Base)
			:
			base(Base)
		{
		}

		public string Text =>
			LabelText?.Select(Label => Label?.Text)?.OrderByDescending(Text => Text?.Length ?? -1)?.FirstOrDefault();
	}

}
