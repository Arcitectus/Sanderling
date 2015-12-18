using Sanderling.Interface.MemoryStruct;
using System.Collections.Generic;
using System.Linq;

namespace Sanderling.Script
{
	/// <summary>
	/// Extension methods for consumption by scripts.
	/// </summary>
	static public class ToScriptExtension
	{
		static public BotEngine.Motor.MotionResult MouseMove(
			this IHostToScript Host,
			IUIElement Destination,
			BotEngine.Motor.MouseButtonIdEnum[] MouseButton = null) =>
			Host?.MotionExecute(new Motor.MotionParam()
			{
				MouseListWaypoint = new[] { new Motor.MotionParamMouseRegion() { UIElement = Destination }, },
				MouseButton = MouseButton,
			});

		static public BotEngine.Motor.MotionResult MouseClick(
			this IHostToScript Host,
			IUIElement Destination,
			BotEngine.Motor.MouseButtonIdEnum MouseButton) =>
			MouseMove(Host, Destination, new[] { MouseButton });

		static public BotEngine.Motor.MotionResult MouseDragAndDrop(
			this IHostToScript Host,
			IUIElement ElementToDrag,
			IUIElement Destination,
			BotEngine.Motor.MouseButtonIdEnum MouseButton) =>
			Host?.MotionExecute(new Motor.MotionParam()
			{
				MouseListWaypoint = new[] { ElementToDrag, Destination }.Select(UIElement => new Motor.MotionParamMouseRegion() { UIElement = UIElement })?.ToArray(),
				MouseButton = new[] { MouseButton },
			});

		static public BotEngine.Motor.MotionResult MouseClickLeft(
			this IHostToScript Host,
			IUIElement Destination) =>
			MouseClick(Host, Destination, BotEngine.Motor.MouseButtonIdEnum.Left);

		static public BotEngine.Motor.MotionResult MouseClickRight(
			this IHostToScript Host,
			IUIElement Destination) =>
			MouseClick(Host, Destination, BotEngine.Motor.MouseButtonIdEnum.Right);

		static public BotEngine.Motor.MotionResult MouseDragAndDrop(
			this IHostToScript Host,
			IUIElement ElementToDrag,
			IUIElement Destination) =>
			MouseDragAndDrop(Host, ElementToDrag, Destination, BotEngine.Motor.MouseButtonIdEnum.Left);

		static public BotEngine.Motor.MotionResult KeyboardPressCombined(
			this IHostToScript Sanderling,
			IEnumerable<WindowsInput.Native.VirtualKeyCode> SetKey) =>
			Sanderling?.MotionExecute(new Motor.MotionParam()
			{
				KeyDown = SetKey?.ToArray(),
				KeyUp = SetKey?.Reverse()?.ToArray(),
			});

		static public BotEngine.Motor.MotionResult KeyboardPress(
			this IHostToScript Sanderling,
			WindowsInput.Native.VirtualKeyCode Key) =>
			Sanderling?.KeyboardPressCombined(new[] { Key });

		static public IEnumerable<BotEngine.Motor.MotionResult> KeyboardPressSequence(
			this IHostToScript Sanderling,
			IEnumerable<WindowsInput.Native.VirtualKeyCode> ListKey) =>
			ListKey?.Select(Key => Sanderling?.KeyboardPressCombined(new[] { Key }));

		static public BotEngine.Motor.MotionResult TextEntry(
			this IHostToScript Sanderling,
			string Text) =>
			Sanderling?.MotionExecute(new Motor.MotionParam()
			{
				TextEntry = Text,
			});

		static public BotEngine.Motor.MotionResult WindowToForeground(
			this IHostToScript Sanderling) =>
			Sanderling?.MotionExecute(new Motor.MotionParam()
			{
				WindowToForeground = true,
			});

		static public void InvalidateMeasurement(this IHostToScript Sanderling) =>
			Sanderling?.InvalidateMeasurement(0);

		static public void WaitForMeasurement(this IHostToScript Sanderling) =>
			Sanderling?.MemoryMeasurement?.Value?.VersionString?.ToArray();
	}
}
