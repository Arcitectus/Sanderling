//	This is a warp to 0km auto-pilot, making your travels faster and thus safer by directly warping to gates/stations.
//	It's just slightly more complex version of "simple" autopilot, which adds undocking if docked at the beginning
//	of the route and also completely stops the script upon arrival to final station. If you arrive to space the script
//	still will be idle-cycling
var Measurement = Sanderling?.MemoryMeasurementParsed?.Value;
var arriving = false;

while(Measurement?.IsDocked ?? false)
{
	var undockBtnText = Measurement?.WindowStation?.FirstOrDefault()?.LabelText.FirstOrDefault(candidate =>
		candidate.Text.Contains("Undock"))?.Text;
	if (!undockBtnText.Contains("Abort")) {
		Sanderling.MouseClickLeft(Measurement?.WindowStation?.FirstOrDefault()?.UndockButton);
	}
	Host.Log("waiting for undocking to complete.");
	Host.Delay(8000);
	Sanderling.InvalidateMeasurement();
	Host.Delay(1000);
	Measurement = Sanderling?.MemoryMeasurementParsed?.Value;
}
while(true)
{
	Measurement = Sanderling?.MemoryMeasurementParsed?.Value;
	if (Measurement?.IsDocked ?? false)
	{
		if (arriving) {
			Host.Log("Arrived, stopping here");
			break;
		}
	}

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
	if (MenuEntry.Text.RegexMatchSuccessIgnoreCase("dock")) {
		Host.Log("Warping to Dock, getting ready to stop");
		arriving = true;
	}

	loop:
	//	wait for four seconds before repeating.
	Host.Delay(4000);
	//	make sure new measurement will be taken.
	Sanderling.InvalidateMeasurement();
}
