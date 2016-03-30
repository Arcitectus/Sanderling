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
			MemoryMeasurementFunc?.Invoke()?.MapValue(Evaluation => Evaluation?.MemoryMeasurement);

		public FromProcessMeasurement<Parse.IMemoryMeasurement> MemoryMeasurementParsed =>
			MemoryMeasurementFunc?.Invoke()?.MapValue(Evaluation => Evaluation?.MemoryMeasurementParsed);

		public FromProcessMeasurement<Accumulation.IMemoryMeasurement> MemoryMeasurementAccu =>
			MemoryMeasurementFunc?.Invoke()?.MapValue(Evaluation => Evaluation?.MemoryMeasurementAccumulation);

		public MotionResult MotionExecute(MotionParam MotionParam) => MotionExecuteFunc?.Invoke(MotionParam);

		public void InvalidateMeasurement(Int32 DelayToMeasurementMilli) => InvalidateMeasurementAction?.Invoke(DelayToMeasurementMilli);

		public IntPtr WindowHandle => WindowHandleFunc?.Invoke() ?? IntPtr.Zero;
	}
}
