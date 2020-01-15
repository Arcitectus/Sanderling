using Bib3;
using Bib3.FCL.UI;
using Sanderling.UI;
using BotEngine.Interface;
using BotEngine.UI;
using System.Linq;
using BotSharp.UI.Wpf;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows;
using System;

namespace Sanderling.Exe
{
	partial class App
	{
		Main MainControl => Window?.Main;

		public IEnumerable<IEnumerable<Key>> SetKeyBotMotionDisable()
		{
			yield return new[] { Key.LeftCtrl, Key.LeftAlt };
			yield return new[] { Key.RightCtrl, Key.RightAlt };
		}

		StatusIcon.StatusEnum BotStatus => ScriptEngineStatus;

		StatusIcon.StatusEnum ScriptEngineStatus =>
			ScriptRun?.StatusIcon() ?? StatusIcon.StatusEnum.None;

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			ProcessInput();
		}

		IEnumerable<(UIElement button, Action action)> ButtonsActions => new (UIElement button, Action action)[]
		{
			(BotOperationPauseContinueToggleButton?.ButtonLinx, PauseBotOperation),
			(BotOperationPauseContinueToggleButton?.ButtonRecz, () => ContinueOrStartBotOperation(StartOrContinueBotTrigger.UserInterface)),
		}
		.Where(buttonAction => buttonAction.button != null);

		void ButtonClicked(object sender, RoutedEventArgs e)
		{
			foreach (var buttonAction in ButtonsActions)
			{
				if (buttonAction.button == e?.OriginalSource)
					buttonAction.action();
			}
		}

		public void ProcessInput()
		{
			if (SetKeyBotMotionDisable()?.Any(setKey => setKey?.All(key => Keyboard.IsKeyDown(key)) ?? false) ?? false)
				PauseBotOperation();
		}

		void UpdateBotOperationPauseContinueToggleButton()
		{
			BotOperationPauseContinueToggleButton.Visibility =
				(ScriptRun?.HasStarted ?? false) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;

			BotOperationPauseContinueToggleButton?.ButtonRecz?.SetValue(ToggleButton.IsCheckedProperty, ScriptRun?.IsRunning ?? false);
		}

		void UIPresent()
		{
			MainControl?.BotHeader?.SetStatus(BotStatus);
			MainControl?.BotsNavigation?.UpdateViewComponentsWithState();

			BotAPIExplorer?.Present(UIAPI);

			InterfaceToEveControl?.Present(SensorServerDispatcher, MemoryMeasurementLast?.MapValue(evaluation => evaluation?.MemoryMeasurement));

			MainControl?.InterfaceHeader?.SetStatus(InterfaceToEveControl.InterfaceStatusEnum());
		}
	}
}
