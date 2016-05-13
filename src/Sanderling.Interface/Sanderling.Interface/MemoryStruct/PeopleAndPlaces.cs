namespace Sanderling.Interface.MemoryStruct
{
	public class WindowPeopleAndPlaces : Window
	{
		public Tab[] Tab;

		public IListViewAndControl ListView;

		public WindowPeopleAndPlaces(IWindow @base)
			:
			base(@base)
		{
		}

		public WindowPeopleAndPlaces()
		{
		}
	}
}
