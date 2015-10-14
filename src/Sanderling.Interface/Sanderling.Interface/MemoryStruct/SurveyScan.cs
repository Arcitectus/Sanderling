using System.Collections.Generic;
using System.Linq;

namespace Sanderling.Interface.MemoryStruct
{
	public class WindowSurveyScanView : Window
	{
		public IListViewAndControl ListView;

		public WindowSurveyScanView(IWindow Base)
			:
			base(Base)
		{
		}

		public WindowSurveyScanView()
		{
		}
	}
}
