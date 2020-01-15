using Bib3;
using Bib3.FCL.UI;
using BotEngine.Interface;
using BotEngine.UI;
using Sanderling.Interface.MemoryStruct;
using System;

namespace Sanderling.UI
{
	static public class Extension
	{
		static public StatusIcon.StatusEnum LicenseStatusEnum(this SimpleInterfaceServerDispatcher dispatcher) =>
			(dispatcher?.AppInterfaceAvailable ?? false) ? StatusIcon.StatusEnum.Accept : StatusIcon.StatusEnum.Reject;

		static public StatusIcon.StatusEnum ProcessStatusEnum(this SictAuswaalWindowsProcess processChoice) =>
			(processChoice?.ChoosenProcessAtTime.Wert?.BewertungMainModuleDataiNaamePasend ?? false) ?
			StatusIcon.StatusEnum.Accept : StatusIcon.StatusEnum.None;

		static public StatusIcon.StatusEnum MemoryMeasurementLastStatusEnum(
			this FromProcessMeasurement<IMemoryMeasurement> measurement,
			Int64 measurementTimeMin)
		{
			if (!(measurementTimeMin < measurement?.Begin))
				return StatusIcon.StatusEnum.Reject;

			if (!Bib3.RefNezDiferenz.Extension.EnumMengeRefAusNezAusWurzel(measurement?.Value, Interface.FromInterfaceResponse.UITreeComponentTypeHandlePolicyCache).CountAtLeast(1))
				return StatusIcon.StatusEnum.Reject;

			if (!(measurement?.Value?.SessionDurationRemainingSufficientToStayExposed() ?? false))
				return StatusIcon.StatusEnum.Warn;

			return StatusIcon.StatusEnum.Accept;
		}

		static public StatusIcon.StatusEnum MemoryMeasurementLastStatusEnum(
			this FromProcessMeasurement<IMemoryMeasurement> measurement) =>
			MemoryMeasurementLastStatusEnum(measurement, Bib3.Glob.StopwatchZaitMiliSictInt() - 8000);

		static public StatusIcon.StatusEnum InterfaceStatusEnum(this InterfaceToEve view) =>
			new[]
			{
				view?.ProcessHeader?.Status ?? StatusIcon.StatusEnum.Reject,
				view?.MeasurementLastHeader?.Status ?? StatusIcon.StatusEnum.Reject,
			}.AggregateStatus().FirstOrNull() ?? StatusIcon.StatusEnum.Reject;
	}
}
