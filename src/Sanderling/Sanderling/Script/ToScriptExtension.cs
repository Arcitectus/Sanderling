using BotEngine.Motor;
using Sanderling.Interface.MemoryStruct;
using System.Collections.Generic;
using System.Linq;
using WindowsInput.Native;

namespace Sanderling.Script
{
	/// <summary>
	/// Extension methods for consumption by scripts.
	/// </summary>
	static public class ToScriptExtension
	{
		static public MotionResult MouseMove(
			this IHostToScript Host,
			IUIElement Destination,
			MouseButtonIdEnum[] MouseButton = null) =>
			Host?.MotionExecute(new Motor.MotionParam()
			{
				MouseListWaypoint = new[] { new Motor.MotionParamMouseRegion() { UIElement = Destination }, },
				MouseButton = MouseButton,
			});

		static public MotionResult MouseClick(
			this IHostToScript Host,
			IUIElement Destination,
			MouseButtonIdEnum MouseButton) =>
			MouseMove(Host, Destination, new[] { MouseButton });

		static public MotionResult MouseDragAndDrop(
			this IHostToScript Host,
			IUIElement ElementToDrag,
			IUIElement Destination,
			MouseButtonIdEnum MouseButton) =>
			Host?.MotionExecute(new Motor.MotionParam()
			{
				MouseListWaypoint = new[] { ElementToDrag, Destination }.Select(UIElement => new Motor.MotionParamMouseRegion() { UIElement = UIElement })?.ToArray(),
				MouseButton = new[] { MouseButton },
			});

		static public MotionResult MouseClickLeft(
			this IHostToScript Host,
			IUIElement Destination) =>
			MouseClick(Host, Destination, MouseButtonIdEnum.Left);

		static public MotionResult MouseClickRight(
			this IHostToScript Host,
			IUIElement Destination) =>
			MouseClick(Host, Destination, MouseButtonIdEnum.Right);

		static public MotionResult MouseDragAndDrop(
			this IHostToScript Host,
			IUIElement ElementToDrag,
			IUIElement Destination) =>
			MouseDragAndDrop(Host, ElementToDrag, Destination, MouseButtonIdEnum.Left);

		static public MotionResult KeyboardPressCombined(
			this IHostToScript Sanderling,
			IEnumerable<VirtualKeyCode> SetKey) =>
			Sanderling?.MotionExecute(new Motor.MotionParam()
			{
				KeyDown = SetKey?.ToArray(),
				KeyUp = SetKey?.Reverse()?.ToArray(),
			});

		static public MotionResult KeyboardPress(
			this IHostToScript Sanderling,
			VirtualKeyCode Key) =>
			Sanderling?.KeyboardPressCombined(new[] { Key });

		static public IEnumerable<MotionResult> KeyboardPressSequence(
			this IHostToScript Sanderling,
			IEnumerable<VirtualKeyCode> ListKey) =>
			ListKey?.Select(Key => Sanderling?.KeyboardPressCombined(new[] { Key }));

		static public MotionResult TextEntry(
			this IHostToScript Sanderling,
			string Text) =>
			Sanderling?.MotionExecute(new Motor.MotionParam()
			{
				TextEntry = Text,
			});

		static public MotionResult WindowToForeground(
			this IHostToScript Sanderling) =>
			Sanderling?.MotionExecute(new Motor.MotionParam()
			{
				WindowToForeground = true,
			});

		static public void InvalidateMeasurement(this IHostToScript Sanderling) =>
			Sanderling?.InvalidateMeasurement(0);

		static public void WaitForMeasurement(this IHostToScript Sanderling) =>
			Sanderling?.MemoryMeasurement?.Value?.VersionString?.ToArray();

		static public bool IsCtrlKey(this VirtualKeyCode Key) =>
			VirtualKeyCode.CONTROL == Key || VirtualKeyCode.LCONTROL == Key || VirtualKeyCode.RCONTROL == Key;

		static public bool IsAltKey(this VirtualKeyCode Key) =>
			VirtualKeyCode.MENU == Key || VirtualKeyCode.LMENU == Key || VirtualKeyCode.RMENU == Key;

	}
}
