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
		FromProcessMeasurement<Accumulation.IMemoryMeasurement> MemoryMeasurementAccumulation
		{
			get;
		}

		MotionResult MotionExecute(MotionParam MotionParam);
	}

	public class HostToScript : IHostToScript
	{
		public const int FromScriptRequestMemoryMeasurementDelayMax = 3000;

		public Func<FromProcessMeasurement<Interface.MemoryMeasurementEvaluation>> MemoryMeasurementFunc;

		public Func<MotionParam, MotionResult> MotionExecuteFunc;

		public FromProcessMeasurement<MemoryStruct.IMemoryMeasurement> MemoryMeasurement =>
			MemoryMeasurementFunc?.Invoke()?.MapValue(Evaluation => Evaluation?.MemoryMeasurement);

		public FromProcessMeasurement<Parse.IMemoryMeasurement> MemoryMeasurementParsed =>
			MemoryMeasurementFunc?.Invoke()?.MapValue(Evaluation => Evaluation?.MemoryMeasurementParsed);

		public FromProcessMeasurement<Accumulation.IMemoryMeasurement> MemoryMeasurementAccumulation =>
			MemoryMeasurementFunc?.Invoke()?.MapValue(Evaluation => Evaluation?.MemoryMeasurementAccumulation);

		public MotionResult MotionExecute(MotionParam MotionParam) => MotionExecuteFunc?.Invoke(MotionParam);
	}

	public class ToScriptGlobals : BotScript.ScriptRun.ToScriptGlobals
	{
		public IHostToScript HostSanderling;
	}
}
