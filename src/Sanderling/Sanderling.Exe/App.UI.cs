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

		public void ProcessInput()
		{
			if (SetKeyBotMotionDisable()?.Any(setKey => setKey?.All(key => System.Windows.Input.Keyboard.IsKeyDown(key)) ?? false) ?? false)
			{
				ScriptRun?.Break();
			}
		}

		void UIPresentScript()
		{
			ToggleButtonMotionEnable?.ButtonRecz?.SetValue(ToggleButton.IsCheckedProperty, ScriptRun?.IsRunning ?? false);

			ScriptIDE?.Present();
		}

		void UIPresent()
		{
			MainControl?.BotHeader?.SetStatus(BotStatus);
			MainControl?.Bot?.ScriptEngineHeader?.SetStatus(ScriptEngineStatus);

			BotAPIExplorer?.Present(UIAPI);

			InterfaceToEveControl?.Present(SensorServerDispatcher, MemoryMeasurementLast?.MapValue(evaluation => evaluation?.MemoryMeasurement));

			MainControl?.InterfaceHeader?.SetStatus(InterfaceToEveControl.InterfaceStatusEnum());
		}
	}
}
