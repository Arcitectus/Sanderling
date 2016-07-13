using BotEngine.Motor;
using Sanderling.Interface.MemoryStruct;
using System.Collections.Generic;
using System.Linq;
using WindowsInput.Native;

namespace Sanderling.Motor
{
	static public class MotionParamExtension
	{
		static public MotionParam MouseMove(
			this IUIElement destination,
			MouseButtonIdEnum[] mouseButton = null) =>
			new MotionParam
			{
				MouseListWaypoint = new[] { new MotionParamMouseRegion() { UIElement = destination }, },
				MouseButton = mouseButton,
			};

		static public MotionParam MouseClick(
			this IUIElement destination,
			MouseButtonIdEnum mouseButton) =>
			MouseMove(destination, new[] { mouseButton });

		static public MotionParam MouseDragAndDropOn(
			this IUIElement elementToDrag,
			IUIElement destination,
			MouseButtonIdEnum mouseButton) =>
			new MotionParam
			{
				MouseListWaypoint = new[] { elementToDrag, destination }.Select(uIElement => new MotionParamMouseRegion() { UIElement = uIElement })?.ToArray(),
				MouseButton = new[] { mouseButton },
			};

		static public MotionParam KeyboardPressCombined(
			this IEnumerable<VirtualKeyCode> setKey) =>
			new MotionParam
			{
				KeyDown = setKey?.ToArray(),
				KeyUp = setKey?.Reverse()?.ToArray(),
			};

		static public MotionParam KeyboardPress(
			this VirtualKeyCode key) =>
			KeyboardPressCombined(new[] { key });

		static public IEnumerable<MotionParam> KeyboardPressSequence(
			this IEnumerable<VirtualKeyCode> listKey) =>
			listKey?.Select(key => KeyboardPressCombined(new[] { key }));

		static public MotionParam KeyDown(
			this IEnumerable<VirtualKeyCode> listKey) =>
			new MotionParam
			{
				KeyDown = listKey?.ToArray(),
			};

		static public MotionParam KeyUp(
			this IEnumerable<VirtualKeyCode> listKey) =>
			new MotionParam
			{
				KeyUp = listKey?.ToArray(),
			};

		static public MotionParam KeyDown(
			this VirtualKeyCode key) => KeyDown(new[] { key });

		static public MotionParam KeyUp(
			this VirtualKeyCode key) => KeyUp(new[] { key });

		static public MotionParam TextEntry(
			this string text) =>
			new MotionParam
			{
				TextEntry = text,
			};
	}
}
