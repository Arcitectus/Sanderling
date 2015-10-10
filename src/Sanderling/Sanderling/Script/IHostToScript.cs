using System;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Script
{
	public interface IHostToScript
	{
		MemoryStruct.MemoryMeasurement LastMemoryMeasurement
		{
			get;
		}

		void ClickUIElement(MemoryStruct.UIElement Target, bool RightButton);

    }

	public class HostToScript : IHostToScript
	{
		public Func<MemoryStruct.MemoryMeasurement> LastMemoryMeasurementFunc;

		MemoryStruct.MemoryMeasurement IHostToScript.LastMemoryMeasurement => LastMemoryMeasurementFunc?.Invoke();

		public void ClickUIElement(MemoryStruct.UIElement Target, bool RightButton)
		{
			throw new NotImplementedException();
		}
	}

	public class ToScriptGlobals : BotScript.ScriptRun.ToScriptGlobals
	{
		public IHostToScript HostSanderling;
	}
}
