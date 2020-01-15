/*
This is a warp to 0km auto-pilot, making your travels faster and thus safer by directly warping to gates/stations.

The bot follows the route set in the in-game autopilot and uses the context menu to initiate warp and dock commands.

To use the bot, set the in-game autopilot route before starting the bot.
Make sure you are undocked before starting the bot because the bot does not undock.
*/

while(true)
{
	var Measurement = Sanderling?.MemoryMeasurementParsed?.Value;

	var ManeuverType = Measurement?.ShipUi?.Indication?.ManeuverType;

	if(ShipManeuverTypeEnum.Warp == ManeuverType	||
		ShipManeuverTypeEnum.Jump == ManeuverType)
		goto loop;	//	do nothing while warping or jumping.

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
	
	//	rightclick the marker to open the contextmenu.
	Sanderling.MouseClickRight(RouteElementMarkerNext);

	//	retrieve a new measurement.
	Measurement = Sanderling?.MemoryMeasurementParsed?.Value;

	//	from the first menu, pick the first entry that contains "dock" or "jump".
	var MenuEntry =
		Measurement?.Menu?.FirstOrDefault()
		?.Entry?.FirstOrDefault(candidate => candidate.Text.RegexMatchSuccessIgnoreCase("dock|jump"));
	
	if(null == MenuEntry)
	{
		Host.Log("no suitable menu entry found.");
		goto loop;
	}

	Host.Log("menu entry found. clicking to initiate warp.");
	Sanderling.MouseClickLeft(MenuEntry);

loop:
	//	wait for four seconds before repeating.
	Host.Delay(4000);
	//	make sure new measurement will be taken.
	Sanderling.InvalidateMeasurement();
}

