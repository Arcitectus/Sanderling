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

	public class WindowShipFitting : Window
	{
		public WindowShipFitting(IWindow Base)
			:
			base(Base)
		{
		}

		public WindowShipFitting()
		{
		}
	}

}
