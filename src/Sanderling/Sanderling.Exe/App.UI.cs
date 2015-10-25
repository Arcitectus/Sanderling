using Bib3;
using Bib3.FCL.UI;
using Sanderling.UI;
using BotEngine.Interface;
using BotEngine.UI;
using System.Linq;
using BotScript.UI.Wpf;

namespace Sanderling.Exe
{
	partial class App
	{
		Main MainControl => Window?.Main;

		StatusIcon.StatusEnum InterfaceStatus =>
			InterfaceMemoryMeasurementLastStatus;

		StatusIcon.StatusEnum InterfaceProcessStatus =>
			(MainControl?.Interface?.ProcessChoice?.ChoosenProcessAtTime.Wert?.BewertungMainModuleDataiNaamePasend ?? false) ?
			StatusIcon.StatusEnum.Acceptance : StatusIcon.StatusEnum.None;

		StatusIcon.StatusEnum InterfaceLicenseStatus =>
			MainControl?.Interface?.LicenseClientInspect?.Status()?.FirstOrDefault() ?? StatusIcon.StatusEnum.None;

		StatusIcon.StatusEnum InterfaceMemoryMeasurementLastStatus
		{
			get
			{
				var MemoryMeasurementLast = this.MemoryMeasurementLast;

				var MemoryMeasurementLastAge = GetTimeStopwatch() - MemoryMeasurementLast?.Begin;

				if (!(MemoryMeasurementLastAge < 8000))
				{
					return StatusIcon.StatusEnum.Error;
				}

				if (!Bib3.RefNezDiferenz.Extension.EnumMengeRefAusNezAusWurzel(
						MemoryMeasurementLast?.Value?.MemoryMeasurement,
						Interface.FromSensorToConsumerMessage.UITreeComponentTypeHandlePolicyCache).CountAtLeast(1))
				{
					return StatusIcon.StatusEnum.Error;
				}

				return StatusIcon.StatusEnum.Acceptance;
			}
		}

		StatusIcon.StatusEnum BotStatus => ScriptEngineStatus;

		StatusIcon.StatusEnum ScriptEngineStatus =>
			ScriptRun?.StatusIcon() ?? StatusIcon.StatusEnum.None;

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
