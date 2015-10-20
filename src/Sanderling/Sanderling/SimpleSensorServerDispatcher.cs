using BotEngine.Common;
using Sanderling.Interface;
using System;
using System.Linq;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling
{
	/// <summary>
	/// dispatches messages between sensor and server.
	/// </summary>
	public class SimpleSensorServerDispatcher
	{
		readonly object Lock = new object();

		public BotEngine.Interface.InterfaceAppManager SensorAppManager;

		public BotEngine.Interface.LicenseClient LicenseClient;

		BotEngine.Interface.FromProcessMeasurement<MemoryStruct.IMemoryMeasurement> MeasurementMemoryReceivedLast = null;

		const int ServerExchangeTimeDistanceMin = 1000;

		Int64 ServerExchangeLastTime;

		public void Exchange(
			int? EveOnlineClientProcessId,
			Int64 RequestedMeasurementTime,
			Action<BotEngine.Interface.FromProcessMeasurement<MemoryStruct.IMemoryMeasurement>> CallbackMeasurementMemoryNew)
		{
			BotEngine.Interface.FromProcessMeasurement<MemoryStruct.IMemoryMeasurement> MeasurementMemoryNew = null;

			lock (Lock)
			{
				var Time = Bib3.Glob.StopwatchZaitMiliSictInt();

				var ServerExchangeLastAge = Time - ServerExchangeLastTime;

				if (ServerExchangeTimeDistanceMin <= ServerExchangeLastAge)
				{
					if (LicenseClient?.AuthCompleted ?? false)
					{
						ServerExchangeLastTime = Time;

						var LicenseServerSessionId = LicenseClient?.ExchangeAuthLast?.Value?.Response?.SessionId;

						var ToServerMessage = new BotEngine.Interface.FromClientToServerMessage()
						{
							SessionId = LicenseServerSessionId,
							Interface = SensorAppManager?.ToServer(),
							Time = Bib3.Glob.StopwatchZaitMiliSictInt(),
						};

						var FromServerMessage = LicenseClient?.ExchangePayload(ToServerMessage);

						if (null != FromServerMessage)
						{
							SensorAppManager.FromServer(FromServerMessage.Interface);
						}
					}
					else
					{
						LicenseClient?.ExchangeAuth();
					}
				}

				var MeasurementMemoryReceivedLastTime = MeasurementMemoryReceivedLast?.End;

				var ToSensorMessage = new FromConsumerToSensorMessage()
				{
					RequestedMeasurementProcessId = EveOnlineClientProcessId,
					MeasurementMemoryRequestTime = RequestedMeasurementTime,
					MeasurementMemoryReceivedLastTime = MeasurementMemoryReceivedLastTime,
				};

				var FromSensorAppMessage = SensorAppManager?.ConsumerExchange(new BotEngine.Interface.FromConsumerToInterfaceProxyMessage()
				{
					AppSpecific = ToSensorMessage.SerializeSingleBib3RefNezDifProtobuf(),
				});

				if (null == FromSensorAppMessage)
				{
					return;
				}

				var FromSensorAppMessagePortionAppSpecific = FromSensorAppMessage.AppSpecific;

				var SensorMessageEveOnline =
					FromSensorAppMessagePortionAppSpecific.DeSerializeProtobufBib3RefNezDif()?.FirstOrDefault() as FromSensorToConsumerMessage;

				var MeasurementMemory = SensorMessageEveOnline?.MemoryMeasurement;

				if (null == MeasurementMemory)
				{
					return;
				}

				if (MeasurementMemory?.End == MeasurementMemoryReceivedLast?.End)
				{
					return;
				}

				MeasurementMemoryReceivedLast = MeasurementMemory;

				MeasurementMemoryNew = MeasurementMemory;
			}

			if (null != MeasurementMemoryNew)
			{
				CallbackMeasurementMemoryNew?.Invoke(MeasurementMemoryNew);
			}
		}
	}
}
