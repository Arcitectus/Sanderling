//	Welcome to Sanderling, the botting framework that makes eve online easily scriptable.
//	Got questions or a feature request? Leave a message at http://forum.botengine.de/cat/eve-online/

//	The script below is a 0km Autopilot.

//	this pattern matches strings containing "dock" or "jump".
const string MenuEntryRegexPattern = "dock|jump";

while(true)
{
	var Measurement = HostSanderling?.MemoryMeasurement?.Wert;

	var Menu = Measurement?.Menu?.FirstOrDefault();

	//	from the first menu, pick the first matching entry.
	var MenuEntry =
		Menu?.Entry?.FirstOrDefault(candidateEntry =>
			candidateEntry.Text.RegexMatchSuccessIgnoreCase(MenuEntryRegexPattern));
	
	if(null != MenuEntry)
	{
		HostSanderling.MouseClickLeft(MenuEntry);
		continue;
	}
	
	Host.Log("no suitable menu entry found.");

	var InfoPanelRoute = Measurement?.InfoPanelRoute;

	//	from the set of Waypoint markers in the Info Panel pick the one that represents the next Waypoint/System.
	//	We assume this is the one which is nearest to the topleft corner of the Screen which is at (0,0)
	var WaypointMarkerNext =
		InfoPanelRoute?.WaypointMarker
		?.OrderByCenterDistanceToPoint(new Vektor2DInt(0, 0))?.FirstOrDefault();

	if(null != WaypointMarkerNext)
	{
		//	righclick the marker to open the contextmenu.
		HostSanderling.MouseClickRight(WaypointMarkerNext);
		continue;
	}

	Host.Log("no route in info panel found.");
	
	//	wait for four seconds before repeating.
	Host.Delay(4000);
}
