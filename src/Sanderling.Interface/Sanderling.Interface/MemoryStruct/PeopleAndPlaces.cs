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

		public IListViewAndControl ListView;

		public WindowPeopleAndPlaces(IWindow Base)
			:
			base(Base)
		{
		}

		public WindowPeopleAndPlaces()
		{
		}
	}
}
