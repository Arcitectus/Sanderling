using BotEngine.Interface;

namespace Sanderling.Interface.MemoryStruct
{
	static public class ToStringStatic
	{
		static public string SensorObjectToString(this object obj)
		{
			return
				(obj as string) ??
				obj?.GetType()?.ToString();
		}
	}

	public class TabGroup : UIElement
	{
		public Tab[] ListTab;

		public TabGroup()
		{
		}

		public TabGroup(IUIElement @base)
			:
			base(@base)
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

		public Tab(IUIElement @base)
			:
			base(@base)
		{
		}
	}


	public class PanelGroup : Container
	{
		public PanelGroup()
		{
		}

		public PanelGroup(IUIElement @base)
			:
			base(@base)
		{
		}
	}


	/// <summary>
	/// appears in some Missions.
	/// </summary>
	public class WindowTelecom : Window
	{
		public WindowTelecom(IWindow window)
			:
			base(window)
		{
		}

		public WindowTelecom()
		{
		}
	}
}

