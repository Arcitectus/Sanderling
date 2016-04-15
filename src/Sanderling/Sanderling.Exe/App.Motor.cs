using BotEngine.Motor;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BotEngine.Common;
using Sanderling.Motor;
using Bib3;

namespace Sanderling.Exe
{
	public class MotionExecution : PropertyGenTimespanInt64<MotionParam>
	{
		public MotionExecution(
			MotionParam param,
			Int64 beginTime,
			Int64 endTime)
			:
			base(param, beginTime, endTime)
		{
		}

		public MotionResult Result;
	}

	partial class App
	{
		readonly object MotorLock = new object();

		Int64? MotionLastTime => MotionExecution?.LastOrDefault()?.End ?? MotionExecution?.LastOrDefault()?.Begin;

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

		Task<MotionResult> ActMotionAsync(MotionParam motion)
		{
			return Task.Run(() =>
		  {
			  lock (MotorLock)
			  {
				  if (null == motion)
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

				  var Result = Motor.ActSequenceMotion(motion.AsSequenceMotion(MemoryMeasurementLast?.Value?.MemoryMeasurement));

				  var EndTime = GetTimeStopwatch();

				  MotionExecution.Enqueue(new MotionExecution(motion, BeginTime, EndTime)
				  {
					  Result = Result,
				  });

				  MotionExecution.TrimHeadToKeep(100);

				  return Result;
			  }
		  });
		}

		MotionResult FromScriptMotionExecute(MotionParam motionParam) => ActMotionAsync(motionParam).Result;
	}
}
