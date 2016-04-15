using BotEngine.Interface;
using BotEngine.Motor;
using Sanderling.Motor;
using System;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Script.Impl
{
	public class HostToScript : IHostToScript
	{
		public const int FromScriptRequestMemoryMeasurementDelayMax = 4000;

		public Func<FromProcessMeasurement<Interface.MemoryMeasurementEvaluation>> MemoryMeasurementFunc;

		public Func<MotionParam, MotionResult> MotionExecuteFunc;

		public Action<int> InvalidateMeasurementAction;

		public Func<IntPtr> WindowHandleFunc;

		public FromProcessMeasurement<MemoryStruct.IMemoryMeasurement> MemoryMeasurement =>
			MemoryMeasurementFunc?.Invoke()?.MapValue(evaluation => evaluation?.MemoryMeasurement);

		public FromProcessMeasurement<Parse.IMemoryMeasurement> MemoryMeasurementParsed =>
			MemoryMeasurementFunc?.Invoke()?.MapValue(evaluation => evaluation?.MemoryMeasurementParsed);

		public FromProcessMeasurement<Accumulation.IMemoryMeasurement> MemoryMeasurementAccu =>
			MemoryMeasurementFunc?.Invoke()?.MapValue(evaluation => evaluation?.MemoryMeasurementAccumulation);

		public MotionResult MotionExecute(MotionParam motionParam) => MotionExecuteFunc?.Invoke(motionParam);

		public void InvalidateMeasurement(Int32 delayToMeasurementMilli) => InvalidateMeasurementAction?.Invoke(delayToMeasurementMilli);

		public IntPtr WindowHandle => WindowHandleFunc?.Invoke() ?? IntPtr.Zero;
	}
}
