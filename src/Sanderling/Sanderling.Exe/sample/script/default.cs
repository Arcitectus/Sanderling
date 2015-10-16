//	Welcome to Sanderling, the botting framework that makes eve online easily scriptable.
//	Got questions or a feature request? Leave a message at http://forum.botengine.de/cat/eve-online/

//	The script below is a Warp to 0 Auto-Pilot, making your travels faster and thus safer.

//	this pattern matches strings containing "dock" or "jump".
const string MenuEntryRegexPattern = "dock|jump";

while(true)
{
	var Measurement = HostSanderling?.MemoryMeasurement?.Wert;

	//	from the set of route element markers in the Info Panel pick the one that represents the next Waypoint/System.
	//	We assume this is the one which is nearest to the topleft corner of the Screen which is at (0,0)
	var RouteElementMarkerNext =
		Measurement?.InfoPanelRoute?.RouteElementMarker
		?.OrderByCenterDistanceToPoint(new Vektor2DInt(0, 0))?.FirstOrDefault();

	if(null == RouteElementMarkerNext)
	{
		Host.Log("no route found in info panel.");
		goto loop;
	}
	
	//	righclick the marker to open the contextmenu.
	HostSanderling.MouseClickRight(RouteElementMarkerNext);

	//	retrieve a new measurement because we issued an  
	Measurement = HostSanderling?.MemoryMeasurement?.Wert;

	//	from the first menu, pick the first matching entry.
	var MenuEntry =
		Measurement?.Menu?.FirstOrDefault()
		?.Entry?.FirstOrDefault(candidate => candidate.Text.RegexMatchSuccessIgnoreCase(MenuEntryRegexPattern));
	
	if(null == MenuEntry)
	{
		Host.Log("no suitable menu entry found.");
		goto loop;
	}

	//	click on the "dock"/"jump" menu entry to initiate warp to at 0km.
	HostSanderling.MouseClickLeft(MenuEntry);

loop:
	//	wait for four seconds before repeating.
	Host.Delay(4000);
}
