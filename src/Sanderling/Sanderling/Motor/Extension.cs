using BotEngine.Common;
using BotEngine.Motor;
using System;
using System.Collections.Generic;
using System.Linq;
using Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;

namespace Sanderling.Motor
{
	static public class Extension
	{
		public const int MotionMouseWaypointSafetyMarginMin = 2;

		public const int MotionMouseWaypointSafetyMarginAdditional = 0;

		static public IEnumerable<Motion> AsSequenceMotion(
			this MotionParam Motion,
			object MemoryMeasurement)
		{
			if (null == Motion)
			{
				yield break;
			}

			var Random = new Random((int)Bib3.Glob.StopwatchZaitMiliSictInt());

			var MouseListWaypoint = Motion?.MouseListWaypoint;

			for (int WaypointIndex = 0; WaypointIndex < (MouseListWaypoint?.Length ?? 0); WaypointIndex++)
			{
				var MouseWaypoint = MouseListWaypoint[WaypointIndex];

				var WaypointUIElement = MouseWaypoint?.UIElement;
				var WaypointRegionReplacement = MouseWaypoint.RegionReplacement;

				var WaypointUIElementCurrent =
					WaypointUIElement.GetInstanceWithIdFromCLRGraph(MemoryMeasurement, Interface.FromSensorToConsumerMessage.SerialisPolicyCache);

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
					WaypointUIElementCurrent.GetOccludedUIElementRemainingRegion(MemoryMeasurement)
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

			var MotionKey = Motion?.Key;

			if (null != MotionKey)
			{
				yield return new Motion(null, null, null, MotionKey);
				yield return new Motion(null, null, null, null, MotionKey);
			}
		}
	}
}
