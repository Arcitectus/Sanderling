namespace Sanderling.Interface.MemoryStruct
{
	public interface IInfoPanel : IUIElement, IExpandable
	{
		IContainer HeaderContent { get; }

		/// <summary>
		/// content which is only visible when expanded.
		/// </summary>
		IContainer ExpandedContent { get; }

		string HeaderText { get; }
    }

	public interface IInfoPanelSystem : IInfoPanel
	{
		IUIElement ListSurroundingsButton { get; }
	}

	public interface IInfoPanelRoute : IInfoPanel
	{
		IUIElementText NextLabel { get; }

		IUIElementText DestinationLabel { get; }

		IUIElement[] RouteElementMarker { get; }
	}

	public interface IInfoPanelMissions : IInfoPanel
	{
		IUIElementText[] ListMissionButton { get; }
	}

	public class InfoPanel : UIElement, IInfoPanel
	{
		public bool? IsExpanded { set; get; }

		public IUIElement ExpandToggleButton { set; get; }

		public IContainer HeaderContent { set; get; }

		public IContainer ExpandedContent { set; get; }

		public string HeaderText => HeaderContent?.LabelText?.Largest()?.Text;

		public InfoPanel()
		{
		}

		public InfoPanel(IUIElement @base)
			:
			base(@base)
		{
		}

		public InfoPanel(IInfoPanel @base)
			:
			this((IUIElement)@base)
		{
			IsExpanded = @base?.IsExpanded;
			ExpandToggleButton = @base?.ExpandToggleButton;
			HeaderContent = @base?.HeaderContent;
			ExpandedContent = @base?.ExpandedContent;
		}
	}

	public class InfoPanelSystem : InfoPanel, IInfoPanelSystem
	{
		public IUIElement ListSurroundingsButton { set; get; }

		public InfoPanelSystem()
		{
		}

		public InfoPanelSystem(IInfoPanel @base)
			:
			base(@base)
		{
		}
	}

	public class InfoPanelRoute : InfoPanel, IInfoPanelRoute
	{
		public IUIElementText NextLabel { set; get; }

		public IUIElementText DestinationLabel { set; get; }

		public IUIElement[] RouteElementMarker { set; get; }

		public InfoPanelRoute()
		{
		}

		public InfoPanelRoute(IInfoPanel @base)
			:
			base(@base)
		{
		}
	}

	public class InfoPanelMissions : InfoPanel, IInfoPanelMissions
	{
		public IUIElementText[] ListMissionButton { set; get; }

		public InfoPanelMissions()
		{
		}

		public InfoPanelMissions(IInfoPanel @base)
			:
			base(@base)
		{
		}
	}

}
