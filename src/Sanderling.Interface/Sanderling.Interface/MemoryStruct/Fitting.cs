namespace Sanderling.Interface.MemoryStruct
{
	public class WindowFittingMgmt : Window
	{
		public IListViewAndControl FittingView;

		public WindowFittingMgmt(IWindow Base)
			:
			base(Base)
		{
		}

		public WindowFittingMgmt()
		{
		}
	}

	public class WindowFittingWindow : Window
	{
		public WindowFittingWindow(IWindow Base)
			:
			base(Base)
		{
		}

		public WindowFittingWindow()
		{
		}
	}

}
