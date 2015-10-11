using BotEngine.Interface;
using BotEngine.Motor;
using Sanderling.Motor;
using System;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Script
{
	public interface IHostToScript
	{
		FromProcessMeasurement<MemoryStruct.MemoryMeasurement> MemoryMeasurement
		{
			get;
		}

		MotionResult MotionExecute(MotionParam MotionParam);
	}

	public class HostToScript : IHostToScript
	{
		public Func<FromProcessMeasurement<MemoryStruct.MemoryMeasurement>> MemoryMeasurementFunc;

		public Func<MotionParam, MotionResult> MotionExecuteFunc;

		FromProcessMeasurement<MemoryStruct.MemoryMeasurement> IHostToScript.MemoryMeasurement => MemoryMeasurementFunc?.Invoke();

		public MotionResult MotionExecute(MotionParam MotionParam) => MotionExecuteFunc?.Invoke(MotionParam);
	}

	public class ToScriptGlobals : BotScript.ScriptRun.ToScriptGlobals
	{
		public IHostToScript HostSanderling;
	}
}
