using System;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Script
{
	public interface IHostToScript : BotScript.IHostToScript
	{
		MemoryStruct.MemoryMeasurement LastMemoryMeasurement
		{
			get;
		}

		void ClickUIElement(MemoryStruct.UIElement Target, bool RightButton);

    }

	public class HostToScript : IHostToScript
	{
		MemoryStruct.MemoryMeasurement IHostToScript.LastMemoryMeasurement
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public void ClickUIElement(MemoryStruct.UIElement Target, bool RightButton)
		{
			throw new NotImplementedException();
		}

		public void Delay(int Duration)
		{
			throw new NotImplementedException();
		}

		public void Log(object o)
		{
			throw new NotImplementedException();
		}
	}
}
