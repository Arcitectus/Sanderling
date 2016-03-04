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

		StatusIcon.StatusEnum InterfaceStatus =>
			InterfaceMemoryMeasurementLastStatus;

		StatusIcon.StatusEnum InterfaceProcessStatus =>
			(MainControl?.Interface?.ProcessChoice?.ChoosenProcessAtTime.Wert?.BewertungMainModuleDataiNaamePasend ?? false) ?
			StatusIcon.StatusEnum.Accept : StatusIcon.StatusEnum.None;

		StatusIcon.StatusEnum InterfaceLicenseStatus =>
			MainControl?.Interface?.LicenseClientInspect?.Status()?.FirstOrDefault() ?? StatusIcon.StatusEnum.None;

		public IEnumerable<IEnumerable<Key>> SetKeyBotMotionDisable()
		{
			yield return new[] { Key.LeftCtrl, Key.LeftAlt };
			yield return new[] { Key.RightCtrl, Key.RightAlt };
		}

		StatusIcon.StatusEnum InterfaceMemoryMeasurementLastStatus
		{
			get
			{
				var MemoryMeasurementLast = this.MemoryMeasurementLast;

				var MemoryMeasurementLastAge = GetTimeStopwatch() - MemoryMeasurementLast?.Begin;

				if (!(MemoryMeasurementLastAge < 8000))
				{
					return StatusIcon.StatusEnum.Reject;
				}

				if (!Bib3.RefNezDiferenz.Extension.EnumMengeRefAusNezAusWurzel(
						MemoryMeasurementLast?.Value?.MemoryMeasurement,
						Interface.FromInterfaceResponse.UITreeComponentTypeHandlePolicyCache).CountAtLeast(1))
				{
					return StatusIcon.StatusEnum.Reject;
				}

				return StatusIcon.StatusEnum.Accept;
			}
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
			MainControl?.InterfaceHeader?.SetStatus(InterfaceStatus);
			MainControl?.Interface?.ProcessHeader?.SetStatus(InterfaceProcessStatus);
			MainControl?.Interface?.LicenseHeader?.SetStatus(InterfaceLicenseStatus);
			MainControl?.Interface?.MeasurementLastHeader?.SetStatus(InterfaceMemoryMeasurementLastStatus);
			MainControl?.BotHeader?.SetStatus(BotStatus);
			MainControl?.Bot?.ScriptEngineHeader?.SetStatus(ScriptEngineStatus);

			BotAPIExplorer?.Present(UIAPI);

			LicenseClientInspect?.Present(LicenseClient?.Value);

			InterfaceToEveControl?.Measurement?.Present(MemoryMeasurementLast?.MapValue(Evaluation => Evaluation?.MemoryMeasurement));
		}
	}
}
