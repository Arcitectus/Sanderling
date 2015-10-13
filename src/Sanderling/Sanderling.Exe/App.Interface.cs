using Bib3;
using BotEngine.Interface;
using BotEngine.UI;
using Sanderling.Script;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Exe
{
	partial class App
	{
		SimpleSensorServerDispatcher SensorServerDispatcher;

		readonly object LicenseClientLock = new object();

		WertZuZaitraum<LicenseClient> LicenseClient;

		Int64 LicenseClientExchangeStartedLastTime;

		int LicenseClientExchangeDistanceMin = 1000;

		InterfaceAppManager SensorAppManager = new InterfaceAppManager();

		public UI.InterfaceToEve InterfaceToEveControl => Window?.Main?.Interface;

		BotEngine.UI.LicenseClientConfig LicenseClientConfigControl => InterfaceToEveControl?.LicenseClientConfig;

		BotEngine.UI.LicenseClientInspect LicenseClientInspect => InterfaceToEveControl?.LicenseClientInspect;

		public int? EveOnlineClientProcessId =>
			InterfaceToEveControl?.ProcessChoice?.ChoosenProcessId;

		FromProcessMeasurement<MemoryStruct.MemoryMeasurement> MemoryMeasurementLast;

		Int64? FromMotionExecutionMemoryMeasurementTimeMin
		{
			get
			{
				var MotionExecutionLast = MotionExecution?.LastOrDefault();

				if (null == MotionExecutionLast)
				{
					return null;
				}

				return (MotionExecutionLast?.EndeZait + 300) ?? Int64.MaxValue;
			}
		}

		FromProcessMeasurement<MemoryStruct.MemoryMeasurement> MemoryMeasurementIfRecentEnough
		{
			get
			{
				lock (MotorLock)
				{
					var FromMotionExecutionMemoryMeasurementTimeMin = this.FromMotionExecutionMemoryMeasurementTimeMin;
					var MemoryMeasurementLast = this.MemoryMeasurementLast;

					if (!FromMotionExecutionMemoryMeasurementTimeMin.HasValue)
					{
						return MemoryMeasurementLast;
					}

					if ((FromMotionExecutionMemoryMeasurementTimeMin <= MemoryMeasurementLast?.Begin))
					{
						return MemoryMeasurementLast;
					}

					return null;
				}
			}
		}

		FromProcessMeasurement<MemoryStruct.MemoryMeasurement> FromScriptRequestMemoryMeasurement()
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

		Int64? RequestedMeasurementTime
		{
			get
			{
				var MemoryMeasurementLast = this.MemoryMeasurementLast;

				if (null == MemoryMeasurementLast)
				{
					return GetTimeStopwatch();
				}

				return
					FromMotionExecutionMemoryMeasurementTimeMin ??
					(MemoryMeasurementLast?.EndeZait + 4000);
			}
		}

		void LicenseClientExchange()
		{
			lock (LicenseClientLock)
			{
				InterfaceAppDomainSetup.Setup();

				var Time = Bib3.Glob.StopwatchZaitMiliSictInt();

				var LicenseClientExchangeStartedLastAge = Time - LicenseClientExchangeStartedLastTime;

				if (LicenseClientExchangeStartedLastAge < LicenseClientExchangeDistanceMin)
				{
					return;
				}

				if (null == LicenseClient)
				{
					LicenseClient = new WertZuZaitraum<LicenseClient>(new LicenseClient(), Time);

					SensorServerDispatcher = new SimpleSensorServerDispatcher()
					{
						LicenseClient = LicenseClient.Wert,
						SensorAppManager = SensorAppManager,
					};
				}

				LicenseClient.Wert.ServerAddress = LicenseClientConfigControl?.ApiVersionAddress();

				var EveOnlineClientProcessId = this.EveOnlineClientProcessId;

				LicenseClientExchangeStartedLastTime = Time;

				Task.Run(() =>
				{
					var LicenseClient = this.LicenseClient;

					if (null == LicenseClient?.Wert)
					{
						return;
					}

					LicenseClient.Wert.Timeout = 1000;

					SensorServerDispatcher.Exchange(
						EveOnlineClientProcessId,
						RequestedMeasurementTime ?? Int64.MaxValue,
						CallbackMeasurementMemoryNew);
				});
			}
		}

		void CallbackMeasurementMemoryNew(FromProcessMeasurement<MemoryStruct.MemoryMeasurement> Measurement)
		{
			MemoryMeasurementLast = Measurement;
		}
	}
}
