using Bib3;
using Bib3.Geometrik;
using BotEngine.Motor;
using Sanderling.Interface.MemoryStruct;
using System.Collections.Generic;
using System.Linq;

namespace Sanderling.Motor
{
	public class MotionParamMouseRegion
	{
		public IUIElement UIElement;

		public OrtogoonInt? RegionReplacement;

		public MotionParamMouseRegion()
		{
		}
	}

	public class MotionParam
	{
		public MotionParamMouseRegion[] MouseListWaypoint;

		public MouseButtonIdEnum[] MouseButton;

		public WindowsInput.Native.VirtualKeyCode[] Key;

		public MotionParam()
		{
		}

		public MotionParam(
			MotionParamMouseRegion[] MouseListWaypoint,
			MouseButtonIdEnum[] MouseButton = null)
		{
			this.MouseListWaypoint = MouseListWaypoint;
			this.MouseButton = MouseButton;
		}

		public MotionParam(
			MotionParamMouseRegion[] MouseListWaypoint,
			MouseButtonIdEnum MouseButton)
			:
			this(MouseListWaypoint, new MouseButtonIdEnum[] { MouseButton })
		{
		}

		public IEnumerable<object> MotionDescription
		{
			get
			{
				var MouseListWaypoint = this.MouseListWaypoint?.WhereNotDefault()?.ToArray();

				var MouseWaypointFirst = MouseListWaypoint?.FirstOrDefault();
				var MouseWaypointLast = MouseListWaypoint?.LastOrDefault();

				if (null != MouseWaypointFirst)
				{
					if (MouseButton.NullOderLeer())
					{
						yield return "move";
					}
					else
					{
						yield return "click";
					}

					yield return "on";
					yield return MouseWaypointFirst.UIElement;
				}

				if (null != MouseWaypointLast && MouseWaypointFirst != MouseWaypointLast)
				{
					yield return "and drag to";
					yield return MouseWaypointLast.UIElement;
				}

				if (0 < Key?.Length)
				{
					yield return "Press Key[" + string.Join(",", Key?.Select(key => key.ToString())) + "]";
				}
			}
		}
	}
}
