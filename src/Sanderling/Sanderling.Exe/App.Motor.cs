using BotEngine.Motor;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BotEngine.Common;
using Sanderling.Motor;

namespace Sanderling.Exe
{
	public class MotionExecution : Bib3.WertZuZaitraum<MotionParam>
	{
		public MotionExecution(
			MotionParam Param,
			Int64 BeginTime,
			Int64 EndTime)
			:
			base(Param, BeginTime, EndTime)
		{
		}

		public MotionResult Result;
	}

	partial class App
	{
		readonly object MotorLock = new object();

		Int64? MotionLastTime => MotionExecution?.LastOrDefault()?.EndeZait ?? MotionExecution?.LastOrDefault()?.BeginZait;

		readonly Queue<MotionExecution> MotionExecution = new Queue<MotionExecution>();

		IMotor Motor;

		IMotor GetMotor()
		{
			var EveOnlineClientProcessId = this.EveOnlineClientProcessId;

			if (!EveOnlineClientProcessId.HasValue)
			{
				return null;
			}

			try
			{
				var Process = System.Diagnostics.Process.GetProcessById(EveOnlineClientProcessId.Value);

				if (null == Process)
				{
					return null;
				}

				return new Motor.WindowMotor(Process.MainWindowHandle);
			}
			catch (ArgumentException)
			{
				//	GetProcessById throws when Process does not exist.
				return null;
			}
		}

		Task<MotionResult> ActMotionAsync(MotionParam Motion)
		{
			return Task.Run(() =>
		  {
			  lock (MotorLock)
			  {
				  if (null == Motion)
				  {
					  return null;
				  }

				  var Motor = this.Motor;

				  if (null == Motor)
				  {
					  return null;
				  }

				  var MotorAsWindowMotor = Motor as Motor.WindowMotor;

				  if (null != MotorAsWindowMotor)
				  {
					  MotorAsWindowMotor.MouseMoveDelay = 140;
					  MotorAsWindowMotor.MouseEventDelay = 140;
				  }

				  var BeginTime = GetTimeStopwatch();

				  var Result = Motor.ActSequenceMotion(Motion.AsSequenceMotion(MemoryMeasurementLast));

				  var EndTime = GetTimeStopwatch();

				  MotionExecution.Enqueue(new MotionExecution(Motion, BeginTime, EndTime)
				  {
					  Result = Result,
				  });

				  MotionExecution.TrimHeadToKeep(100);

				  return Result;
			  }
		  });
		}

		MotionResult FromScriptMotionExecute(MotionParam MotionParam) => ActMotionAsync(MotionParam).Result;
	}
}
