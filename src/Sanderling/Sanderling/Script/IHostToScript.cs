using BotEngine.Interface;
using BotEngine.Motor;
using Sanderling.Motor;
using System;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Script
{
	public interface IHostToScript
	{
		/// <summary>
		/// Raw data obtained from memory reading.
		/// </summary>
		FromProcessMeasurement<MemoryStruct.IMemoryMeasurement> MemoryMeasurement
		{
			get;
		}

		/// <summary>
		/// Parsing/mapping might depend on localization specific symbols in the UI.
		/// In case of problems with parsing, make sure the language in eve online is set to english.
		/// </summary>
		FromProcessMeasurement<Parse.IMemoryMeasurement> MemoryMeasurementParsed
		{
			get;
		}

		/// <summary>
		/// Data collected and connected from multiple measurements.
		/// </summary>
		FromProcessMeasurement<Accumulation.IMemoryMeasurement> MemoryMeasurementAccu
		{
			get;
		}

		MotionResult MotionExecute(MotionParam MotionParam);

		/// <summary>
		/// Adds a lower bound to time of measurement to be returned on next call to a measurement property.
		/// </summary>
		/// <param name="DelayToMeasurementMilli"></param>
		void InvalidateMeasurement(int DelayToMeasurementMilli);

		IntPtr WindowHandle { get; }
	}

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

	public class ToScriptGlobals : BotSharp.ScriptRun.ToScriptGlobals
	{
		public IHostToScript Sanderling;

		public IHostToScript HostSanderling => Sanderling;
	}
}
