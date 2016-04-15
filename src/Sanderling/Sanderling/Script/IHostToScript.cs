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

		MotionResult MotionExecute(MotionParam motionParam);

		/// <summary>
		/// Adds a lower bound to time of measurement to be returned on next call to a measurement property.
		/// </summary>
		/// <param name="delayToMeasurementMilli"></param>
		void InvalidateMeasurement(int delayToMeasurementMilli);

		IntPtr WindowHandle { get; }
	}

	public class ToScriptGlobals : BotSharp.ScriptRun.ScriptRun.ToScriptGlobals
	{
		public IHostToScript Sanderling;

		public IHostToScript HostSanderling => Sanderling;
	}
}
