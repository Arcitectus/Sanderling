using BotEngine.Interface;
using Sanderling.Interface;
using System;
using System.Threading;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling
{
	static public class InterfaceAppManagerExtension
	{
		static public FromInterfaceResponse ClientRequest(
			this InterfaceAppManager interfaceAppManager,
			ToInterfaceRequest request) =>
			FromInterfaceResponse.DeserializeFromString<FromInterfaceResponse>(
				interfaceAppManager?.ClientRequest(FromInterfaceResponse.SerializeToString(request)));

		static public bool? MeasurementInProgress(this Sensor interfaceAppManager) =>
			interfaceAppManager?.ClientRequest(new ToInterfaceRequest())?.MemoryMeasurementInProgress;

		static public FromInterfaceResponse MeasurementTakeRequest(
			this Sensor interfaceAppManager,
			int processId,
			Int64 measurementBeginTimeMinMilli)
		{
			while (interfaceAppManager?.MeasurementInProgress() ?? false)
				Thread.Sleep(11);

			var processIdLast = interfaceAppManager?.ClientRequest(new ToInterfaceRequest
			{
				MemoryMeasurementInitGetLast = true,
			})
			?.MemoryMeasurementInit?.ProcessId;

			var MemoryMeasurementInitReuse = processIdLast == processId;

			var Response = interfaceAppManager?.ClientRequest(new ToInterfaceRequest
			{
				MemoryMeasurementInitTake = MemoryMeasurementInitReuse ? null : new MemoryMeasurementInitParam
				{
					ProcessId = processId,
				},
				MemoryMeasurementTake = !MemoryMeasurementInitReuse,
				MemoryMeasurementGetLast = true,
			});

			if (!(measurementBeginTimeMinMilli <= Response?.MemoryMeasurement?.Begin))
				Response = interfaceAppManager?.ClientRequest(new ToInterfaceRequest
				{
					MemoryMeasurementTake = true,
				});

			return Response;
		}

		static public FromProcessMeasurement<MemoryStruct.IMemoryMeasurement> MeasurementTake(
			this Sensor interfaceAppManager,
			int processId,
			Int64 measurementBeginTimeMinMilli) =>
			MeasurementTakeRequest(interfaceAppManager, processId, measurementBeginTimeMinMilli)?.MemoryMeasurement;

		static public FromInterfaceResponse MeasurementTakeNewRequest(
			this Sensor interfaceAppManager,
			int processId) =>
			MeasurementTakeRequest(interfaceAppManager, processId, Bib3.Glob.StopwatchZaitMiliSictInt());
	}
}
