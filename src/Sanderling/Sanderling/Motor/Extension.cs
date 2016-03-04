using BotEngine.Common;
using BotEngine.Motor;
using System;
using System.Collections.Generic;
using System.Linq;
using Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;
using Bib3;

namespace Sanderling.Motor
{
	static public class Extension
	{
		public const int MotionMouseWaypointSafetyMarginMin = 2;

		public const int MotionMouseWaypointSafetyMarginAdditional = 0;

		static public IEnumerable<IUIElement> EnumerateSetElementExcludedFromOcclusion(this IMemoryMeasurement MemoryMeasurement) => new[]
			{
				MemoryMeasurement?.ModuleButtonTooltip,
			}.WhereNotDefault();

		static public IEnumerable<Motion> AsSequenceMotion(
			this MotionParam Motion,
			IMemoryMeasurement MemoryMeasurement)
		{
			if (null == Motion)
			{
				yield break;
			}

			if (Motion?.WindowToForeground ?? false)
				yield return new Motion(null, WindowToForeground: true);

			var SetElementExcludedFromOcclusion = MemoryMeasurement?.EnumerateSetElementExcludedFromOcclusion()?.ToArray();

			var Random = new Random((int)Bib3.Glob.StopwatchZaitMiliSictInt());

			var MouseListWaypoint = Motion?.MouseListWaypoint;

			for (int WaypointIndex = 0; WaypointIndex < (MouseListWaypoint?.Length ?? 0); WaypointIndex++)
			{
				var MouseWaypoint = MouseListWaypoint[WaypointIndex];

				var WaypointUIElement = MouseWaypoint?.UIElement;

				WaypointUIElement = (WaypointUIElement as Accumulation.IRepresentingMemoryObject)?.RepresentedMemoryObject as UIElement ?? WaypointUIElement;

				var WaypointRegionReplacement = MouseWaypoint.RegionReplacement;

				var WaypointUIElementCurrent =
					WaypointUIElement.GetInstanceWithIdFromCLRGraph(MemoryMeasurement, Interface.FromInterfaceResponse.SerialisPolicyCache);

				if (null == WaypointUIElementCurrent)
				{
					throw new ApplicationException("mouse waypoint not anymore contained in UITree");
				}

				var WaypointUIElementRegion = WaypointUIElementCurrent.RegionInteraction?.Region;

				if (!WaypointUIElementRegion.HasValue)
				{
					throw new ArgumentException("Waypoint UIElement has no Region to interact with");
				}

				if (WaypointRegionReplacement.HasValue)
				{
					WaypointUIElementRegion = WaypointRegionReplacement.Value + WaypointUIElementRegion.Value.Center();
				}

				WaypointUIElementCurrent = WaypointUIElementCurrent.WithRegion(WaypointUIElementRegion.Value);

				var WaypointRegionPortionVisible =
					WaypointUIElementCurrent.GetOccludedUIElementRemainingRegion(
						MemoryMeasurement,
						c => SetElementExcludedFromOcclusion?.Contains(c) ?? false)
					//	remaining region is contracted to provide an safety margin.
					?.Select(PortionVisible => PortionVisible.WithSizeExpandedPivotAtCenter(-MotionMouseWaypointSafetyMarginMin * 2))
					?.Where(PortionVisible => !PortionVisible.IsEmpty())
					?.ToArray();

				var WaypointRegionPortionVisibleLargestPatch =
					WaypointRegionPortionVisible
					?.OrderByDescending(Patch => Math.Min(Patch.Side0Length(), Patch.Side1Length()))
					?.FirstOrDefault();

				if (!(0 < WaypointRegionPortionVisibleLargestPatch?.Side0Length() &&
					0 < WaypointRegionPortionVisibleLargestPatch?.Side1Length()))
				{
					throw new ApplicationException("mouse waypoint region remaining after occlusion is too small");
				}

				var Point =
					WaypointRegionPortionVisibleLargestPatch.Value
					.WithSizeExpandedPivotAtCenter(-MotionMouseWaypointSafetyMarginAdditional * 2)
					.RandomPointInRectangle(Random);

				yield return new Motion(Point);

				if (0 == WaypointIndex)
				{
					//	Mouse Buttons Down
					yield return new Motion(null, Motion?.MouseButton);
				}
			}

			//	Mouse Buttons Up
			yield return new Motion(null, null, Motion?.MouseButton);

			var MotionKeyDown = Motion?.KeyDown;
			var MotionKeyUp = Motion?.KeyUp;

			if (null != MotionKeyDown)
			{
				yield return new Motion(null, KeyDown: MotionKeyDown);
			}

			if (null != MotionKeyUp)
			{
				yield return new Motion(null, KeyUp: MotionKeyUp);
			}

			var MotionTextEntry = Motion?.TextEntry;

			if (0 < MotionTextEntry?.Length)
				yield return new Motion(null, TextEntry: MotionTextEntry);
		}
	}
}
