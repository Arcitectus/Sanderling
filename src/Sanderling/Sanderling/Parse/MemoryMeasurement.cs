using Bib3;
using BotEngine.Common;
using System.Linq;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Parse
{
	public interface IMemoryMeasurement : MemoryStruct.IMemoryMeasurement
	{
		new IModuleButtonTooltip ModuleButtonTooltip { get; }

		new IWindowInventory[] WindowInventory { get; }

		new INeocom Neocom { get; }

		bool? IsDocked { get; }

		bool? IsUnDocking { get; }
	}

	public class MemoryMeasurement : IMemoryMeasurement
	{
		MemoryStruct.IMemoryMeasurement Raw;

		public IModuleButtonTooltip ModuleButtonTooltip { set; get; }

		public IWindowInventory[] WindowInventory { set; get; }

		public INeocom Neocom { set; get; }

		public MemoryStruct.IUIElementText[] AbovemainMessage => Raw?.AbovemainMessage;

		public MemoryStruct.PanelGroup[] AbovemainPanelEveMenu => Raw?.AbovemainPanelEveMenu;

		public MemoryStruct.PanelGroup[] AbovemainPanelGroup => Raw?.AbovemainPanelGroup;

		public MemoryStruct.IUIElement InfoPanelButtonIncursions => Raw?.InfoPanelButtonIncursions;

		public MemoryStruct.IUIElement InfoPanelButtonCurrentSystem => Raw?.InfoPanelButtonCurrentSystem;

		public MemoryStruct.IUIElement InfoPanelButtonMissions => Raw?.InfoPanelButtonMissions;

		public MemoryStruct.IUIElement InfoPanelButtonRoute => Raw?.InfoPanelButtonRoute;

		public MemoryStruct.InfoPanelCurrentSystem InfoPanelCurrentSystem => Raw?.InfoPanelCurrentSystem;

		public MemoryStruct.InfoPanelMissions InfoPanelMissions => Raw?.InfoPanelMissions;

		public MemoryStruct.InfoPanelRoute InfoPanelRoute => Raw?.InfoPanelRoute;

		public MemoryStruct.IMenu[] Menu => Raw?.Menu;

		MemoryStruct.INeocom MemoryStruct.IMemoryMeasurement.Neocom => Neocom;

		public MemoryStruct.IShipUi ShipUi => Raw?.ShipUi;

		public MemoryStruct.IWindow SystemMenu => Raw?.SystemMenu;

		public MemoryStruct.IShipUiTarget[] Target => Raw?.Target;

		public MemoryStruct.IContainer[] Utilmenu => Raw?.Utilmenu;

		public string VersionString => Raw?.VersionString;

		public MemoryStruct.WindowAgentBrowser[] WindowAgentBrowser => Raw?.WindowAgentBrowser;

		public MemoryStruct.IWindowAgentDialogue[] WindowAgentDialogue => Raw?.WindowAgentDialogue;

		public MemoryStruct.WindowChatChannel[] WindowChatChannel => Raw?.WindowChatChannel;

		public MemoryStruct.IWindowDroneView[] WindowDroneView => Raw?.WindowDroneView;

		public MemoryStruct.WindowFittingMgmt[] WindowFittingMgmt => Raw?.WindowFittingMgmt;

		public MemoryStruct.WindowShipFitting[] WindowShipFitting => Raw?.WindowShipFitting;

		MemoryStruct.IWindowInventory[] MemoryStruct.IMemoryMeasurement.WindowInventory => WindowInventory;

		public MemoryStruct.WindowItemSell[] WindowItemSell => Raw?.WindowItemSell;

		public MemoryStruct.WindowMarketAction[] WindowMarketAction => Raw?.WindowMarketAction;

		public MemoryStruct.IWindow[] WindowOther => Raw?.WindowOther;

		public MemoryStruct.IWindowOverview[] WindowOverview => Raw?.WindowOverview;

		public MemoryStruct.WindowPeopleAndPlaces[] WindowPeopleAndPlaces => Raw?.WindowPeopleAndPlaces;

		public MemoryStruct.WindowRegionalMarket[] WindowRegionalMarket => Raw?.WindowRegionalMarket;

		public MemoryStruct.IWindowSelectedItemView[] WindowSelectedItemView => Raw?.WindowSelectedItemView;

		public MemoryStruct.WindowStack[] WindowStack => Raw?.WindowStack;

		public MemoryStruct.IWindowStation[] WindowStation => Raw?.WindowStation;

		public MemoryStruct.IWindowSurveyScanView[] WindowSurveyScanView => Raw?.WindowSurveyScanView;

		public MemoryStruct.WindowTelecom[] WindowTelecom => Raw?.WindowTelecom;

		MemoryStruct.IContainer MemoryStruct.IMemoryMeasurement.ModuleButtonTooltip => ModuleButtonTooltip;

		public bool? IsDocked { private set; get; }

		public bool? IsUnDocking { private set; get; }

		public MemoryMeasurement(MemoryStruct.IMemoryMeasurement Raw)
		{
			this.Raw = Raw;

			if (null == Raw)
			{
				return;
			}

			ModuleButtonTooltip = Raw?.ModuleButtonTooltip?.ParseAsModuleButtonTooltip();

			var ShipUi = Raw?.ShipUi;

			var SetWindowStation = Raw?.WindowStation;

			if (!SetWindowStation.IsNullOrEmpty())
			{
				IsDocked = true;
			}

			if (null != ShipUi ||
				(Raw?.WindowOverview?.WhereNotDefault()?.Any() ?? false))
			{
				IsDocked = false;
			}

			WindowInventory = Raw?.WindowInventory?.Select(InventoryExtension.Parse)?.ToArray();

			if (SetWindowStation?.Any(WindowStationLobby => WindowStationLobby?.LabelText?.Any(LabelText =>
				 LabelText?.Text?.RegexMatchSuccess(@"abort\s*undock|undocking") ?? false) ?? false) ?? false)
			{
				IsUnDocking = true;
			}

			Neocom = Raw?.Neocom?.Parse();
        }
	}
}
