using Bib3;
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
		static readonly Type[] AssemblyAndNamespaceAdditionType = new[]
		{
			typeof(Vektor2DInt),
			typeof(ObjectIdInt64),
			typeof(FromProcessMeasurement<>),
			typeof(MemoryStruct.IMemoryMeasurement),
			typeof(MotionParam),
			typeof(MouseButtonIdEnum),
			typeof(BotEngine.Common.Extension),
			typeof(Sanderling.Extension),
			typeof(Extension),
		};

		static readonly Type[] NamespaceStaticAdditionType = new[]
		{
			typeof(Bib3.Extension),
		};

		static	IEnumerable<Type> AssemblyAdditionType => new[]
		{
			AssemblyAndNamespaceAdditionType,
			NamespaceStaticAdditionType,
		}.ConcatNullable();

		static public IEnumerable<System.Reflection.Assembly> AssemblyAddition =>
			AssemblyAdditionType?.Select(t => t.Assembly);

		static public IEnumerable<string> NamespaceAddition =>
			new[]
			{
				AssemblyAndNamespaceAdditionType?.Select(t => t.Namespace),
				NamespaceStaticAdditionType?.Select(t => t.FullName),
			}.ConcatNullable();

	}
	public interface IHostToScript
	{
		FromProcessMeasurement<MemoryStruct.IMemoryMeasurement> MemoryMeasurement
		{
			get;
		}

		MotionResult MotionExecute(MotionParam MotionParam);
	}

	public class HostToScript : IHostToScript
	{
		public const int FromScriptRequestMemoryMeasurementDelayMax = 3000;

		public Func<FromProcessMeasurement<MemoryStruct.IMemoryMeasurement>> MemoryMeasurementFunc;

		public Func<MotionParam, MotionResult> MotionExecuteFunc;

		FromProcessMeasurement<MemoryStruct.IMemoryMeasurement> IHostToScript.MemoryMeasurement => MemoryMeasurementFunc?.Invoke();

		public MotionResult MotionExecute(MotionParam MotionParam) => MotionExecuteFunc?.Invoke(MotionParam);
	}

	public class ToScriptGlobals : BotScript.ScriptRun.ToScriptGlobals
	{
		public IHostToScript HostSanderling;
	}
}
