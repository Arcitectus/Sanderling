using Bib3;
using Bib3.Synchronization;
using BotEngine.Interface;
using Optimat.EveOnline;
using Sanderling.Interface;
using Sanderling.Interface.MemoryStruct;
using System;
using System.Linq;
using System.Threading;

namespace Sanderling
{
	public class Sensor
	{
		class MemoryMeasurementInitReport
		{
			public MemoryMeasurementInitParam Param;

			public SictProcessMitIdAuswertWurzelSuuce ForDerived;

			public Int64[] SetRootAdr;
		}

		class MemoryMeasurementReport
		{
			public MemoryMeasurementInitReport DerivedFrom;

			public FromProcessMeasurement<GbsAstInfo> Raw;

			public FromProcessMeasurement<IMemoryMeasurement> ViewInterface;
		}

		const int EveOnlineSensoGbsMengeAstAnzaalScrankeMax = 50000;
		const int EveOnlineSensoGbsAstListeChildAnzaalScrankeMax = 0x200;
		const int EveOnlineSensoGbsSuuceTiifeScrankeMax = 0x30;

		readonly object MeasurementLock = new object();

		FromProcessMeasurement<MemoryMeasurementInitReport> MemoryMeasurementInitLastReport;

		FromProcessMeasurement<MemoryMeasurementReport> MemoryMeasurementLastReport;

		FromProcessMeasurement<MemoryMeasurementInitParam> MemoryMeasurementInitLast =>
			MemoryMeasurementInitLastReport?.MapValue(report => report?.Param);

		FromProcessMeasurement<IMemoryMeasurement> MemoryMeasurementLast =>
			MemoryMeasurementLastReport?.Value?.ViewInterface;

		public FromInterfaceResponse ClientRequest(ToInterfaceRequest request)
		{
			if (null == request)
				return null;

			try
			{
				var MemoryMeasurementInitTake = null != request.MemoryMeasurementInitTake;

				if (MemoryMeasurementInitTake)
					this.MemoryMeasurementInitTake(request.MemoryMeasurementInitTake);

				if (request.MemoryMeasurementTake)
					MemoryMeasurementTake();

				return new FromInterfaceResponse
				{
					MemoryMeasurementInit =
					(MemoryMeasurementInitTake || request.MemoryMeasurementInitGetLast) ? MemoryMeasurementInitLast : null,

					MemoryMeasurement =
						request.MemoryMeasurementTake || request.MemoryMeasurementGetLast ? MemoryMeasurementLast : null,

					MemoryMeasurementInProgress = MeasurementLock.IsLocked(),
				};
			}
			catch
			{
				return new FromInterfaceResponse
				{
				};
			}
		}

		FromProcessMeasurement<MemoryMeasurementInitReport> MemoryMeasurementInitTake(MemoryMeasurementInitParam param)
		{
			if (null == param)
				return null;

			lock (MeasurementLock)
			{
				var StartTimeMilli = Bib3.Glob.StopwatchZaitMiliSictInt();

				var Measurement = new MemoryMeasurementInitReport
				{
					Param = param,
				};

				var ProcessId = param.ProcessId;

				var SuuceWurzel = new SictProcessMitIdAuswertWurzelSuuce(ProcessId);

				SuuceWurzel.Berecne();

				Measurement.ForDerived = SuuceWurzel;

				Measurement.SetRootAdr =
					SuuceWurzel?.GbsMengeWurzelObj
					?.Select(wurzelObj => wurzelObj?.HerkunftAdrese)
					?.WhereNotNullSelectValue()
					?.ToArray();

				var EndTimeMilli = Bib3.Glob.StopwatchZaitMiliSictInt();

				var ProcessMeasurementReport = new FromProcessMeasurement<MemoryMeasurementInitReport>(
					Measurement,
					StartTimeMilli,
					EndTimeMilli,
					ProcessId);

				Thread.MemoryBarrier();

				return MemoryMeasurementInitLastReport = ProcessMeasurementReport;
			}
		}

		FromProcessMeasurement<MemoryMeasurementReport> MemoryMeasurementTake()
		{
			lock (MeasurementLock)
			{
				var StartTimeMilli = Bib3.Glob.StopwatchZaitMiliSictInt();

				var MeasurementInit = MemoryMeasurementInitLastReport;

				var DerivedFrom = MeasurementInit?.Value;

				var Measurement = new MemoryMeasurementReport
				{
					DerivedFrom = DerivedFrom,
				};

				var ProcessId = MeasurementInit?.ProcessId ?? 0;

				var MeasurementInitPortionForDerived = DerivedFrom?.ForDerived;

				Sanderling.Parse.Culture.InvokeInParseCulture(() =>
				{
					var ScnapscusAuswert =
						new SictProzesAuswertZuusctandScpezGbsBaum(
							new ProcessMemoryReader(ProcessId),
							MeasurementInitPortionForDerived,
							EveOnlineSensoGbsAstListeChildAnzaalScrankeMax,
							EveOnlineSensoGbsMengeAstAnzaalScrankeMax,
							EveOnlineSensoGbsSuuceTiifeScrankeMax);

					ScnapscusAuswert.BerecneScrit();

					var GbsBaumDirekt = ScnapscusAuswert.GbsWurzelHauptInfo;

					Measurement.Raw =
						new FromProcessMeasurement<GbsAstInfo>(GbsBaumDirekt, StartTimeMilli, Bib3.Glob.StopwatchZaitMiliSictInt(), ProcessId);

					var GbsBaumAuswert =
						Optimat.EveOnline.AuswertGbs.Extension.SensorikScnapscusKonstrukt(
							ScnapscusAuswert.GbsWurzelHauptInfo, (int?)(1e+9));

					Measurement.ViewInterface =
						new FromProcessMeasurement<IMemoryMeasurement>(GbsBaumAuswert, StartTimeMilli, Bib3.Glob.StopwatchZaitMiliSictInt(), ProcessId);
				});

				var EndTimeMilli = Bib3.Glob.StopwatchZaitMiliSictInt();

				var ProcessMeasurementReport = new FromProcessMeasurement<MemoryMeasurementReport>(
					Measurement,
					StartTimeMilli,
					EndTimeMilli,
					ProcessId);

				Thread.MemoryBarrier();

				return MemoryMeasurementLastReport = ProcessMeasurementReport;
			}
		}
	}
}
