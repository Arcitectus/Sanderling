using System;

namespace Sanderling.Interface.MemoryStruct
{
	/// <summary>
	/// Root of the graph of objects read from memory that is delivered from the sensor to the consuming application.
	/// </summary>
	public class MemoryMeasurement : ICloneable
	{
		public string VersionString;

		public Menu[] Menu;

		public ShipUi ShipUi;

		public ShipUiTarget[] Target;

		/// <summary>
		/// shown when hovering mouse cursor over module.
		/// </summary>
		public ModuleButtonTooltip ModuleButtonTooltip;

		/// <summary>
		/// Occludes all other UIElements and can usually be opened and closed by pressing ESC.
		/// </summary>
		public SystemMenu SystemMenu;

		public Neocom Neocom;

		public UIElement InfoPanelButtonLocationInfo;

		public UIElement InfoPanelButtonRoute;

		public UIElement InfoPanelButtonMissions;

		public UIElement InfoPanelButtonIncursions;

		public InfoPanelLocationInfo InfoPanelLocationInfo;

		public InfoPanelRoute InfoPanelRoute;

		public InfoPanelMissions InfoPanelMissions;

		public Utilmenu[] Utilmenu;

		public UIElementText[] AbovemainMessage;

		public PanelGroup[] AbovemainPanelGroup;

		public PanelGroup[] AbovemainPanelEveMenu;

		public Window[] WindowOther;

		public WindowStack[] WindowStack;

		public WindowOverView[] WindowOverview;

		public WindowChatChannel[] WindowChatChannel;

		public WindowSelectedItemView[] WindowSelectedItemView;

		public WindowDroneView[] WindowDroneView;

		public WindowPeopleAndPlaces[] WindowPeopleAndPlaces;

		public WindowStationLobby[] WindowStationLobby;

		public WindowFittingWindow[] WindowFittingWindow;

		public WindowFittingMgmt[] WindowFittingMgmt;

		public WindowSurveyScanView[] WindowSurveyScanView;

		public WindowInventory[] WindowInventory;

		public WindowAgentDialogue[] WindowAgentDialogue;

		public WindowAgentBrowser[] WindowAgentBrowser;

		public WindowTelecom[] WindowTelecom;

		public WindowRegionalMarket[] WindowRegionalMarket;

		public WindowMarketAction[] WindowMarketAction;

		public WindowItemSell[] WindowItemSell;

		public MemoryMeasurement Copy() => this.CopyByPolicyMemoryMeasurement();

		public object Clone()
		{
			return Copy();
		}
	}
}
