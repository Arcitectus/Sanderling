using Bib3;
using BotEngine.Interface;
using BotEngine.UI;
using System;
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

		FromProcessMeasurement<MemoryStruct.MemoryMeasurement> FromScriptRequestMeasurementLast()
		{
			lock (MotorLock)
			{
				return MemoryMeasurementLast;
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

					var RequestedMeasurementTime = Bib3.Glob.StopwatchZaitMiliSictInt();

					SensorServerDispatcher.Exchange(
						EveOnlineClientProcessId,
						RequestedMeasurementTime,
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
