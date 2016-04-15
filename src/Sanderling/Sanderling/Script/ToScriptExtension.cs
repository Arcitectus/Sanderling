using BotEngine.Motor;
using Sanderling.Interface.MemoryStruct;
using System;
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
			this IHostToScript host,
			IUIElement destination,
			MouseButtonIdEnum[] mouseButton = null) =>
			host?.MotionExecute(new Motor.MotionParam()
			{
				MouseListWaypoint = new[] { new Motor.MotionParamMouseRegion() { UIElement = destination }, },
				MouseButton = mouseButton,
			});

		static public MotionResult MouseClick(
			this IHostToScript host,
			IUIElement destination,
			MouseButtonIdEnum mouseButton) =>
			MouseMove(host, destination, new[] { mouseButton });

		static public MotionResult MouseDragAndDrop(
			this IHostToScript host,
			IUIElement elementToDrag,
			IUIElement destination,
			MouseButtonIdEnum mouseButton) =>
			host?.MotionExecute(new Motor.MotionParam()
			{
				MouseListWaypoint = new[] { elementToDrag, destination }.Select(uIElement => new Motor.MotionParamMouseRegion() { UIElement = uIElement })?.ToArray(),
				MouseButton = new[] { mouseButton },
			});

		static public MotionResult MouseClickLeft(
			this IHostToScript host,
			IUIElement destination) =>
			MouseClick(host, destination, MouseButtonIdEnum.Left);

		static public MotionResult MouseClickRight(
			this IHostToScript host,
			IUIElement destination) =>
			MouseClick(host, destination, MouseButtonIdEnum.Right);

		static public MotionResult MouseDragAndDrop(
			this IHostToScript host,
			IUIElement elementToDrag,
			IUIElement destination) =>
			MouseDragAndDrop(host, elementToDrag, destination, MouseButtonIdEnum.Left);

		static public MotionResult KeyboardPressCombined(
			this IHostToScript sanderling,
			IEnumerable<VirtualKeyCode> setKey) =>
			sanderling?.MotionExecute(new Motor.MotionParam()
			{
				KeyDown = setKey?.ToArray(),
				KeyUp = setKey?.Reverse()?.ToArray(),
			});

		static public MotionResult KeyboardPress(
			this IHostToScript sanderling,
			VirtualKeyCode key) =>
			sanderling?.KeyboardPressCombined(new[] { key });

		static public IEnumerable<MotionResult> KeyboardPressSequence(
			this IHostToScript sanderling,
			IEnumerable<VirtualKeyCode> listKey) =>
			listKey?.Select(key => sanderling?.KeyboardPressCombined(new[] { key }));

		static public MotionResult TextEntry(
			this IHostToScript sanderling,
			string text) =>
			sanderling?.MotionExecute(new Motor.MotionParam()
			{
				TextEntry = text,
			});

		static public MotionResult WindowToForeground(
			this IHostToScript sanderling) =>
			sanderling?.MotionExecute(new Motor.MotionParam()
			{
				WindowToForeground = true,
			});

		static public void InvalidateMeasurement(this IHostToScript sanderling) =>
			sanderling?.InvalidateMeasurement(0);

		static public void WaitForMeasurement(this IHostToScript sanderling) =>
			sanderling?.MemoryMeasurement?.Value?.VersionString?.ToArray();

		static public bool IsCtrlKey(this VirtualKeyCode key) =>
			VirtualKeyCode.CONTROL == key || VirtualKeyCode.LCONTROL == key || VirtualKeyCode.RCONTROL == key;

		static public bool IsAltKey(this VirtualKeyCode key) =>
			VirtualKeyCode.MENU == key || VirtualKeyCode.LMENU == key || VirtualKeyCode.RMENU == key;

		static public bool WindowPostMessage(this IHostToScript host, uint msg, IntPtr wParam, IntPtr lParam = default(IntPtr)) =>
			BotEngine.WinApi.User32.PostMessage(host.WindowHandle, msg, wParam, lParam);

		static public bool WindowPostMessageKeyDown(this IHostToScript host, VirtualKeyCode key, IntPtr lParam = default(IntPtr)) =>
			host.WindowPostMessage(0x100, (IntPtr)key, lParam);

		static public bool WindowPostMessageKeyUp(this IHostToScript host, VirtualKeyCode key, IntPtr lParam = default(IntPtr)) =>
			host.WindowPostMessage(0x101, (IntPtr)key, lParam);
	}
}
