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

	public class Neocom : UIElement
	{
		public IUIElement EveMenuButton;

		public IUIElement CharButton;

		/// <summary>
		/// The type can be identified by TexturePath.
		/// Some examples:
		/// "res:/ui/Texture/WindowIcons/peopleandplaces.png"
		/// "res:/UI/Texture/WindowIcons/items.png"
		/// "res:/ui/Texture/WindowIcons/market.png"
		/// </summary>
		public ISprite[] Button;

		public IUIElementText Clock;

		public Neocom()
		{
		}

		public Neocom(IUIElement Base)
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

		public TabGroup(IUIElement Base)
			:
			base(Base)
		{
		}
	}


	public class Tab : UIElement
	{
		public IUIElementText Label;

		public int? LabelColorOpacityMilli;

		public int? BackgroundOpacityMilli;

		public Tab()
		{
		}

		public Tab(IUIElement Base)
			:
			base(Base)
		{
		}
	}


	public class PanelGroup : UIElement
	{
		public PanelGroup()
		{
		}

		public PanelGroup(IUIElement Base)
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
		public WindowTelecom(IWindow Window)
			:
			base(Window)
		{
		}

		public WindowTelecom()
		{
		}
	}

}

