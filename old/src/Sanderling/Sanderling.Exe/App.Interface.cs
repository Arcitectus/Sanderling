using Bib3;
using Bib3.Synchronization;
using BotEngine.Interface;
using Sanderling.Interface;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sanderling.Exe
{
	/// <summary>
	/// This Type must reside in an Assembly that can be resolved by the default assembly resolver.
	/// </summary>
	public class InterfaceAppDomainSetup
	{
		static InterfaceAppDomainSetup()
		{
			BotEngine.Interface.InterfaceAppDomainSetup.Setup();
		}
	}

	partial class App
	{
		readonly SimpleInterfaceServerDispatcher SensorServerDispatcher = new SimpleInterfaceServerDispatcher
		{
			InterfaceAppDomainSetupType = typeof(InterfaceAppDomainSetup),
			InterfaceAppDomainSetupTypeLoadFromMainModule = true,
		};

		readonly Sensor sensor = new Sensor();

		readonly object LicenseClientLock = new object();

		LicenseClient LicenseClient => SensorServerDispatcher?.LicenseClient;

		InterfaceAppDomainSetup TriggerSetup = new InterfaceAppDomainSetup();

		public UI.InterfaceToEve InterfaceToEveControl => Window?.Main?.Interface;

		public int? EveOnlineClientProcessId =>
			InterfaceToEveControl?.ProcessChoice?.ChoosenProcessId;

		FromProcessMeasurement<MemoryMeasurementEvaluation> MemoryMeasurementLast;

		Int64? MemoryMeasurementLastAge => GetTimeStopwatch() - MemoryMeasurementLast?.Begin;

		public Int64? FromScriptMeasurementInvalidationTime = null;

		readonly Bib3.RateLimit.IRateLimitStateInt MemoryMeasurementRequestRateLimit = new Bib3.RateLimit.RateLimitStateIntSingle();

		int MotionInvalidateMeasurementDelay = 400;

		Int64? FromMotionExecutionMemoryMeasurementTimeMin =>
			MotorLock.BranchOnTryEnter(() =>
			{
				var motionLastTime = MotionLastTime;

				if (!motionLastTime.HasValue)
					return null;

				return motionLastTime.Value + MotionInvalidateMeasurementDelay;

			},
			() => (Int64?)Int64.MaxValue);

		Int64? MeasurementRecentEnoughTime => new[]
			{
				FromMotionExecutionMemoryMeasurementTimeMin,
				FromScriptMeasurementInvalidationTime,
			}.Max();

		Int64? RequestedMeasurementTime
		{
			get
			{
				var MeasurementRecentEnoughTime = this.MeasurementRecentEnoughTime;
				var MemoryMeasurementLast = this.MemoryMeasurementLast;

				if (MemoryMeasurementLast?.Begin < MeasurementRecentEnoughTime)
					return MeasurementRecentEnoughTime;

				return (MemoryMeasurementLast?.End + 4000) ?? 0;
			}
		}

		FromProcessMeasurement<MemoryMeasurementEvaluation> MemoryMeasurementIfRecentEnough
		{
			get
			{
				var MemoryMeasurementLast = this.MemoryMeasurementLast;
				var MeasurementRecentEnoughTime = this.MeasurementRecentEnoughTime;

				if (MemoryMeasurementLast?.Begin < MeasurementRecentEnoughTime)
					return null;

				return MemoryMeasurementLast;
			}
		}

		FromProcessMeasurement<MemoryMeasurementEvaluation> FromScriptRequestMemoryMeasurementEvaluation()
		{
			var BeginTime = GetTimeStopwatch();

			while (true)
			{
				var MemoryMeasurementIfRecentEnough = this.MemoryMeasurementIfRecentEnough;

				if (null != MemoryMeasurementIfRecentEnough)
					return MemoryMeasurementIfRecentEnough;

				var RequestAge = GetTimeStopwatch() - BeginTime;

				if (Sanderling.Script.Impl.HostToScript.FromScriptRequestMemoryMeasurementDelayMax < RequestAge)
					return null;    //	Timeout

				Thread.Sleep(44);
			}
		}

		void FromScriptInvalidateMeasurement(int delayToMeasurementMilli)
		{
			FromScriptMeasurementInvalidationTime =
				Math.Max(FromScriptMeasurementInvalidationTime ?? int.MinValue, GetTimeStopwatch() + Math.Min(delayToMeasurementMilli, 10000));
		}

		void InterfaceExchange()
		{
			LicenseClientExchange();

			var EveOnlineClientProcessId = this.EveOnlineClientProcessId;

			var RequestedMeasurementTime = this.RequestedMeasurementTime ?? 0;

			if (EveOnlineClientProcessId.HasValue && RequestedMeasurementTime <= GetTimeStopwatch())
				if (MemoryMeasurementRequestRateLimit.AttemptPass(GetTimeStopwatch(), 700))
					Task.Run(() => MeasurementMemoryTake(EveOnlineClientProcessId.Value, RequestedMeasurementTime));
		}

		void LicenseClientExchange()
		{
			var licenseClientConfig = (ConfigDefaultConstruct()?.LicenseClient).CompletedWithDefault().WithRequestLicenseKey(ExeConfig.ConfigLicenseKeyDefault);

			Task.Run(() => SensorServerDispatcher?.Exchange(licenseClientConfig, SensorServerDispatcher.AppInterfaceAvailable ? 1000 : (int?)null));
		}

		void MeasurementMemoryTake(int processId, Int64 measurementBeginTimeMinMilli)
		{
			var MeasurementRaw = sensor.MeasurementTake(processId, measurementBeginTimeMinMilli);

			if (null == MeasurementRaw)
				return;

			MemoryMeasurementLast = MeasurementRaw?.MapValue(value => new Interface.MemoryMeasurementEvaluation(
				MeasurementRaw,
				MemoryMeasurementLast?.Value?.MemoryMeasurementAccumulation as Accumulator.MemoryMeasurementAccumulator));
		}
	}
}
