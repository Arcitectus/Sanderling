using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanderling.Interface.MemoryStruct
{
	public class WindowPeopleAndPlaces : Window
	{
		public Tab[] Tab;

		public ListViewAndControl ListView;

		public WindowPeopleAndPlaces(Window Base)
			:
			base(Base)
		{
		}

		public WindowPeopleAndPlaces()
		{
		}
	}
}
