/*
This bot mines ore from asteroids and offloads the ore in a station.

Before running this bot, prepare the EVE online client as follows:

+ Set the UI language to english.
+ Move your mining ship to a solar system which has asteroid belts and at least one station in which you can dock.
+ In the Overview, create a preset which includes asteroids and rats and enter the name of that preset in the configuration section below at 'OverviewPreset'. The bot will make sure this preset is loaded when it needs to use the overview.
+ Set Overview to sort by distance with the nearest entry at the top.
+ In the Inventory, select the 'List' view.
+ Enable the info panel 'System info'. The bot needs this to find asteroid belts and stations.
+ Arrange windows to not occlude ship modules or info panels.
+ In the ship UI, disable "Display Passive Modules" and disable "Display Empty Slots" and enable "Display Module Tooltips". The bot uses the module tooltips to automatically identify the properties of the modules.

This bot tracks the total mined amount of ore by watching inventory volume while mining modules are active.
*/

using BotSharp.ToScript.Extension;
using MemoryStruct = Sanderling.Interface.MemoryStruct;
using Parse = Sanderling.Parse;

//	Begin of configuration section ->

//	The bot loads this preset to the active tab in the Overview window.
string OverviewPreset = null;

//	Activate shield hardener.
var ActivateHardener = true;

//	Bot will switch mining site when rats are visible and shield hitpoints are lower than this value.
var SwitchMiningSiteHitpointThresholdPercent = 85;

var EmergencyWarpOutHitpointPercent = 60;

//	Percentage of fill level at which to enter the offload process.
var EnterOffloadOreContainerFillPercent = 95;

//	Warp to station when a neutral or hostile is visible in local.
var RetreatOnNeutralOrHostileInLocal = false;

//	Name of the container to unload to as shown in inventory.
string UnloadDestContainerName = "Item Hangar";

//	When this is set to true, the bot will try to unload when undocked.
bool UnloadInSpace = false;

//	<- End of configuration section


Func<object> BotStopActivity = () => null;

Func<object> NextActivity = MainStep;

Queue<string> visitedLocations = new Queue<string>();

// State used to track total mined ore. -->
struct StatisticsMeasurementSnapshot
{
	public Int64 timeMilli;
	public Int64 volumeMilli;
}
readonly Queue<StatisticsMeasurementSnapshot> efficiencyRawMeasurements = new Queue<StatisticsMeasurementSnapshot>(); 
readonly Queue<StatisticsMeasurementSnapshot> efficiencyDebouncedMeasurements = new Queue<StatisticsMeasurementSnapshot>(); 
Int64 miningModuleLastActiveTime = 0;
Int64 totalMinedOreVolumeMilli = 0;
// <-- State used to track total mined ore.

for(;;)
{
	var stepBeginTimeMilli = Host.GetTimeContinuousMilli();

	MemoryUpdate();

	Host.Log(
		"ore container fill: " + OreContainerFillPercent + "%" +
		", mining range: " + MiningRange +
		", mining modules (inactive): " + SetModuleMiner?.Length + "(" + SetModuleMinerInactive?.Length + ")" +
		", shield.hp: " + ShieldHpPercent + "%" +
		", retreat: " + RetreatReason + 
		", JLA: " + JammedLastAge +
		", overview.rats: " + ListRatOverviewEntry?.Length +
		", overview.roids: " + ListAsteroidOverviewEntry?.Length +
		", offload count: " + OffloadCount +
		", nextAct: " + NextActivity?.Method?.Name);

	CloseModalUIElement();

	if(0 < RetreatReason?.Length && !(Measurement?.IsDocked ?? false))
	{
		InitiateWarpToRetreat();
		continue;
	}

	NextActivity = NextActivity?.Invoke() as Func<object>;

	if(BotStopActivity == NextActivity)
		break;

	if(null == NextActivity)
		NextActivity = MainStep;

	Host.Delay((int)Math.Max(0, 1000 - (Host.GetTimeContinuousMilli() - stepBeginTimeMilli)));
}

bool? ShipHasOreHold
{
	get
	{
		var	inventoryActiveShipEntry = WindowInventory?.ActiveShipEntry;

		//	If the tree entry for the ship is not expanded....
		if(!(IsExpanded(inventoryActiveShipEntry) ?? false))
			return null;	// Then I do not know if there is an ore hold.

		return inventoryActiveShipEntry?.TreeEntryFromCargoSpaceType(ShipCargoSpaceTypeEnum.OreHold) != null;
	}
}
	

//	seconds since ship was jammed.
long? JammedLastAge => Jammed ? 0 : (Host.GetTimeContinuousMilli() - JammedLastTime) / 1000;

int?	ShieldHpPercent => ShipUi?.HitpointsAndEnergy?.Shield / 10;

bool	ShouldSwitchMiningSite =>
	!(Measurement?.IsDocked ?? false) &&
	0 < ListRatOverviewEntry?.Length &&
	!(SwitchMiningSiteHitpointThresholdPercent < ShieldHpPercent) || JammedLastAge < 10;

bool	OreContainerFilledForOffload => Math.Max(0, Math.Min(100, EnterOffloadOreContainerFillPercent)) <= OreContainerFillPercent;

Int64?	JammedLastTime = null;
string RetreatReasonTemporary = null;
string RetreatReasonPermanent = null;
string RetreatReason => RetreatReasonPermanent ?? RetreatReasonTemporary;
int? LastCheckOreContainerFillPercent = null;

int OffloadCount = 0;

Func<object>	MainStep()
{
	if(Measurement?.IsDocked ?? false)
	{
		InInventoryUnloadItems();

		if (0 < RetreatReasonPermanent?.Length)
			return BotStopActivity;

		if (0 < RetreatReason?.Length)
			return MainStep;

		Undock();
	}

	EnsureOverviewTypeSelectionLoaded();

	EnsureWindowInventoryOreContainerIsOpen();

	if(ReadyForManeuver)
	{
		if(OreContainerFilledForOffload)
		{
			if(ReadyForManeuver)
				InitiateDockToOffload();

			if (UnloadInSpace)
			{
				Host.Delay(4444);
				InInventoryUnloadItems();
			}

			return MainStep;
		}

		if(!(0 < ListAsteroidOverviewEntry?.Length) || ShouldSwitchMiningSite)
			InitiateWarpToMiningSite();
	}

	ModuleMeasureAllTooltip();

	if(ActivateHardener)
		ActivateHardenerExecute();

	return InBeltMineStep;
}

int RandomInt() => new Random((int)Host.GetTimeContinuousMilli()).Next();

T RandomElement<T>(IEnumerable<T> sequence)
{
	var array = (sequence as T[]) ?? sequence?.ToArray();

	if (!(0 < array?.Length))
		return default(T);

	return array[RandomInt() % array.Length];
}

void CloseModalUIElement()
{
	var	ButtonClose =
		ModalUIElement?.ButtonText?.FirstOrDefault(button => (button?.Text).RegexMatchSuccessIgnoreCase("close|no|ok"));

	Sanderling.MouseClickLeft(ButtonClose);
}

Func<object> InBeltMineStep()
{
	if(ShouldSwitchMiningSite)
		return MainStep;

	EnsureWindowInventoryOreContainerIsOpen();

	EnsureOverviewTypeSelectionLoaded();

	if(OreContainerFilledForOffload)
		return null;

	var moduleMinerInactive = SetModuleMinerInactive?.FirstOrDefault();

	if (null == moduleMinerInactive)
	{
		DelayWhileUpdatingMemory(6000);
		return InBeltMineStep;
	}

	var	setTargetAsteroidInRange	=
		SetTargetAsteroid?.Where(target => target?.DistanceMax <= MiningRange)?.ToArray();

	var setTargetAsteroidInRangeNotAssigned =
		setTargetAsteroidInRange?.Where(target => !(0 < target?.Assigned?.Length))?.ToArray();

	Host.Log("targeted asteroids in range (without assignment): " + setTargetAsteroidInRange?.Length + " (" + setTargetAsteroidInRangeNotAssigned?.Length + ")");

	if(0 < setTargetAsteroidInRangeNotAssigned?.Length)
	{
		var targetAsteroidInputFocus	=
			setTargetAsteroidInRangeNotAssigned?.FirstOrDefault(target => target?.IsSelected ?? false);

		if(null == targetAsteroidInputFocus)
			Sanderling.MouseClickLeft(setTargetAsteroidInRangeNotAssigned?.FirstOrDefault());

		ModuleToggle(moduleMinerInactive);

		return InBeltMineStep;
	}

	var asteroidOverviewEntryNext = ListAsteroidOverviewEntry?.FirstOrDefault();
	var asteroidOverviewEntryNextNotTargeted = ListAsteroidOverviewEntry?.FirstOrDefault(entry => !((entry?.MeTargeted ?? false) || (entry?.MeTargeting ?? false)));

	Host.Log("next asteroid: (" + asteroidOverviewEntryNext?.Name + " , distance: " + asteroidOverviewEntryNext?.DistanceMax + ")" + 
		", next asteroid not targeted: (" + asteroidOverviewEntryNextNotTargeted?.Name + " , distance: " + asteroidOverviewEntryNextNotTargeted?.DistanceMax + ")");

	if(null == asteroidOverviewEntryNext)
	{
		Host.Log("no asteroid available");
		return null;
	}

	if(null == asteroidOverviewEntryNextNotTargeted)
	{
		Host.Log("all asteroids targeted");
		return null;
	}

	if (!(asteroidOverviewEntryNextNotTargeted.DistanceMax < MiningRange))
	{
		if(!(1111 < asteroidOverviewEntryNext?.DistanceMin))
		{
			Host.Log("distance between asteroids too large");
			return null;
		}

		Host.Log("out of range, approaching");
		ClickMenuEntryOnMenuRoot(asteroidOverviewEntryNext, "approach");
	}
	else
	{
		Host.Log("initiate lock asteroid");
		ClickMenuEntryOnMenuRoot(asteroidOverviewEntryNextNotTargeted, "^lock");
	}
	
	return InBeltMineStep;
}


Sanderling.Parse.IMemoryMeasurement	Measurement	=>
	Sanderling?.MemoryMeasurementParsed?.Value;

IWindow ModalUIElement =>
	Measurement?.EnumerateReferencedUIElementTransitive()?.OfType<IWindow>()?.Where(window => window?.isModal ?? false)
	?.OrderByDescending(window => window?.InTreeIndex ?? int.MinValue)
	?.FirstOrDefault();	

IEnumerable<Parse.IMenu> Menu => Measurement?.Menu;

Parse.IShipUi ShipUi => Measurement?.ShipUi;

bool Jammed => ShipUi?.EWarElement?.Any(EwarElement => (EwarElement?.EWarType).RegexMatchSuccess("electronic")) ?? false;

Sanderling.Interface.MemoryStruct.IMenuEntry MenuEntryLockTarget =>
	Menu?.FirstOrDefault()?.Entry?.FirstOrDefault(entry => entry.Text.RegexMatchSuccessIgnoreCase("^lock"));

Sanderling.Parse.IWindowOverview	WindowOverview	=>
	Measurement?.WindowOverview?.FirstOrDefault();

Sanderling.Parse.IWindowInventory	WindowInventory	=>
	Measurement?.WindowInventory?.FirstOrDefault();

ITreeViewEntry InventoryActiveShipOreContainer
{
	get
	{
		var	hasOreHold = ShipHasOreHold;

		if(hasOreHold == null)
			return null;

		return
			WindowInventory?.ActiveShipEntry?.TreeEntryFromCargoSpaceType(
				hasOreHold.Value ? ShipCargoSpaceTypeEnum.OreHold : ShipCargoSpaceTypeEnum.General);
	}
}

IInventoryCapacityGauge OreContainerCapacityMilli =>
	(InventoryActiveShipOreContainer?.IsSelected ?? false) ? WindowInventory?.SelectedRightInventoryCapacityMilli : null;

int? OreContainerFillPercent => (int?)((OreContainerCapacityMilli?.Used * 100) / OreContainerCapacityMilli?.Max);

Tab OverviewPresetTabActive =>
	WindowOverview?.PresetTab
	?.OrderByDescending(tab => tab?.LabelColorOpacityMilli ?? 0)
	?.FirstOrDefault();

string OverviewTypeSelectionName =>
	WindowOverview?.Caption?.RegexMatchIfSuccess(@"\(([^\)]*)\)")?.Groups?[1]?.Value;

Parse.IOverviewEntry[] ListRatOverviewEntry => WindowOverview?.ListView?.Entry?.Where(entry =>
		(entry?.MainIconIsRed ?? false)	&& (entry?.IsAttackingMe ?? false))
		?.OrderBy(entry => entry?.DistanceMax ?? int.MaxValue)
		?.ToArray();

Parse.IOverviewEntry[] ListAsteroidOverviewEntry =>
	WindowOverview?.ListView?.Entry
	?.Where(entry => null != OreTypeFromAsteroidName(entry?.Name))
	?.OrderBy(entry => entry.DistanceMax ?? int.MaxValue)
	?.ToArray();
	

bool ReadyForManeuverNot =>
	Measurement?.ShipUi?.Indication?.LabelText?.Any(indicationLabel =>
		(indicationLabel?.Text).RegexMatchSuccessIgnoreCase("warp|docking")) ?? false;

bool ReadyForManeuver => !ReadyForManeuverNot && !(Measurement?.IsDocked ?? true);

Sanderling.Parse.IShipUiTarget[] SetTargetAsteroid =>
	Measurement?.Target?.Where(target =>
		target?.TextRow?.Any(textRow => textRow.RegexMatchSuccessIgnoreCase("asteroid")) ?? false)?.ToArray();

Sanderling.Interface.MemoryStruct.IListEntry	WindowInventoryItem	=>
	WindowInventory?.SelectedRightInventory?.ListView?.Entry?.FirstOrDefault();

Sanderling.Accumulation.IShipUiModule[] SetModuleMiner =>
	Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Where(module => module?.TooltipLast?.Value?.IsMiner ?? false)?.ToArray();

Sanderling.Accumulation.IShipUiModule[] SetModuleMinerInactive	 =>
	SetModuleMiner?.Where(module => !(module?.RampActive ?? false))?.ToArray();

int?	MiningRange => SetModuleMiner?.Select(module =>
	module?.TooltipLast?.Value?.RangeOptimal ?? module?.TooltipLast?.Value?.RangeMax ?? module?.TooltipLast?.Value?.RangeWithin ?? 0)?.DefaultIfEmpty(0)?.Min();;

WindowChatChannel chatLocal =>
	 Sanderling.MemoryMeasurementParsed?.Value?.WindowChatChannel
	 ?.FirstOrDefault(windowChat => windowChat?.Caption?.RegexMatchSuccessIgnoreCase("local") ?? false);

//    assuming that own character is always visible in local
bool hostileOrNeutralsInLocal => 1 != chatLocal?.ParticipantView?.Entry?.Count(IsNeutralOrEnemy);

//	extract the ore type from the name as seen in overview. "Asteroid (Plagioclase)"
string OreTypeFromAsteroidName(string AsteroidName)	=>
	AsteroidName.ValueFromRegexMatchGroupAtIndex(@"Asteroid \(([^\)]+)", 0);

void ClickMenuEntryOnMenuRoot(IUIElement MenuRoot, string MenuEntryRegexPattern)
{
	Sanderling.MouseClickRight(MenuRoot);
	
	var Menu = Measurement?.Menu?.FirstOrDefault();
	
	var	MenuEntry = Menu?.EntryFirstMatchingRegexPattern(MenuEntryRegexPattern, RegexOptions.IgnoreCase);
	
	Sanderling.MouseClickLeft(MenuEntry);
}

void EnsureWindowInventoryOpen()
{
	if (null != WindowInventory)
		return;

	Host.Log("open Inventory.");
	Sanderling.MouseClickLeft(Measurement?.Neocom?.InventoryButton);
}

void EnsureWindowInventoryOreContainerIsOpen()
{
	EnsureWindowInventoryOpen();

	var inventoryActiveShip = WindowInventory?.ActiveShipEntry;

	if(InventoryActiveShipOreContainer == null && !(IsExpanded(inventoryActiveShip) ?? false))
	{
		Host.Log("It looks like the active ships entry in the inventory is not expanded. I try to expand it to see if the ship has an ore hold.");
		Sanderling.MouseClickLeft(inventoryActiveShip?.ExpandToggleButton);
	}

	if(!(InventoryActiveShipOreContainer?.IsSelected ?? false))
		Sanderling.MouseClickLeft(InventoryActiveShipOreContainer);
}

//	sample label text: Intensive Reprocessing Array <color=#66FFFFFF>1,123 m</color>
string InventoryContainerLabelRegexPatternFromContainerName(string containerName) =>
	@"^\s*" + Regex.Escape(containerName) + @"\s*($|\<)";

void InInventoryUnloadItems() => InInventoryUnloadItemsTo(UnloadDestContainerName);

void InInventoryUnloadItemsTo(string DestinationContainerName)
{
	Host.Log("unload items to '" + DestinationContainerName + "'.");

	EnsureWindowInventoryOreContainerIsOpen();

	for (;;)
	{
		var oreContainerListItem = WindowInventory?.SelectedRightInventory?.ListView?.Entry?.ToArray();

		var oreContainerItem = oreContainerListItem?.FirstOrDefault();

		if(null == oreContainerItem)
			break;    //    0 items in the container which holds the ore.

		if(2 < oreContainerListItem?.Length)
			ClickMenuEntryOnMenuRoot(oreContainerItem, @"select\s*all");

		var DestinationContainerLabelRegexPattern =
			InventoryContainerLabelRegexPatternFromContainerName(DestinationContainerName);

		var DestinationContainer =
			WindowInventory?.LeftTreeListEntry?.SelectMany(entry => new[] { entry }.Concat(entry.EnumerateChildNodeTransitive()))
			?.FirstOrDefault(entry => entry?.Text?.RegexMatchSuccessIgnoreCase(DestinationContainerLabelRegexPattern) ?? false);

		if (null == DestinationContainer)
			Host.Log("error: Inventory entry labeled '" + DestinationContainerName + "' not found");

		Sanderling.MouseDragAndDrop(oreContainerItem, DestinationContainer);
	}
}

bool InitiateWarpToMiningSite()	=>
	InitiateDockToOrWarpToLocationInSolarSystemMenu("asteroid belts", PickNextMiningSiteFromSystemMenu);

bool InitiateDockToOffload()	=>
	InitiateDockToOrWarpToLocationInSolarSystemMenu("stations", PickPlaceToOffloadfFromSystemMenu);

bool InitiateWarpToRetreat()	=>
	InitiateDockToOrWarpToLocationInSolarSystemMenu("stations");

MemoryStruct.IMenuEntry PickNextMiningSiteFromSystemMenu(IReadOnlyList<MemoryStruct.IMenuEntry> availableMenuEntries)
{
	Host.Log("I am seeing " + availableMenuEntries?.Count.ToString() + " mining sites to choose from.");

	var nextSite =
		availableMenuEntries
		?.OrderBy(menuEntry => visitedLocations.ToList().IndexOf(menuEntry?.Text))
		?.FirstOrDefault();

	Host.Log("I pick '" + nextSite?.Text + "' as next mining site, based on the intent to rotate through the mining sites and recorded previous locations.");
	return nextSite;
}

MemoryStruct.IMenuEntry PickPlaceToOffloadfFromSystemMenu(IReadOnlyList<MemoryStruct.IMenuEntry> availableMenuEntries)
{
	Host.Log("I am seeing " + availableMenuEntries?.Count.ToString() + " stations to choose from for offloading ore.");

	var	availableMenuEntriesTexts =
		availableMenuEntries
		?.Select(menuEntry => menuEntry.Text)
		?.ToList();

	foreach(var visitedLocation in visitedLocations.Reverse())
	{
		var visitedLocationMenuEntry =
			availableMenuEntries?.FirstOrDefault(menuEntry =>
				StationFromSystemInfoPanelEqualsStationFromSystemMenu(visitedLocation, menuEntry?.Text));

		if(visitedLocationMenuEntry == null)
			continue;

		Host.Log("I pick '" + visitedLocationMenuEntry?.Text + "' as destination, because this is the last one visited.");
		return visitedLocationMenuEntry;
	}

	var	destination = availableMenuEntries?.FirstOrDefault();

	Host.Log("I pick '" + destination?.Text + "' as destination, because I do not remember having visited any of the available stations.");
	return destination;
}

bool InitiateDockToOrWarpToLocationInSolarSystemMenu(
	string submenuLabel,
	Func<IReadOnlyList<MemoryStruct.IMenuEntry>, MemoryStruct.IMenuEntry> pickPreferredDestination = null)
{
	Host.Log("Attempt to initiate dock to or warp to menu entry in submenu '" + submenuLabel + "'");
	
	var listSurroundingsButton = Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton;
	
	Sanderling.MouseClickRight(listSurroundingsButton);

	var submenuEntry = Measurement?.Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("^" + submenuLabel + "$", RegexOptions.IgnoreCase);

	if(null == submenuEntry)
	{
		Host.Log("Submenu '" + submenuLabel + "' not found in the solar system menu.");
		return true;
	}

	Sanderling.MouseClickLeft(submenuEntry);

	var submenu = Measurement?.Menu?.ElementAtOrDefault(1);

	var destinationMenuEntry = pickPreferredDestination?.Invoke(submenu?.Entry?.ToList()) ?? submenu?.Entry?.FirstOrDefault();

	if(destinationMenuEntry == null)
	{
		Host.Log("Failed to open submenu '" + submenuLabel + "' in the solar system menu.");
		return true;
	}

	Sanderling.MouseClickLeft(destinationMenuEntry);

	var actionsMenu = Measurement?.Menu?.ElementAtOrDefault(2);

	if(destinationMenuEntry == null)
	{
		Host.Log("Failed to open actions menu for '" + destinationMenuEntry.Text + "' in the solar system menu.");
		return true;
	}

	var dockMenuEntry = actionsMenu?.EntryFirstMatchingRegexPattern("dock", RegexOptions.IgnoreCase);
	var warpMenuEntry = actionsMenu?.EntryFirstMatchingRegexPattern(@"warp.*within.*m", RegexOptions.IgnoreCase);
	var approachEntry = actionsMenu?.EntryFirstMatchingRegexPattern(@"approach", RegexOptions.IgnoreCase);

	var maneuverMenuEntry = dockMenuEntry ?? warpMenuEntry;

	if (null != maneuverMenuEntry)
	{
		Host.Log("initiating '" + maneuverMenuEntry.Text + "' on '" + destinationMenuEntry?.Text + "'");
		Sanderling.MouseClickLeft(maneuverMenuEntry);
		return false;
	}

	if (null != approachEntry)
	{
		Host.Log("found menu entry '" + approachEntry.Text + "'. Assuming we are already there.");
		return false;
	}

	Host.Log("no suitable menu entry found on '" + destinationMenuEntry?.Text + "'");
	return true;
}

void Undock()
{
	while(Measurement?.IsDocked ?? true)
	{
		Sanderling.MouseClickLeft(Measurement?.WindowStation?.FirstOrDefault()?.UndockButton);
		Host.Log("waiting for undocking to complete.");
		Host.Delay(8000);
	}

	Host.Delay(4444);
	Sanderling.InvalidateMeasurement();
}

void ModuleMeasureAllTooltip()
{
	Host.Log("measure tooltips of all modules.");

	for (;;)
	{
		var NextModule = Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.FirstOrDefault(m => null == m?.TooltipLast);

		if(null == NextModule)
			break;

		Host.Log("measure module.");
		//	take multiple measurements of module tooltip to reduce risk to keep bad read tooltip.
		Sanderling.MouseMove(NextModule);
		Sanderling.WaitForMeasurement();
		Sanderling.MouseMove(NextModule);
	}
}

void ActivateHardenerExecute()
{
	var	SubsetModuleHardener =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
		?.Where(module => module?.TooltipLast?.Value?.IsHardener ?? false);

	var	SubsetModuleToToggle =
		SubsetModuleHardener
		?.Where(module => !(module?.RampActive ?? false));

	foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
		ModuleToggle(Module);
}

void ModuleToggle(Sanderling.Accumulation.IShipUiModule Module)
{
	var ToggleKey = Module?.TooltipLast?.Value?.ToggleKey;

	Host.Log("toggle module using " + (null == ToggleKey ? "mouse" : Module?.TooltipLast?.Value?.ToggleKeyTextLabel?.Text));

	if(null == ToggleKey)
		Sanderling.MouseClickLeft(Module);
	else
		Sanderling.KeyboardPressCombined(ToggleKey);
}

void EnsureOverviewTypeSelectionLoaded()
{
	if(null == OverviewPresetTabActive || null == WindowOverview || null == OverviewPreset)
		return;

	if(string.Equals(OverviewTypeSelectionName, OverviewPreset, StringComparison.OrdinalIgnoreCase))
		return;

	Host.Log("loading preset '" + OverviewPreset + "' to overview (current selection is '" + OverviewTypeSelectionName + "').");
	Sanderling.MouseClickRight(OverviewPresetTabActive);
	Sanderling.MouseClickLeft(Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("load.*preset", RegexOptions.IgnoreCase));
	var PresetMenuEntry = Menu?.ElementAtOrDefault(1)?.EntryFirstMatchingRegexPattern(@"^\s*" + Regex.Escape(OverviewPreset) + @"\s*$", RegexOptions.IgnoreCase);

	if(null == PresetMenuEntry)
	{
		Host.Log("error: menu entry '" + OverviewPreset + "' not found");
		return;
	}

	Sanderling.MouseClickLeft(PresetMenuEntry);
}

void DelayWhileUpdatingMemory(int delayAmountMilli)
{
	var beginTimeMilli = Host.GetTimeContinuousMilli();

	while(true)
	{
		var remainingTimeMilli = beginTimeMilli + delayAmountMilli - Host.GetTimeContinuousMilli();
		if(remainingTimeMilli < 0)
			break;

		Host.Delay(Math.Min(1000, (int)remainingTimeMilli));
		Sanderling.InvalidateMeasurement();
		MemoryUpdate();
	}
}

void MemoryUpdate()
{
	RetreatUpdate();
	JammedLastTimeUpdate();
	OffloadCountUpdate();
	UpdateLocationRecord();
	TrackTotalMinedOre();
}

void UpdateLocationRecord()
{
	//	I am not interested in locations which are only close during warp.
	if(Measurement?.ShipUi?.Indication?.ManeuverType == ShipManeuverTypeEnum.Warp)
		return;

	// Purpose of recording locations is to prioritize our next destination when warping to mining site or docking to station.
	// For this purpose, I will compare the recorded locations with the menu entries in the system menu.
	// Therefore I want the format of the recorded location to be the same as it appears in the menu entries in the system menu.

	var currentSystemLocationLabelText =
		Measurement?.InfoPanelCurrentSystem?.ExpandedContent?.LabelText
		?.OrderByCenterVerticalDown()?.FirstOrDefault()?.Text;

	if(currentSystemLocationLabelText == null)
		return;

	// 2018-03 observed label text: <url=showinfo:15//40088644 alt='Nearest'>Amsen V - Asteroid Belt 1</url>

	var currentLocationName = RegexExtension.RemoveXmlTag(currentSystemLocationLabelText)?.Trim();

	var	lastRecordedLocation = visitedLocations.LastOrDefault();

	if(lastRecordedLocation == currentLocationName)
		return;

	visitedLocations.Enqueue(currentLocationName);
	Host.Log("Recorded transition from location '" + lastRecordedLocation + "' to location '" + currentLocationName + "'");

	if(100 < visitedLocations.Count)
		visitedLocations.Dequeue();
}

/*
2018-03 Observation: Station name containing reference to moon appears different between system menu and system info panel.
In the info panel 'Moon 17' was used while in the system menu it was 'M17'.
*/
public bool StationFromSystemInfoPanelEqualsStationFromSystemMenu(
	string stationNameInCurrentSystemInfoPanel,
	string stationNameInSystemMenu)
{
	//	Copied from https://github.com/botengine-de/Optimat.EO/blob/6c19e8f36e30d5468e94d627eb16dcb78bb47d12/src/Optimat.EveOnline.Bot/Sonst/AgentUndMission.Aktualisiire.cs#L1888-L1893

	var representationPattern =
		Regex.Replace(
		stationNameInCurrentSystemInfoPanel,
		"Moon\\s*",
		"M([\\w]*\\s*)",
		RegexOptions.IgnoreCase);

	return
		stationNameInSystemMenu.RegexMatchSuccessIgnoreCase(representationPattern);
}

void JammedLastTimeUpdate()
{
	if(Jammed)
		JammedLastTime	= Host.GetTimeContinuousMilli();
}

bool MeasurementEmergencyWarpOutEnter =>
	!(Measurement?.IsDocked ?? false) && !(EmergencyWarpOutHitpointPercent < ShieldHpPercent);

void RetreatUpdate()
{
	RetreatReasonTemporary = (RetreatOnNeutralOrHostileInLocal && hostileOrNeutralsInLocal)	? "hostile or neutral in local" : null;

	if (!MeasurementEmergencyWarpOutEnter)
		return;

	//	measure multiple times to avoid being scared off by noise from a single measurement. 
	Sanderling.InvalidateMeasurement();

	if (!MeasurementEmergencyWarpOutEnter)
		return;

	RetreatReasonPermanent = "shield hp";
}

void OffloadCountUpdate()
{
	var	OreContainerFillPercentSynced	= OreContainerFillPercent;

	if(!OreContainerFillPercentSynced.HasValue)
		return;

	if(0 == OreContainerFillPercentSynced && OreContainerFillPercentSynced < LastCheckOreContainerFillPercent)
		++OffloadCount;

	LastCheckOreContainerFillPercent = OreContainerFillPercentSynced;
}

bool IsNeutralOrEnemy(IChatParticipantEntry participantEntry) =>
   !(participantEntry?.FlagIcon?.Any(flagIcon =>
	 new[] { "good standing", "excellent standing", "Pilot is in your (fleet|corporation)", }
	 .Any(goodStandingText =>
		flagIcon?.HintText?.RegexMatchSuccessIgnoreCase(goodStandingText) ?? false)) ?? false);

bool? IsExpanded(IInventoryTreeViewEntryShip shipEntryInInventory) =>
	shipEntryInInventory == null ? null :
	(bool?)((shipEntryInInventory.IsExpanded ?? false) || 0 < (shipEntryInInventory.Child?.Count() ?? 0));

void TrackTotalMinedOre()
{
	var currentTimeMilli = Sanderling.MemoryMeasurement?.End;

	// Host.Log("TrackTotalMinedOre called at " + currentTimeMilli);

	if(!currentTimeMilli.HasValue)
		return;

	if(Sanderling.MemoryMeasurementParsed?.Value?.IsDocked ?? false)
		return;

	if(SetModuleMiner?.Any(module => module?.RampActive ?? false) ?? false)
		miningModuleLastActiveTime = currentTimeMilli.Value;

	var containerCurrentUsedVolumeMilli = OreContainerCapacityMilli?.Used;

	if(!containerCurrentUsedVolumeMilli.HasValue)
		return;

	var lastRecordingAgeMilli = currentTimeMilli - efficiencyRawMeasurements.LastOrDefault().timeMilli;

	if(lastRecordingAgeMilli < 500)
		return;

	efficiencyRawMeasurements.Enqueue(
		new StatisticsMeasurementSnapshot{ timeMilli = currentTimeMilli.Value, volumeMilli = containerCurrentUsedVolumeMilli.Value });

	while(2 < efficiencyRawMeasurements.Count)
		efficiencyRawMeasurements.Dequeue();

	if(efficiencyRawMeasurements.Select(measurement => measurement.volumeMilli).Distinct().Count() == 1)
		efficiencyDebouncedMeasurements.Enqueue(efficiencyRawMeasurements.Last());

	while(2 < efficiencyDebouncedMeasurements.Count)
		efficiencyDebouncedMeasurements.Dequeue();

	if(1 < efficiencyDebouncedMeasurements.Count)
	{
		var lastMeasurement = efficiencyDebouncedMeasurements.Last(); 
		var secondLastMeasurement = efficiencyDebouncedMeasurements.Reverse().ElementAt(1);

		var usedVolumeIncreaseMilli = lastMeasurement.volumeMilli - secondLastMeasurement.volumeMilli;
		var secondLastMeasurementAgeMilli = currentTimeMilli - secondLastMeasurement.timeMilli;

		if(0 < usedVolumeIncreaseMilli)
		{
			Host.Log("Inventory used volume increased by " + (usedVolumeIncreaseMilli / 1000) + " m³");

			var miningModuleActiveLastAge = (currentTimeMilli.Value - miningModuleLastActiveTime) / 1000;

			if(10 * 1000 < miningModuleActiveLastAge)
				Host.Log("Measuring Statistics: Ignored volume increase because miningModuleActiveLastAge was " + miningModuleActiveLastAge + " s.");
			else
			{
				if(10 * 1000 < secondLastMeasurementAgeMilli)
					Host.Log("Measuring Statistics: Ignored volume increase because timespan was " + secondLastMeasurementAgeMilli + " ms.");
				else
				{
					totalMinedOreVolumeMilli += usedVolumeIncreaseMilli;
					Host.Log("Total_mined_ore_in_cubic_meters: " + (totalMinedOreVolumeMilli / 1000));
				}
			}
		}
	}
}

