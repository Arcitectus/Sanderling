using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Parse
{
	public interface IMemoryMeasurement : MemoryStruct.IMemoryMeasurement
	{
		new IModuleButtonTooltip ModuleButtonTooltip { get; }
	}

	public class MemoryMeasurement : IMemoryMeasurement
	{
		MemoryStruct.IMemoryMeasurement Raw;

		public IModuleButtonTooltip ModuleButtonTooltip { set; get; }

		public MemoryStruct.IUIElementText[] AbovemainMessage => Raw?.AbovemainMessage;

		public MemoryStruct.PanelGroup[] AbovemainPanelEveMenu => Raw?.AbovemainPanelEveMenu;

		public MemoryStruct.PanelGroup[] AbovemainPanelGroup => Raw?.AbovemainPanelGroup;

		public MemoryStruct.IUIElement InfoPanelButtonIncursions => Raw?.InfoPanelButtonIncursions;

		public MemoryStruct.IUIElement InfoPanelButtonLocationInfo => Raw?.InfoPanelButtonLocationInfo;

		public MemoryStruct.IUIElement InfoPanelButtonMissions => Raw?.InfoPanelButtonMissions;

		public MemoryStruct.IUIElement InfoPanelButtonRoute => Raw?.InfoPanelButtonRoute;

		public MemoryStruct.InfoPanelCurrentSystem InfoPanelLocationInfo => Raw?.InfoPanelLocationInfo;

		public MemoryStruct.InfoPanelMissions InfoPanelMissions => Raw?.InfoPanelMissions;

		public MemoryStruct.InfoPanelRoute InfoPanelRoute => Raw?.InfoPanelRoute;

		public MemoryStruct.IMenu[] Menu => Raw?.Menu;

		public MemoryStruct.Neocom Neocom => Raw?.Neocom;

		public MemoryStruct.IShipUi ShipUi => Raw?.ShipUi;

		public MemoryStruct.IWindow SystemMenu => Raw?.SystemMenu;

		public MemoryStruct.ShipUiTarget[] Target => Raw?.Target;

		public MemoryStruct.IContainer[] Utilmenu => Raw?.Utilmenu;

		public string VersionString => Raw?.VersionString;

		public MemoryStruct.WindowAgentBrowser[] WindowAgentBrowser => Raw?.WindowAgentBrowser;

		public MemoryStruct.IWindowAgentDialogue[] WindowAgentDialogue => Raw?.WindowAgentDialogue;

		public MemoryStruct.WindowChatChannel[] WindowChatChannel => Raw?.WindowChatChannel;

		public MemoryStruct.IWindowDroneView[] WindowDroneView => Raw?.WindowDroneView;

		public MemoryStruct.WindowFittingMgmt[] WindowFittingMgmt => Raw?.WindowFittingMgmt;

		public MemoryStruct.WindowFittingWindow[] WindowFittingWindow => Raw?.WindowFittingWindow;

		public MemoryStruct.IWindowInventory[] WindowInventory => Raw?.WindowInventory;

		public MemoryStruct.WindowItemSell[] WindowItemSell => Raw?.WindowItemSell;

		public MemoryStruct.WindowMarketAction[] WindowMarketAction => Raw?.WindowMarketAction;

		public MemoryStruct.IWindow[] WindowOther => Raw?.WindowOther;

		public MemoryStruct.IWindowOverview[] WindowOverview => Raw?.WindowOverview;

		public MemoryStruct.WindowPeopleAndPlaces[] WindowPeopleAndPlaces => Raw?.WindowPeopleAndPlaces;

		public MemoryStruct.WindowRegionalMarket[] WindowRegionalMarket => Raw?.WindowRegionalMarket;

		public MemoryStruct.IWindowSelectedItemView[] WindowSelectedItemView => Raw?.WindowSelectedItemView;

		public MemoryStruct.WindowStack[] WindowStack => Raw?.WindowStack;

		public MemoryStruct.WindowStationLobby[] WindowStationLobby => Raw?.WindowStationLobby;

		public MemoryStruct.WindowSurveyScanView[] WindowSurveyScanView => Raw?.WindowSurveyScanView;

		public MemoryStruct.WindowTelecom[] WindowTelecom => Raw?.WindowTelecom;

		MemoryStruct.IContainer MemoryStruct.IMemoryMeasurement.ModuleButtonTooltip => ModuleButtonTooltip;

		public MemoryMeasurement(MemoryStruct.IMemoryMeasurement Raw)
		{
			this.Raw = Raw;

			if (null == Raw)
			{
				return;
			}

			ModuleButtonTooltip = Raw?.ModuleButtonTooltip?.ParseAsModuleButtonTooltip();
        }
	}
}
