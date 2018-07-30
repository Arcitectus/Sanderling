namespace Sanderling.Interface.MemoryStruct
{
	public class WindowFittingMgmt : Window
	{
		public IListViewAndControl FittingView;

		public WindowFittingMgmt(IWindow @base)
			:
			base(@base)
		{
		}

		public WindowFittingMgmt()
		{
		}
	}

	public class WindowShipFitting : Window
	{
		public WindowShipFitting(IWindow @base)
			:
			base(@base)
		{
		}

		public WindowShipFitting()
		{
		}
	}

}
