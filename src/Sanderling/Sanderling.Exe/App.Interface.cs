using Bib3;
using BotEngine.Interface;
using BotEngine.UI;
using Sanderling.Interface;
using Sanderling.Script;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

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
		SimpleSensorServerDispatcher SensorServerDispatcher;

		readonly object LicenseClientLock = new object();

		PropertyGenTimespanInt64<LicenseClient> LicenseClient;

		Int64 LicenseClientExchangeStartedLastTime;

		int LicenseClientExchangeDistanceMin = 1000;

		InterfaceAppDomainSetup TriggerSetup = new InterfaceAppDomainSetup();

		InterfaceAppManager SensorAppManager = new InterfaceAppManager(typeof(InterfaceAppDomainSetup), true);

		public UI.InterfaceToEve InterfaceToEveControl => Window?.Main?.Interface;

		BotEngine.UI.LicenseClientConfig LicenseClientConfigControl => InterfaceToEveControl?.LicenseClientConfig;

		BotEngine.UI.LicenseClientInspect LicenseClientInspect => InterfaceToEveControl?.LicenseClientInspect;

		public int? EveOnlineClientProcessId =>
			InterfaceToEveControl?.ProcessChoice?.ChoosenProcessId;

		FromProcessMeasurement<MemoryMeasurementEvaluation> MemoryMeasurementLast;

		Int64? MemoryMeasurementLastAge => GetTimeStopwatch() - MemoryMeasurementLast?.Begin;

		public Int64? FromScriptMeasurementInvalidationTime = null;

		Int64? FromMotionExecutionMemoryMeasurementTimeMin
		{
			get
			{
				var MotionExecutionLast = MotionExecution?.LastOrDefault();

				if (null == MotionExecutionLast)
				{
					return null;
				}

				return (MotionExecutionLast?.End + 300) ?? Int64.MaxValue;
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

				return MemoryMeasurementLast?.End + 4000;
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

					if (HostToScript.FromScriptRequestMemoryMeasurementDelayMax < RequestAge)
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

		void LicenseClientExchange()
		{
			lock (LicenseClientLock)
			{
				var Time = Bib3.Glob.StopwatchZaitMiliSictInt();

				var LicenseClientExchangeStartedLastAge = Time - LicenseClientExchangeStartedLastTime;

				if (LicenseClientExchangeStartedLastAge < LicenseClientExchangeDistanceMin)
				{
					return;
				}

				if (null == LicenseClient)
				{
					LicenseClient = new PropertyGenTimespanInt64<LicenseClient>(new LicenseClient(), Time, Time);

					SensorServerDispatcher = new SimpleSensorServerDispatcher()
					{
						LicenseClient = LicenseClient.Value,
						SensorAppManager = SensorAppManager,
					};
				}

				LicenseClient.Value.ServerAddress = LicenseClientConfigControl?.ApiVersionAddress();

				var EveOnlineClientProcessId = this.EveOnlineClientProcessId;

				LicenseClientExchangeStartedLastTime = Time;

				Task.Run(() =>
				{
					var LicenseClient = this.LicenseClient;

					if (null == LicenseClient?.Value)
					{
						return;
					}

					LicenseClient.Value.Timeout = 4000;

					SensorServerDispatcher.Exchange(
						EveOnlineClientProcessId,
						RequestedMeasurementTime ?? Int64.MaxValue,
						CallbackMeasurementMemoryNew);
				});
			}
		}

		void CallbackMeasurementMemoryNew(FromProcessMeasurement<MemoryStruct.IMemoryMeasurement> Measurement)
		{
			MemoryMeasurementLast = Measurement.MapValue(Value => new Interface.MemoryMeasurementEvaluation(
				Measurement,
				MemoryMeasurementLast?.Value?.MemoryMeasurementAccumulation as Accumulator.MemoryMeasurementAccumulator));
		}
	}
}
