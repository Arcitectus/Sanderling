//	This is a warp to 0km auto-pilot, making your travels faster and thus safer by directly warping to gates/stations.
//	It's just slightly more complex version of "simple" autopilot, which adds undocking if docked at the beginning
//	of the route and also completely stops the script upon arrival to final destination.
using Parse = Sanderling.Parse;
Parse.IMemoryMeasurement Measurement => Sanderling?.MemoryMeasurementParsed?.Value;

//	from the set of route element markers in the Info Panel pick the one that represents the next Waypoint/System.
//	We assume this is the one which is nearest to the topleft corner of the Screen which is at (0,0)
IUIElement RouteElementMarkerNext =>
	Measurement?.InfoPanelRoute?.RouteElementMarker
		?.OrderByCenterDistanceToPoint(new Vektor2DInt(0, 0))?.FirstOrDefault();
while(null == RouteElementMarkerNext)
{
	Host.Log("no route found in info panel.");
	Host.Break();
	Sanderling.InvalidateMeasurement();
}

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
}
int seemsArrived = 2;
while(true)
{
	var ManeuverType = Measurement?.ShipUi?.Indication?.ManeuverType;

	if(ShipManeuverTypeEnum.Warp == ManeuverType	||
	   ShipManeuverTypeEnum.Jump == ManeuverType)
		goto loop;	//	do nothing while warping or jumping.

	if(null == RouteElementMarkerNext && --seemsArrived <= 0)
	{
		Host.Log("no route found in info panel. We're here!");
		break;
	}

	//	rightclick the marker to open the contextmenu.
	Sanderling.MouseClickRight(RouteElementMarkerNext);

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
