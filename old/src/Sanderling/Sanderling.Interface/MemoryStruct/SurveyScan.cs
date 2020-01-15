namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowSurveyScanView : IWindow
	{
		IListViewAndControl ListView { get; }
	}

	public class WindowSurveyScanView : Window, IWindowSurveyScanView
	{
		public IListViewAndControl ListView { set; get; }

		public WindowSurveyScanView(IWindow @base)
			:
			base(@base)
		{
		}

		public WindowSurveyScanView()
		{
		}
	}
}
