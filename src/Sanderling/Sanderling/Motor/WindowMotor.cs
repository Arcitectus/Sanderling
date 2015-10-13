using BotEngine.Motor;
using System;
using System.Collections.Generic;
using System.Linq;
using Bib3;
using BotEngine.Windows;
using BotEngine.WinApi;
using System.Threading;

namespace Sanderling.Motor
{
	public class WindowMotor : IMotor
	{
		readonly public IntPtr WindowHandle;

		public int MouseMoveDelay;

		public int MouseEventDelay;

		/// <summary>
		/// For some reason, the mouse positions seem to be offset by -1,-1 when moving the mouse in the window client area.
		/// </summary>
		static public Vektor2DInt MouseOffsetStatic = new Vektor2DInt(1, 1);

		public WindowMotor(IntPtr WindowHandle)
		{
			this.WindowHandle = WindowHandle;
		}

		static public void EnsureWindowIsForeground(
			IntPtr WindowHandle)
		{
			var PreviousForegroundWindowHandle = BotEngine.WinApi.User32.GetForegroundWindow();

			if (PreviousForegroundWindowHandle == WindowHandle)
			{
				return;
			}

			BotEngine.WinApi.User32.SetForegroundWindow(WindowHandle);
		}

		static public void MouseMoveToPointInClientRect(
			IntPtr WindowHandle,
			Vektor2DInt DestinationPointInClientRect,
			out POINT DestinationPointInScreen)
		{
			DestinationPointInScreen = DestinationPointInClientRect.AsWindowsPoint();

			// get screen coordinates
			BotEngine.WinApi.User32.ClientToScreen(WindowHandle, ref DestinationPointInScreen);

			var lParam = (IntPtr)((((int)DestinationPointInClientRect.B) << 16) | ((int)DestinationPointInClientRect.A));
			var wParam = IntPtr.Zero;

			BotEngine.WinApi.User32.SetCursorPos(DestinationPointInScreen.x, DestinationPointInScreen.y);

			BotEngine.WinApi.User32.SendMessage(WindowHandle, (uint)SictMessageTyp.WM_MOUSEMOVE, wParam, lParam);
		}

		/// <summary>
		/// https://msdn.microsoft.com/en-us/library/windows/desktop/ms646260(v=vs.85).aspx
		/// </summary>
		static KeyValuePair<MouseButtonIdEnum, int>[] MouseEventButtonDownFlag = new[]
		{
			new KeyValuePair<MouseButtonIdEnum, int>( MouseButtonIdEnum.Left, (int)User32.MouseEventFlagEnum.MOUSEEVENTF_LEFTDOWN),
			new KeyValuePair<MouseButtonIdEnum, int>( MouseButtonIdEnum.Right, (int)User32.MouseEventFlagEnum.MOUSEEVENTF_RIGHTDOWN),
			new KeyValuePair<MouseButtonIdEnum, int>( MouseButtonIdEnum.Left, (int)User32.MouseEventFlagEnum.MOUSEEVENTF_MIDDLEDOWN),
		};

		/// <summary>
		/// https://msdn.microsoft.com/en-us/library/windows/desktop/ms646260(v=vs.85).aspx
		/// </summary>
		static KeyValuePair<MouseButtonIdEnum, int>[] MouseEventButtonUpFlag = new[]
		{
			new KeyValuePair<MouseButtonIdEnum, int>( MouseButtonIdEnum.Left, (int)User32.MouseEventFlagEnum.MOUSEEVENTF_LEFTUP),
			new KeyValuePair<MouseButtonIdEnum, int>( MouseButtonIdEnum.Right, (int)User32.MouseEventFlagEnum.MOUSEEVENTF_RIGHTUP),
			new KeyValuePair<MouseButtonIdEnum, int>( MouseButtonIdEnum.Left, (int)User32.MouseEventFlagEnum.MOUSEEVENTF_MIDDLEUP),
		};

		static int User32MouseEventFlagAggregate(
			IEnumerable<MouseButtonIdEnum> MouseButtonDown,
			IEnumerable<MouseButtonIdEnum> MouseButtonUp) =>
			MouseEventButtonDownFlag.Where(ButtonIdAndFlag => MouseButtonDown?.Contains(ButtonIdAndFlag.Key) ?? false)
				.Concat(
				MouseEventButtonUpFlag.Where(ButtonIdAndFlag => MouseButtonUp?.Contains(ButtonIdAndFlag.Key) ?? false))
				.Select(ButtonIdAndFlag => ButtonIdAndFlag.Value)
				.Aggregate(0, (a, b) => a | b);

		static public void User32MouseEvent(
			Vektor2DInt MousePositionOnScreen,
			IEnumerable<MouseButtonIdEnum> MouseButtonDown,
			IEnumerable<MouseButtonIdEnum> MouseButtonUp)
		{
			var MouseEventFlag = User32MouseEventFlagAggregate(MouseButtonDown, MouseButtonUp);

			User32.mouse_event(
				(uint)MouseEventFlag | (uint)User32.MouseEventFlagEnum.MOUSEEVENTF_ABSOLUTE,
				(uint)MousePositionOnScreen.A,
				(uint)MousePositionOnScreen.B,
				0,
				UIntPtr.Zero);
		}

		public MotionResult ActSequenceMotion(IEnumerable<Motion> SeqMotion)
		{
			try
			{
				if (null == SeqMotion)
				{
					return null;
				}

				var MousePosition = BotEngine.Windows.Extension.User32GetCursorPos() ?? new Vektor2DInt(0, 0);

				var InputSimulator = new WindowsInput.InputSimulator();
				var KeyboardSimulator = new WindowsInput.KeyboardSimulator(InputSimulator);

				foreach (var Motion in SeqMotion)
				{
					var MotionMousePosition = Motion?.MousePosition;

					if (MotionMousePosition.HasValue)
					{
						POINT PositionOnScreen;

						EnsureWindowIsForeground(WindowHandle);

						MouseMoveToPointInClientRect(WindowHandle, MotionMousePosition.Value + MouseOffsetStatic, out PositionOnScreen);

						MousePosition = PositionOnScreen.AsVektor2DInt();

						Thread.Sleep(MouseMoveDelay);
					}

					if (0 < Motion?.MouseButtonDown?.Count() || 0 < Motion?.MouseButtonUp?.Count())
					{
						EnsureWindowIsForeground(WindowHandle);

						User32MouseEvent(MousePosition, Motion?.MouseButtonDown, Motion?.MouseButtonUp);

						Thread.Sleep(MouseEventDelay);
					}

					Motion?.KeyDown?.ForEachNullable(KeyDown =>
					{
						EnsureWindowIsForeground(WindowHandle);
						InputSimulator.Keyboard.KeyDown(KeyDown);
					});

					Motion?.KeyUp?.ForEachNullable(KeyUp =>
					{
						EnsureWindowIsForeground(WindowHandle);
						InputSimulator.Keyboard.KeyUp(KeyUp);
					});
				}

				return new MotionResult(true);
			}
			catch (Exception Exception)
			{
				return new MotionResult(Exception);
			}
		}
	}
}
