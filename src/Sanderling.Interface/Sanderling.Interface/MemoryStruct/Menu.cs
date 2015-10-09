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
	public class Menu : UIElement
	{
		public MenuEntry[] Entry;

		public Menu(UIElement Base)
			:
			base(Base)
		{
		}

		public Menu()
		{
		}
	}

	public class MenuEntry : UIElementText
	{
		public bool? HighlightVisible;

		public MenuEntry()
			:
			this(null)
		{
		}

		public MenuEntry(UIElementText Base)
			:
			base(Base)
		{
		}
	}

}
