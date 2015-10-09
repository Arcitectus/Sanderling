namespace Sanderling.Interface.MemoryStruct
{
	public class InfoPanel : UIElement
	{
		public bool? IsExpanded;

		public UIElement HeaderButtonExpand;

		public UIElementText HeaderLabel;

		/// <summary>
		/// content which is only visible when expanded.
		/// </summary>
		public UIElementText[] ExpandedContentLabel;

		public InfoPanel()
		{
		}

		public InfoPanel(UIElement Base)
			:
			base(Base)
		{
		}

		public InfoPanel(InfoPanel Base)
			:
			this((UIElement)Base)
		{
			IsExpanded = Base?.IsExpanded;
			HeaderButtonExpand = Base?.HeaderButtonExpand;
			HeaderLabel = Base?.HeaderLabel;
			ExpandedContentLabel = Base?.ExpandedContentLabel;
		}
	}

	public class InfoPanelLocationInfo : InfoPanel
	{
		public UIElement ButtonListSurroundings;

		public InfoPanelLocationInfo()
		{
		}

		public InfoPanelLocationInfo(InfoPanel Base)
			:
			base(Base)
		{
		}
	}


	public class InfoPanelRoute : InfoPanel
	{
		public UIElementText NextLabel;

		public UIElementText DestinationLabel;

		public UIElement[] WaypointMarker;

		public InfoPanelRoute()
		{
		}

		public InfoPanelRoute(InfoPanel Base)
			:
			base(Base)
		{
		}
	}

	public class InfoPanelMissions : InfoPanel
	{
		public UIElementText[] ListMissionButton;

		public InfoPanelMissions()
		{
		}

		public InfoPanelMissions(InfoPanel Base)
			:
			base(Base)
		{
		}
	}


}
