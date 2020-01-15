using BotEngine.Interface;
using BotEngine.Motor;
using BotSharp.ScriptRun;
using Sanderling.Interface;
using Sanderling.Motor;
using System;
using System.Threading;

namespace Sanderling.Script.Impl
{
	public class ScriptRunClient : IScriptRunClient
	{
		static public Int64 GetTimeStopwatch() => Bib3.Glob.StopwatchZaitMiliSictInt();

		public Func<FromProcessMeasurement<MemoryMeasurementEvaluation>> MemoryMeasurementLastDelegate;

		public Func<FromProcessMeasurement<MemoryMeasurementEvaluation>> FromScriptRequestMemoryMeasurementEvaluation;

		public Action FromScriptExecutionControlCheck { set; get; }

		public Action<int> InvalidateMeasurementAction;

		public Action GetKillEveProcessAction;

		public Func<IntPtr> GetWindowHandleDelegate;

		public Func<MotionParam, MotionResult> FromScriptMotionExecute;

		public Action<ScriptRunClient, ScriptRun> ExecutionStatusChangedDelegate;

		public ScriptRun.ToScriptGlobals ToScriptGlobals =>
			new ToScriptGlobals()
			{
				Sanderling = new HostToScript()
				{
					MemoryMeasurementFunc = () =>
					{
						FromScriptExecutionControlCheck?.Invoke();
						return FromScriptRequestMemoryMeasurementEvaluation();
					},

					MotionExecuteFunc = motionParam =>
					{
						FromScriptExecutionControlCheck?.Invoke();
						return FromScriptMotionExecute?.Invoke(motionParam);
					},

					InvalidateMeasurementAction = InvalidateMeasurementAction,

					WindowHandleFunc = () => GetWindowHandleDelegate?.Invoke() ?? IntPtr.Zero,

					KillEveProcessAction = () => GetKillEveProcessAction?.Invoke(),
				}
			};


		public void ExecutionStatusChanged(ScriptRun run)
		{
			if (null == run)
				return;

			if (ScriptRunExecutionStatus.Failed == run.Status)
				System.Media.SystemSounds.Beep.Play();

			ExecutionStatusChangedDelegate(this, run);
		}

		public void RunThreadEnterBefore(ScriptRun run)
		{
			//	make sure script runs on same culture independend of host culture.
			Thread.CurrentThread.CurrentCulture = Parse.Culture.ParseCulture;
		}
	}
}
