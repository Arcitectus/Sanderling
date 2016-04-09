using Bib3;
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

		Int64? FromMotionExecutionMemoryMeasurementTimeMin
		{
			get
			{
				lock (MotorLock)
				{
					var motionLastTime = MotionLastTime;

					if (!motionLastTime.HasValue)
						return null;

					return motionLastTime.Value + MotionInvalidateMeasurementDelay;
				}
			}
		}

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
				lock (MotorLock)
				{
					var MemoryMeasurementLast = this.MemoryMeasurementLast;
					var MeasurementRecentEnoughTime = this.MeasurementRecentEnoughTime;

					if (MemoryMeasurementLast?.Begin < MeasurementRecentEnoughTime)
						return null;

					return MemoryMeasurementLast;
				}
			}
		}

		FromProcessMeasurement<MemoryMeasurementEvaluation> FromScriptRequestMemoryMeasurementEvaluation()
		{
			lock (MotorLock)
			{
				var BeginTime = GetTimeStopwatch();

				while (true)
				{
					var MemoryMeasurementIfRecentEnough = this.MemoryMeasurementIfRecentEnough;

					if (null != MemoryMeasurementIfRecentEnough)
					{
						return MemoryMeasurementIfRecentEnough;
					}

					var RequestAge = GetTimeStopwatch() - BeginTime;

					if (Sanderling.Script.Impl.HostToScript.FromScriptRequestMemoryMeasurementDelayMax < RequestAge)
					{
						//	Timeout
						return null;
					}

					Thread.Sleep(44);
				}
			}
		}

		void FromScriptInvalidateMeasurement(int DelayToMeasurementMilli)
		{
			FromScriptMeasurementInvalidationTime =
				Math.Max(FromScriptMeasurementInvalidationTime ?? int.MinValue, GetTimeStopwatch() + Math.Min(DelayToMeasurementMilli, 10000));
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
			var licenseClientConfig = ConfigReadFromUI()?.LicenseClient.CompletedWithDefault();

			Task.Run(() => SensorServerDispatcher?.Exchange(licenseClientConfig, SensorServerDispatcher.AppInterfaceAvailable ? 1000 : (int?)null));
		}

		void MeasurementMemoryTake(int processId, Int64 measurementBeginTimeMinMilli)
		{
			var MeasurementRaw = SensorServerDispatcher.InterfaceAppManager.MeasurementTake(processId, measurementBeginTimeMinMilli);

			if (null == MeasurementRaw)
				return;

			MemoryMeasurementLast = MeasurementRaw?.MapValue(Value => new Interface.MemoryMeasurementEvaluation(
				MeasurementRaw,
				MemoryMeasurementLast?.Value?.MemoryMeasurementAccumulation as Accumulator.MemoryMeasurementAccumulator));
		}
	}
}
