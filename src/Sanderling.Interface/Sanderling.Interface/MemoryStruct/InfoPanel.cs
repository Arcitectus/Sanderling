using System;

namespace Sanderling.Interface.MemoryStruct
{
	public interface IInfoPanel : IUIElement, IExpandable
	{
		IContainer HeaderContent { get; }

		/// <summary>
		/// content which is only visible when expanded.
		/// </summary>
		IContainer ExpandedContent { get; }
	}

	public class InfoPanel : UIElement, IInfoPanel, IUIElementText
	{
		public bool? IsExpanded { set; get; }

		public IUIElement ExpandToggleButton { set; get; }

		public IContainer HeaderContent { set; get; }

		public IContainer ExpandedContent { set; get; }

		public string Text => HeaderContent?.LabelText?.Largest()?.Text;

		public InfoPanel()
		{
		}

		public InfoPanel(IUIElement Base)
			:
			base(Base)
		{
		}

		public InfoPanel(IInfoPanel Base)
			:
			this((IUIElement)Base)
		{
			IsExpanded = Base?.IsExpanded;
			ExpandToggleButton = Base?.ExpandToggleButton;
			HeaderContent = Base?.HeaderContent;
			ExpandedContent = Base?.ExpandedContent;
		}
	}

	public class InfoPanelCurrentSystem : InfoPanel
	{
		public IUIElement ListSurroundingsButton;

		public InfoPanelCurrentSystem()
		{
		}

		public InfoPanelCurrentSystem(IInfoPanel Base)
			:
			base(Base)
		{
		}
	}


	public class InfoPanelRoute : InfoPanel
	{
		public IUIElementText NextLabel;

		public IUIElementText DestinationLabel;

		public IUIElement[] RouteElementMarker;

		public InfoPanelRoute()
		{
		}

		public InfoPanelRoute(IInfoPanel Base)
			:
			base(Base)
		{
		}
	}

	public class InfoPanelMissions : InfoPanel
	{
		public IUIElementText[] ListMissionButton;

		public InfoPanelMissions()
		{
		}

		public InfoPanelMissions(IInfoPanel Base)
			:
			base(Base)
		{
		}
	}


}
