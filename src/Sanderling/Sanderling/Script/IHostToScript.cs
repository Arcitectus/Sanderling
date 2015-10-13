using BotEngine;
using BotEngine.Interface;
using BotEngine.Motor;
using Sanderling.Motor;
using System;
using System.Collections.Generic;
using System.Linq;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Script
{
	static public class Script
	{
		static readonly Type[] AssemblyAndNamespaceAddition = new[]
		{
			typeof(Bib3.Extension),
			typeof(ObjectIdInt64),
			typeof(FromProcessMeasurement<>),
			typeof(MemoryStruct.MemoryMeasurement),
			typeof(MotionParam),
			typeof(MouseButtonIdEnum),
			typeof(BotEngine.Common.Extension),
			typeof(Sanderling.Extension),
			typeof(Extension),
		};

		static public IEnumerable<System.Reflection.Assembly> AssemblyAddition =>
			AssemblyAndNamespaceAddition?.Select(t => t.Assembly);

		static public IEnumerable<string> NamespaceAddition =>
			AssemblyAndNamespaceAddition?.Select(t => t.Namespace);

	}
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
		public const int FromScriptRequestMemoryMeasurementDelayMax = 3000;

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
