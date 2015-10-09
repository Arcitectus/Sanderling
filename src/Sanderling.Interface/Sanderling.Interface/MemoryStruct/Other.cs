using BotEngine.Interface;

namespace Sanderling.Interface.MemoryStruct
{
	static public class ToStringStatic
	{
		static public string SensorObjectToString(this object Obj)
		{
			return
				(Obj as string) ??
				Obj?.GetType()?.ToString();
		}
	}

	public class Sprite : UIElement
	{
		public ColorORGB Color;

		public string Name;

		public ObjectIdInMemory Texture0Id;

		public string TexturePath;

		public string HintText;

		public Sprite(UIElement Base)
			:
			base(Base)
		{
		}

		public Sprite()
		{
		}
	}

	public class Neocom : UIElement
	{
		public UIElement EveMenuButton;

		public UIElement CharButton;

		/// <summary>
		/// The type can be identified by TexturePath.
		/// Some examples:
		/// "res:/ui/Texture/WindowIcons/peopleandplaces.png"
		/// "res:/UI/Texture/WindowIcons/items.png"
		/// "res:/ui/Texture/WindowIcons/market.png"
		/// </summary>
		public Sprite[] Button;

		public UIElementText Clock;

		public Neocom()
		{
		}

		public Neocom(UIElement Base)
			: base(Base)
		{
		}
	}


	public class TabGroup : UIElement
	{
		public Tab[] ListTab;

		public TabGroup()
		{
		}

		public TabGroup(UIElement Base)
			:
			base(Base)
		{
		}
	}


	public class Tab : UIElement
	{
		public UIElementText Label;

		public int? LabelColorOpacityMilli;

		public int? BackgroundOpacityMilli;

		public Tab()
		{
		}

		public Tab(
			UIElement Base,
			UIElementText Label,
			int? LabelColorOpacityMilli,
			int? BackgroundOpacityMilli)
			:
			base(Base)
		{
			this.Label = Label;
			this.LabelColorOpacityMilli = LabelColorOpacityMilli;
			this.BackgroundOpacityMilli = BackgroundOpacityMilli;
		}
	}

	public class WindowSelectedItemView : Window
	{
		public Sprite[] ActionSprite;

		public WindowSelectedItemView(
			Window Base)
			:
			base(Base)
		{
		}

		public WindowSelectedItemView()
		{
		}
	}

	public class TreeViewEntry : UIElement
	{
		public UIElement TopContRegion;

		public UIElement ExpandCollapseToggleRegion;

		public UIElementText TopContLabel;

		public ColorORGB TopContIconColor;

		public string LabelText;

		public ObjectIdInMemory TopContIconType;

		public TreeViewEntry[] Child;

		public bool? IsSelected;

		public TreeViewEntry()
		{
		}

		public TreeViewEntry(UIElement Base)
			:
			base(Base)
		{
		}
	}



	public class Scroll : UIElement
	{
		public ColumnHeader[] ListColumnHeader;

		public UIElement Clipper;

		public UIElement ScrollHandleBound;

		public UIElement ScrollHandle;

		public Scroll()
			:
			this((Scroll)null)
		{
		}

		public Scroll(UIElement Base)
			:
			base(Base)
		{
		}

		public Scroll(Scroll Base)
			:
			this((UIElement)Base)
		{
			ListColumnHeader = Base?.ListColumnHeader;
			Clipper = Base?.Clipper;
			ScrollHandleBound = Base?.ScrollHandleBound;
			ScrollHandle = Base?.ScrollHandle;
		}
	}


	public class PanelGroup : UIElement
	{
		public PanelGroup()
		{
		}

		public PanelGroup(UIElement Base)
			:
			base(Base)
		{
		}
	}


	/// <summary>
	/// appears in some Missions.
	/// </summary>
	public class WindowTelecom : Window
	{
		public WindowTelecom(Window Window)
			:
			base(Window)
		{
		}

		public WindowTelecom()
		{
		}
	}


	public class MessageBox : Window
	{
		public string TopCaptionText;

		public string MainEditText;

		public MessageBox(Window Base)
			:
			base(Base)
		{
		}

		public MessageBox()
		{
		}
	}

	public class HybridWindow : MessageBox
	{
		public HybridWindow(MessageBox Base)
			:
			base(Base)
		{
		}

		public HybridWindow()
		{
		}
	}



}

