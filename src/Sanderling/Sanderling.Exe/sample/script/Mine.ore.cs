//	This script mines ore from asteroids.
//	before running this script, make sure to prepare as follows:
//	+enter bookmark for mining site and bookmark for station in the configuration section below.
//	+in the Overview create a preset which includes asteroids and rats and enter the name of that preset in the configuration section below at 'OverviewPreset'. The bot will make sure this preset is loaded when it needs to use the overview.
//	+set Overview to sort by distance with the nearest entry at the top.
//	+in the Inventory select the 'List' view.
//	+set the UI language to english.
//	+use a ship with an ore hold.
//	+put some light drones into your ships' drone bay. The bot will make use of them to attack rats when HP are too low (configurable) or it gets jammed.
//	+enable the info panel 'System info'. The bot will use the button in there to access bookmarks and asteroid belts.
//	+arrange windows to not occlude modules or info panels.
//	+in the ship UI, disable "Display Passive Modules" and disable "Display Empty Slots" and enable "Display Module Tooltips". The bot uses the module tooltips to automatically identify the properties of the modules.
//
//	for optional features (such as warp to safe on hostile in local) see the configuration section below.

using BotSharp.ToScript.Extension;
using Parse = Sanderling.Parse;

//	begin of configuration section ->

//	The bot uses the bookmarks from the menu which is opened from the button in the 'System info' panel.

//	Bookmarks of places to mine. Add additional bookmarks separated by comma.
string[] SetMiningSiteBookmark = new[] {
	"Asteroid Belts",
	};

//	Bookmark of location where ore should be unloaded.
string UnloadBookmark = "home";

//	Name of the container to unload to as shown in inventory.
string UnloadDestContainerName = "Item Hangar";

//	when this is set to true, the bot will try to unload when undocked.
bool UnloadInSpace = false;

//	Bookmark of place to retreat to to prevent ship loss.
string RetreatBookmark = UnloadBookmark;
string moneyn;
//	The bot loads this preset to the active tab. 
string OverviewPreset = "b"; //default start overview
string OverviewPresetDef = "def"; //def overview
string OverviewPresetExitDef= "b"; //return to default overview
var ActivateHardener = true; // activate shield hardener.

//	bot will start fighting (and stop mining) when hitpoints are lower. 
var DefenseEnterHitpointThresholdPercent = 95;
var DefenseExitHitpointThresholdPercent = 100;

var EmergencyWarpOutHitpointPercent = 40;
var EmergencyWarpOutHitpointPercentArmor = 80;

var i = 0;

var FightAllRats = false;	//	when this is set to true, the bot will attack rats independent of shield hp.

var EnterOffloadOreHoldFillPercent = 95;	//	percentage of ore hold fill level at which to enter the offload process.

var RetreatOnNeutralOrHostileInLocal = false;   // warp to RetreatBookmark when a neutral or hostile is visible in local.

//	<- end of configuration section


Func<object> BotStopActivity = () => null;

Func<object> NextActivity = MainStep;

for(;;)
{

	MemoryUpdate();
	if (money !=null)
	{
	 moneyn = Regex.Replace(money, "[^0-9\\s]+", "");
	
	}


  
	Host.Log(
		"ore hold fill: " + OreHoldFillPercent + "%" +
		", mining range: " + MiningRange +
		", mining modules (inactive): " + SetModuleMiner?.Length + "(" + SetModuleMinerInactive?.Length + ")" +
		", shield.hp: " + ShieldHpPercent + "%" +
		", Armor.hp: " + ShieldApPercent + "%" +
		", retreat: " + RetreatReason + 
		", JLA: " + JammedLastAge +
		", overview.rats: " + ListRatOverviewEntry?.Length +
		", overview.roids: " + ListAsteroidOverviewEntry?.Length +
		", offload count: " + OffloadCount +
		", Money : " + moneyn+		
		", nextAct: " + NextActivity?.Method?.Name);

	CloseModalUIElement();

	if(0 < RetreatReason?.Length && !(Measurement?.IsDocked ?? false))
	{
	
		InitiateDockToOrWarpToBookmark(RetreatBookmark);
		continue;
	}
	loops1:
	MemoryUpdate();
	var ManeuverType = Measurement?.ShipUi?.Indication?.ManeuverType;
	if(ShipManeuverTypeEnum.Warp == ManeuverType	||
		ShipManeuverTypeEnum.Jump == ManeuverType)
		{
		Host.Log("Wait Finish");
		Host.Delay(3000);
		
		goto loops1;
		}
		

	

	//	from the set of route element markers in the Info Panel pick the one that represents the next Waypoint/System.
	//	We assume this is the one which is nearest to the topleft corner of the Screen which is at (0,0)
	

var RouteElementMarkerNext =
		Measurement?.InfoPanelRoute?.RouteElementMarker
		?.OrderByCenterDistanceToPoint(new Vektor2DInt(0, 0))?.FirstOrDefault();
	if(null != RouteElementMarkerNext )
	{
		
		Undock();
	
		
	
	
	//	rightclick the marker to open the contextmenu.
	Sanderling.MouseClickRight(RouteElementMarkerNext);

	//	retrieve a new measurement.
	
	//	from the first menu, pick the first entry that contains "dock" or "jump".
	var MenuEntry =
		Measurement?.Menu?.FirstOrDefault()
		?.Entry?.FirstOrDefault(candidate => candidate.Text.RegexMatchSuccessIgnoreCase("dock|jump"));
	if (ReadyForManeuver)
	{
	if(null == MenuEntry)
	{
		
		Host.Log("no suitable menu entry found.");
		
		Host.Delay(3000);
		goto loops1;
		
	}
	}
     
   
	Host.Log("menu entry found. clicking to initiate warp.");
	Sanderling.MouseClickLeft(MenuEntry);


	//	wait for four seconds before repeating.
	Host.Delay(1000);
	
	//	make sure new measurement will be taken.
	Sanderling.InvalidateMeasurement(); 
	goto loops1;
	
	}
	
	

	NextActivity = NextActivity?.Invoke() as Func<object>;

	if(BotStopActivity == NextActivity)
		break;
		if (0 < RetreatReasonPermanent?.Length)
			NextActivity = Heal;
			
		
	
	if(null == NextActivity)
		NextActivity = MainStep;
	
	Host.Delay(1111);
}

//	seconds since ship was jammed.
long? JammedLastAge => Jammed ? 0 : (Host.GetTimeContinuousMilli() - JammedLastTime) / 1000;

int?	ShieldHpPercent => ShipUi?.HitpointsAndEnergy?.Shield / 10;

 int?ShieldApPercent =>ShipUi?.HitpointsAndEnergy?.Armor /10;
bool	DefenseExit =>
	(Measurement?.IsDocked ?? false) ||
	!(0 < ListRatOverviewEntry?.Length)	||
	(DefenseExitHitpointThresholdPercent < ShieldHpPercent && !(JammedLastAge < 40) &&
	!(FightAllRats && 0 < ListRatOverviewEntry?.Length && ListRatOverviewEntry?.Length == 0 )) ||
	(DefenseExitHitpointThresholdPercent < ShieldHpPercent && !(JammedLastAge < 40) &&
	!(FightAllRats && 0 < ListRatOverviewEntry?.Length && ListRatOverviewEntry?.Length > 0 ));

bool	DefenseEnter =>
	!DefenseExit	||
	!(DefenseEnterHitpointThresholdPercent < ShieldHpPercent) && ListRatOverviewEntry?.Length != 0 && ListRatOverviewEntry?.Length != null || JammedLastAge < 10 ;

bool	OreHoldFilledForOffload => Math.Max(0, Math.Min(100, EnterOffloadOreHoldFillPercent)) <= OreHoldFillPercent;

Int64?	JammedLastTime = null;
string RetreatReasonTemporary = null;
string RetreatReasonPermanent = null;
string RetreatReason => RetreatReasonPermanent ?? RetreatReasonTemporary;
int? LastCheckOreHoldFillPercent = null;

int OffloadCount = 0;

Func<object>	Heal()
{


var inventoryActiveShip = WindowInventory?.ActiveShipEntry;


ClickMenuEntryOnMenuRoot(inventoryActiveShip,@"get\s*repair\s*quote");
Host.Delay(2000);
		
		Sanderling.KeyboardPress(VirtualKeyCode.TAB);
		Host.Delay(500);
		Sanderling.KeyboardPress(VirtualKeyCode.TAB);
		Host.Delay(500);
		Sanderling.KeyboardPress(VirtualKeyCode.RETURN);
		Host.Delay(1000);
		Sanderling.KeyboardPress(VirtualKeyCode.RETURN);
		Host.Delay(1000);
		Sanderling.KeyboardPress(VirtualKeyCode.TAB);
		Host.Delay(500);
		Sanderling.KeyboardPress(VirtualKeyCode.RETURN);
		Host.Log("repair finish");
		NextActivity = MainStep;
		RetreatReasonPermanent=null;
return MainStep;
}
Func<object>	MainStep()
{
	if(Measurement?.IsDocked ?? false)
	{
		InInventoryUnloadItems();

		if (0 < RetreatReasonPermanent?.Length)
			return Heal;

		if (0 < RetreatReason?.Length)
			return MainStep;

		Undock();
	}
	
	if(DefenseEnter)
	{
		Host.Log("enter defense.");
		return DefenseStep;
	}

	EnsureOverviewTypeSelectionLoaded();

	EnsureWindowInventoryOpenOreHold();

	if(ReadyForManeuver)
	{
		DroneEnsureInBay();

		if(OreHoldFilledForOffload)
		{
			if(ReadyForManeuver)
				InitiateDockToOrWarpToBookmark(UnloadBookmark);

			if (UnloadInSpace)
			{
				Host.Delay(4444);
				InInventoryUnloadItems();
			}

			return MainStep;
		}
		
		if(!(0 < ListAsteroidOverviewEntry?.Length))
			InitiateWarpToRandomMiningSite();
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

void DroneLaunch()
{
	Host.Log("launch drones.");
	Sanderling.MouseClickRight(DronesInBayListEntry);
	Sanderling.MouseClickLeft(Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("launch", RegexOptions.IgnoreCase));
}

void DroneEnsureInBay()
{
 i =0;
	if(0 == DronesInSpaceCount && null != DronesInSpaceCount)
		return;

	DroneReturnToBay();
	

}

void DroneReturnToBay()
{
	
	Host.Log("return drones to bay.");
	Sanderling.MouseClickRight(DronesInSpaceListEntry);
	Sanderling.MouseClickLeft(Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("return.*bay", RegexOptions.IgnoreCase));
	
	Host.Delay(5000);
		if(0 == DronesInSpaceCount && null != DronesInSpaceCount)
		return;
		
		 

}

Func<object>	DefenseStep()
{
MemoryUpdate();
	OverviewPreset =  OverviewPresetDef;
	if(DefenseExit)
	{
	 i = 0;
	 OverviewPreset =  OverviewPresetExitDef;
	var	SubsetModuleHardenero =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
		?.Where(module => module?.TooltipLast?.Value?.IsShieldBooster ?? true);



	foreach (var Module in SubsetModuleHardenero.EmptyIfNull())
		ModuleToggle(Module);
		
		var	SubsetModuleHardeneroa2 =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
		?.Where(module => module?.TooltipLast?.Value?.IsWeapon ?? true);

if(SetModulewaepon?.Length != 0)
		{
		foreach (var Module in SubsetModuleHardeneroa2.EmptyIfNull())
		ModuleToggle(Module);
		}
		
		DroneEnsureInBay();
		Host.Log("exit defense.");
		return null;
	}
	
	
	var	SubsetModuleHardener =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
		?.Where(module => module?.TooltipLast?.Value?.IsShieldBooster ?? true);

	var	SubsetModuleToToggle =
		SubsetModuleHardener
		?.Where(module => !(module?.RampActive ?? true));

	foreach (var Module in SubsetModuleToToggle.EmptyIfNull())
		ModuleToggle(Module);

	if (0 == DronesInSpaceCount && 0 != DronesInBayCount)
		DroneLaunch();

	EnsureOverviewTypeSelectionLoaded();

	var SetRatName =
		ListRatOverviewEntry?.Select(entry => Regex.Split(entry?.Name ?? "", @"\s+")?.FirstOrDefault())
		?.Distinct()
		?.ToArray();
	
	var SetRatTarget = Measurement?.Target?.Where(target =>
		SetRatName?.Any(ratName => target?.TextRow?.Any(row => row.RegexMatchSuccessIgnoreCase(ratName)) ?? false) ?? false);
	
	var RatTargetNext = SetRatTarget?.OrderBy(target => target?.DistanceMax ?? int.MaxValue)?.FirstOrDefault();
	
	if(null == RatTargetNext )
	{
		Host.Log("no rat targeted.");
		Sanderling.MouseClickRight(ListRatOverviewEntry?.FirstOrDefault());
		Sanderling.MouseClickLeft(MenuEntryLockTarget);
	
			var	SubsetModuleHardeneroa =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
		?.Where(module => module?.TooltipLast?.Value?.IsWeapon ?? true);
		if(SetModulewaepon?.Length != 0)
		{
			Sanderling.MouseClickRight(ListRatOverviewEntry?.FirstOrDefault());
		ClickMenuEntryOnMenuRoot(RatTargetNext, "approach");
		}
		
		Host.Delay(1000);
	}
if(null != RatTargetNext  )
	{
		Host.Log("rat targeted. sending drones.");
		Sanderling.MouseClickLeft(RatTargetNext);
		Sanderling.MouseClickRight(DronesInSpaceListEntry);
		Sanderling.MouseClickLeft(Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("engage", RegexOptions.IgnoreCase));
		var	SubsetModuleHardeneroa =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
		?.Where(module => module?.TooltipLast?.Value?.IsWeapon ?? true);

	var	SubsetModuleToToggleoa =
		SubsetModuleHardeneroa
		?.Where(module => !(module?.RampActive ?? false));
if(SetModulewaepon?.Length != 0)
		{
		foreach (var Module in SubsetModuleToToggleoa.EmptyIfNull())
		ModuleToggle(Module);
		}
		i = 0;
	
	}
	
	return DefenseStep;
}

Func<object> InBeltMineStep()
{
if (Measurement?.IsDocked ?? true)
{
	return MainStep;
}
var loop = 0;
 loopdef:
	if (DefenseEnter)
	{
	
				var	SubsetModuleHardeneroa =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
		?.Where(module => module?.TooltipLast?.Value?.IsWeapon ?? true);
		if(SetModulewaepon?.Length != 0)
		{
			
	var asteroidOverviewEntryNextNotTargeteda = ListAsteroidOverviewEntry?.FirstOrDefault(entry => !((entry?.MeTargeted ?? false) || (entry?.MeTargeting ?? false)));

		var asteroidOverviewEntryNexatq = ListAsteroidOverviewEntry?.FirstOrDefault();
		Host.Delay(1000);
		ClickMenuEntryOnMenuRoot(asteroidOverviewEntryNexatq, "^unlock");
		Host.Delay(1000);
		ClickMenuEntryOnMenuRoot(asteroidOverviewEntryNextNotTargeteda, "^unlock");
		
		}
			
	
		Host.Log("enter defense.");
		 i = 0;
		return DefenseStep;
	}

	EnsureWindowInventoryOpenOreHold();

	EnsureOverviewTypeSelectionLoaded();

	if(OreHoldFilledForOffload)
		return null;

	var moduleMinerInactive = SetModuleMinerInactive?.FirstOrDefault();

	if (null == moduleMinerInactive)
	{
	
	var MenuEntrya =
		Measurement?.ShipUi?.Indication?.LabelText?.FirstOrDefault(candidate => candidate.Text.RegexMatchSuccessIgnoreCase("APPROACHING|WARP"));
		var stop =
		Measurement?.ShipUi?.ButtonSpeed0;
			
	if(null != MenuEntrya)
	{
	
		var	SubsetModuleHardenerr=
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
		?.Where(module => module?.TooltipLast?.Value?.IsAfterburner ?? true);



	foreach (var Module in SubsetModuleHardenerr.EmptyIfNull())
		ModuleToggle(Module);
	Host.Delay(1000);
	Sanderling.MouseClickLeft(stop);
	Host.Delay(7777);
		return InBeltMineStep;
	}
		Host.Delay(7777);
		return InBeltMineStep;
	}
	loopm1:
	var ManeuverType = Measurement?.ShipUi?.Indication?.ManeuverType;
if(ShipManeuverTypeEnum.Warp == ManeuverType	||
		ShipManeuverTypeEnum.Jump == ManeuverType)
		{
		Host.Log("Wait Finish");
		Host.Delay(2000);
		goto loopm1;
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
	
	if(ShipManeuverTypeEnum.Warp == ManeuverType	||
		ShipManeuverTypeEnum.Jump == ManeuverType)
		{
		Host.Log("Wait Finish");
		Host.Delay(2000);
		goto loopm1;
		}
		return null;
	}

	if(null == asteroidOverviewEntryNextNotTargeted)
	{
		Host.Log("all asteroids targeted");
		loop= 0;
		return null;
	}
	

	if (!(asteroidOverviewEntryNextNotTargeted.DistanceMax < MiningRange))
	{
		if(!(1111 < asteroidOverviewEntryNext?.DistanceMin))
		{
		if(loop ==0){
 		loop = loop +1;
		Host.Log("out of range, approaching");
		ClickMenuEntryOnMenuRoot(asteroidOverviewEntryNextNotTargeted, "approach");
		Host.Delay(2000);
		var	a =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
		?.Where(module => module?.TooltipLast?.Value?.IsAfterburner ?? true);

	var	z =
		a
		?.Where(module => !(module?.RampActive ?? true));

	foreach (var Module in z.EmptyIfNull())
		ModuleToggle(Module);
		}
		var MenuEntryz =
		Measurement?.ShipUi?.Indication?.LabelText?.FirstOrDefault(candidate => candidate.Text.RegexMatchSuccessIgnoreCase("APPROACHING|WARP"));
		if(0 < RetreatReason?.Length && !(Measurement?.IsDocked ?? false))
		{
		InitiateDockToOrWarpToBookmark(RetreatBookmark);
		MenuEntryz = null;
		}
	if(null != MenuEntryz)
	{
	
	if (DefenseEnter)
	{
	goto loopdef;
	}
		Host.Log("Wait For Finish.");
		Host.Delay(2000);
		MemoryUpdate();
		goto loopm1;
	}
			Host.Log("distance between asteroids too large");
			return null;
		}
 		if(loop ==0){
 		loop = loop +1;
		Host.Log("out of range, approaching");
		ClickMenuEntryOnMenuRoot(asteroidOverviewEntryNext, "approach");
		Host.Delay(2000);
		var	a =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
		?.Where(module => module?.TooltipLast?.Value?.IsAfterburner ?? true);

	var	z =
		a
		?.Where(module => !(module?.RampActive ?? true));

	foreach (var Module in z.EmptyIfNull())
		ModuleToggle(Module);
		}
		var MenuEntry =
		Measurement?.ShipUi?.Indication?.LabelText?.FirstOrDefault(candidate => candidate.Text.RegexMatchSuccessIgnoreCase("APPROACHING|WARP"));
		if(0 < RetreatReason?.Length && !(Measurement?.IsDocked ?? false))
		{
		InitiateDockToOrWarpToBookmark(RetreatBookmark);
		MenuEntry = null;
		}
	if(null != MenuEntry)
	{
	
	if (DefenseEnter)
	{
	goto loopdef;
	}
		Host.Log("Wait For Finish.");
		Host.Delay(2000);
		MemoryUpdate();
		goto loopm1;
	}
	}
	else
	{
		var MenuEntry =
		Measurement?.ShipUi?.Indication?.LabelText?.FirstOrDefault(candidate => candidate.Text.RegexMatchSuccessIgnoreCase("APPROACHING|WARP"));
	
	var stop =
		Measurement?.ShipUi?.ButtonSpeed0;
	if(null != MenuEntry)
	{
		Host.Log("ActionFinish.");
		var	SubsetModuleHardenere =
		Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule
		?.Where(module => module?.TooltipLast?.Value?.IsAfterburner ?? true);



	foreach (var Module in SubsetModuleHardenere.EmptyIfNull())
		ModuleToggle(Module);
		Host.Delay(2000);
			Sanderling.MouseClickLeft(stop);
	}
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
	Sanderling.Interface.MemoryStruct.IMenuEntry MenuEntryorbitTarget =>
	Menu?.FirstOrDefault()?.Entry?.FirstOrDefault(entry => entry.Text.RegexMatchSuccessIgnoreCase("approach"));

IWindow WindowOther =>
Measurement?.WindowOther?.FirstOrDefault();
Sanderling.Parse.IWindowOverview	WindowOverview	=>
	Measurement?.WindowOverview?.FirstOrDefault();
	


Sanderling.Parse.IWindowInventory	WindowInventory	=>
	Measurement?.WindowInventory?.FirstOrDefault();

IWindowDroneView	WindowDrones	=>
	Measurement?.WindowDroneView?.FirstOrDefault();
IWindowStation	WindowStation	=>
	Measurement?.WindowStation?.FirstOrDefault();

ITreeViewEntry InventoryActiveShipOreHold =>
	WindowInventory?.ActiveShipEntry?.TreeEntryFromCargoSpaceType(ShipCargoSpaceTypeEnum.OreHold);
	
ITreeViewEntry InventoryActiveShipgeneralHold =>
	WindowInventory?.ActiveShipEntry?.TreeEntryFromCargoSpaceType(ShipCargoSpaceTypeEnum.General);
	

IInventoryCapacityGauge OreHoldCapacityMilli =>
	(InventoryActiveShipOreHold?.IsSelected ?? false) ? WindowInventory?.SelectedRightInventoryCapacityMilli : null;

int? OreHoldFillPercent => (int?)((OreHoldCapacityMilli?.Used * 100) / OreHoldCapacityMilli?.Max);

Tab OverviewPresetTabActive =>
	WindowOverview?.PresetTab
	?.OrderByDescending(tab => tab?.LabelColorOpacityMilli ?? 0)
	?.FirstOrDefault();

string OverviewTypeSelectionName =>
	WindowOverview?.Caption?.RegexMatchIfSuccess(@"\(([^\)]*)\)")?.Groups?[1]?.Value;
	
string money =>
	WindowOther?.LabelText?.FirstOrDefault(entry => entry.Text.RegexMatchSuccessIgnoreCase("\\d",RegexOptions.IgnoreCase))?.Text;

  




Parse.IOverviewEntry[] ListRatOverviewEntry => WindowOverview?.ListView?.Entry?.Where(entry =>
		(entry?.MainIconIsRed ?? false)	&& (entry?.IsAttackingMe ?? false))
		?.OrderBy(entry => entry?.DistanceMax ?? int.MaxValue)
		?.ToArray();

Parse.IOverviewEntry[] ListAsteroidOverviewEntry =>
	WindowOverview?.ListView?.Entry
	?.Where(entry => null != OreTypeFromAsteroidName(entry?.Name))
	?.OrderBy(entry => entry.DistanceMax ?? int.MaxValue)
	?.ToArray();
	

DroneViewEntryGroup DronesInBayListEntry =>
	WindowDrones?.ListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(Entry => null != Entry?.Caption?.Text?.RegexMatchIfSuccess(@"Drones in bay", RegexOptions.IgnoreCase));

DroneViewEntryGroup DronesInSpaceListEntry =>
	WindowDrones?.ListView?.Entry?.OfType<DroneViewEntryGroup>()?.FirstOrDefault(Entry => null != Entry?.Caption?.Text?.RegexMatchIfSuccess(@"Drones in Local Space", RegexOptions.IgnoreCase));



int?	DronesInSpaceCount => DronesInSpaceListEntry?.Caption?.Text?.AsDroneLabel()?.Status?.TryParseInt();

int?	DronesInBayCount => DronesInBayListEntry?.Caption?.Text?.AsDroneLabel()?.Status?.TryParseInt();



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
Sanderling.Accumulation.IShipUiModule[] SetModulebooster =>
	Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Where(module => module?.TooltipLast?.Value?.IsAfterburner ?? false)?.ToArray();
	Sanderling.Accumulation.IShipUiModule[] SetModulewaepon =>
	Sanderling.MemoryMeasurementAccu?.Value?.ShipUiModule?.Where(module => module?.TooltipLast?.Value?.IsWeapon ?? false)?.ToArray();
	
Sanderling.Accumulation.IShipUiModule[] SetModuleBoosteractif =>
	SetModuleMiner?.Where(module => (module?.RampActive ?? false))?.ToArray();

Sanderling.Accumulation.IShipUiModule[] SetModuleMinerInactive	 =>
	SetModuleMiner?.Where(module => !(module?.RampActive ?? false))?.ToArray();

int?	MiningRange => SetModuleMiner?.Select(module =>
	module?.TooltipLast?.Value?.RangeOptimal ?? module?.TooltipLast?.Value?.RangeMax ?? module?.TooltipLast?.Value?.RangeWithin ?? 0)?.DefaultIfEmpty(0)?.Min();;



WindowChatChannel chatLocal =>
	 Sanderling.MemoryMeasurementParsed?.Value?.WindowChatChannel
	 ?.FirstOrDefault(windowChat => windowChat?.Caption?.RegexMatchSuccessIgnoreCase("local") ?? false);
	 

//    assuming that own character is always visible in local
bool hostileOrNeutralsInLocal => 1 != chatLocal?.ParticipantView?.Entry?.Count(IsNeutralOrEnemy);
var ok = false;
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

void EnsureWindowInventoryOpenOreHold()
{
	EnsureWindowInventoryOpen();

	var inventoryActiveShip = WindowInventory?.ActiveShipEntry;

	if(InventoryActiveShipOreHold == null && !(inventoryActiveShip?.IsExpanded ?? false))
		Sanderling.MouseClickLeft(inventoryActiveShip?.ExpandToggleButton);

	if(!(InventoryActiveShipOreHold?.IsSelected ?? false))
		Sanderling.MouseClickLeft(InventoryActiveShipOreHold);
}

void EnsureWindowInventoryOpengeneraleHold(string s )
{
	EnsureWindowInventoryOpen();
	var DestinationContainerLabelRegexPatterna =
			InventoryContainerLabelRegexPatternFromContainerName(s);

		var DestinationContainera =
			WindowInventory?.LeftTreeListEntry?.SelectMany(entry => new[] { entry }.Concat(entry.EnumerateChildNodeTransitive()))
			?.FirstOrDefault(entry => entry?.Text?.RegexMatchSuccessIgnoreCase(DestinationContainerLabelRegexPatterna) ?? false);

	var inventoryActiveShip = WindowInventory?.ActiveShipEntry;

	if( InventoryActiveShipgeneralHold == null && !(inventoryActiveShip?.IsExpanded ?? false))
		Sanderling.MouseClickLeft(inventoryActiveShip?.ExpandToggleButton);

	if(!(InventoryActiveShipgeneralHold?.IsSelected ?? false))
		Sanderling.MouseClickLeft(DestinationContainera);
		ok = true;
}

//	sample label text: Intensive Reprocessing Array <color=#66FFFFFF>1,123 m</color>
string InventoryContainerLabelRegexPatternFromContainerName(string containerName) =>
	@"^\s*" + Regex.Escape(containerName) + @"\s*($|\<)";

void InInventoryUnloadItems() => InInventoryUnloadItemsTo(UnloadDestContainerName);

void InInventoryUnloadItemsTo(string DestinationContainerName)
{
	Host.Log("unload items to '" + DestinationContainerName + "'.");

	EnsureWindowInventoryOpenOreHold();

	for (;;)
	{
		var oreHoldListItem = WindowInventory?.SelectedRightInventory?.ListView?.Entry?.ToArray();

		var oreHoldItem = oreHoldListItem?.FirstOrDefault();

		if(null == oreHoldItem)
			break;    //    0 items in OreHold

		if(1 < oreHoldListItem?.Length)
			ClickMenuEntryOnMenuRoot(oreHoldItem, @"select\s*all");

		var DestinationContainerLabelRegexPattern =
			InventoryContainerLabelRegexPatternFromContainerName(DestinationContainerName);

		var DestinationContainer =
			WindowInventory?.LeftTreeListEntry?.SelectMany(entry => new[] { entry }.Concat(entry.EnumerateChildNodeTransitive()))
			?.FirstOrDefault(entry => entry?.Text?.RegexMatchSuccessIgnoreCase(DestinationContainerLabelRegexPattern) ?? false);

		if (null == DestinationContainer)
			Host.Log("error: Inventory entry labeled '" + DestinationContainerName + "' not found");

		Sanderling.MouseDragAndDrop(oreHoldItem, DestinationContainer);
		ok = true;
	}
	EnsureWindowInventoryOpengeneraleHold(DestinationContainerName);
	for (;;)
	{
		var oreHoldListItem = WindowInventory?.SelectedRightInventory?.ListView?.Entry?.ToArray();

		var oreHoldItem = oreHoldListItem?.FirstOrDefault();

		if(null == oreHoldItem)
			break;    //    0 items in OreHold

		if(1 < oreHoldListItem?.Length)
			{ClickMenuEntryOnMenuRoot(oreHoldItem, @"select\s*all");

		
			Host.Log("try sell item");

		ClickMenuEntryOnMenuRoot(oreHoldItem, @"sell\s*items|sell\s*this\s*item");
		Host.Delay(5000);
		Sanderling.KeyboardPress(VirtualKeyCode.TAB);
		Host.Delay(500);
		Sanderling.KeyboardPress(VirtualKeyCode.TAB);
		Host.Delay(500);
		Sanderling.KeyboardPress(VirtualKeyCode.TAB);
		Host.Delay(500);
		Sanderling.KeyboardPress(VirtualKeyCode.TAB);
		Host.Delay(500);
		Sanderling.KeyboardPress(VirtualKeyCode.RETURN);
		Host.Delay(2000);
		Sanderling.KeyboardPress(VirtualKeyCode.TAB);
		Host.Delay(500);
		Sanderling.KeyboardPress(VirtualKeyCode.RETURN);
		Host.Delay(1000);
		Sanderling.KeyboardPress(VirtualKeyCode.TAB);
		Host.Delay(500);
		Sanderling.KeyboardPress(VirtualKeyCode.RETURN);
		ok = false;
		}
			if(1 == oreHoldListItem?.Length)
			{

		
			Host.Log("try sell item");

		ClickMenuEntryOnMenuRoot(oreHoldItem, @"sell\s*items|sell\s*this\s*item");
		Host.Delay(5000);
		
		Sanderling.KeyboardPress(VirtualKeyCode.TAB);
		Host.Delay(500);
		Sanderling.KeyboardPress(VirtualKeyCode.TAB);
		Host.Delay(500);
		Sanderling.KeyboardPress(VirtualKeyCode.RETURN);
		Host.Delay(1000);
		Sanderling.KeyboardPress(VirtualKeyCode.TAB);
		Host.Delay(500);
		Sanderling.KeyboardPress(VirtualKeyCode.RETURN);
		Host.Delay(1000);
		Sanderling.KeyboardPress(VirtualKeyCode.TAB);
		Host.Delay(500);
		Sanderling.KeyboardPress(VirtualKeyCode.RETURN);
		ok = false;
		}
		
		
		ok = false;
	}
	}



bool InitiateWarpToRandomMiningSite()	=>
	InitiateDockToOrWarpToBookmark(RandomElement(SetMiningSiteBookmark));

bool InitiateDockToOrWarpToBookmark(string bookmarkOrFolder)
{
 DroneEnsureInBay();

	loopr1:
	
	var ManeuverType = Measurement?.ShipUi?.Indication?.ManeuverType;
	if(ShipManeuverTypeEnum.Warp == ManeuverType	||
		ShipManeuverTypeEnum.Jump == ManeuverType)
		{
		Host.Log("Wait Finish");
		Host.Delay(3000);
		goto loopr1;
		}
	Host.Log("dock to or warp to bookmark or random bookmark in folder: '" + bookmarkOrFolder + "'");
	
	var listSurroundingsButton = Measurement?.InfoPanelCurrentSystem?.ListSurroundingsButton;
	
	Sanderling.MouseClickRight(listSurroundingsButton);
	
	var bookmarkMenuEntry = Measurement?.Menu?.FirstOrDefault()?.EntryFirstMatchingRegexPattern("^" + bookmarkOrFolder + "$", RegexOptions.IgnoreCase);

	if(null == bookmarkMenuEntry)
	{
		Host.Log("menu entry not found for bookmark or folder: '" + bookmarkOrFolder + "'");
		return true;
	}

	var currentLevelMenuEntry = bookmarkMenuEntry;

	for (var menuLevel = 1; ; ++menuLevel)
	{
		Sanderling.MouseClickLeft(currentLevelMenuEntry);

		var menu = Measurement?.Menu?.ElementAtOrDefault(menuLevel);
		var dockMenuEntry = menu?.EntryFirstMatchingRegexPattern("dock", RegexOptions.IgnoreCase);
		var warpMenuEntry = menu?.EntryFirstMatchingRegexPattern(@"warp.*within\s*0", RegexOptions.IgnoreCase);
		var approachEntry = menu?.EntryFirstMatchingRegexPattern(@"approach", RegexOptions.IgnoreCase);

		var maneuverMenuEntry = dockMenuEntry ?? warpMenuEntry;
	

	Host.Delay(100);
	Sanderling.InvalidateMeasurement();


		if (null != maneuverMenuEntry)
		{
			Host.Log("initiating '" + maneuverMenuEntry.Text + "' on entry '" + currentLevelMenuEntry?.Text + "'");
			Sanderling.MouseClickLeft(maneuverMenuEntry);
			return false;
		}

		if (null != approachEntry)
		{
			Host.Log("found menu entry '" + approachEntry.Text + "'. Assuming we are already there.");
			return false;
		}

		var setBookmarkOrFolderMenuEntry =
			menu?.Entry;	//	assume that each entry on the current menu level is a bookmark or a bookmark folder.

		var nextLevelMenuEntry = RandomElement(setBookmarkOrFolderMenuEntry);

		if(null == nextLevelMenuEntry)
		{
			Host.Log("no suitable menu entry found");
			return true;
		}

		currentLevelMenuEntry = nextLevelMenuEntry;
	}
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

void MemoryUpdate()
{
	RetreatUpdate();
	RetreatUpdate1();
	JammedLastTimeUpdate();
	OffloadCountUpdate();
}

void JammedLastTimeUpdate()
{
	if(Jammed)
		JammedLastTime	= Host.GetTimeContinuousMilli();
}

bool MeasurementEmergencyWarpOutEnter =>
	!(Measurement?.IsDocked ?? false) && !(EmergencyWarpOutHitpointPercent < ShieldHpPercent);

bool MeasurementEmergencyWarpOutEntera =>
	!(Measurement?.IsDocked ?? false) && !(EmergencyWarpOutHitpointPercentArmor < ShieldApPercent);
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

void RetreatUpdate1()
{
	RetreatReasonTemporary = (RetreatOnNeutralOrHostileInLocal && hostileOrNeutralsInLocal)	? "hostile or neutral in local" : null;
	
	
	
	if (!MeasurementEmergencyWarpOutEntera)
		return;

	//	measure multiple times to avoid being scared off by noise from a single measurement. 
	Sanderling.InvalidateMeasurement();
	if (!MeasurementEmergencyWarpOutEntera)
		return;
		
		RetreatReasonPermanent = "shield hp";
		
	
}
void OffloadCountUpdate()
{
	var	OreHoldFillPercentSynced	= OreHoldFillPercent;

	if(!OreHoldFillPercentSynced.HasValue)
		return;

	if(0 == OreHoldFillPercentSynced && OreHoldFillPercentSynced < LastCheckOreHoldFillPercent)
		++OffloadCount;

	LastCheckOreHoldFillPercent = OreHoldFillPercentSynced;
}

bool IsNeutralOrEnemy(IChatParticipantEntry participantEntry) =>
   !(participantEntry?.FlagIcon?.Any(flagIcon =>
	 new[] { "good standing", "excellent standing", "Pilot is in your (fleet|corporation)", }
	 .Any(goodStandingText =>
		flagIcon?.HintText?.RegexMatchSuccessIgnoreCase(goodStandingText) ?? false)) ?? false);

