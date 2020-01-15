namespace Sanderling.Interface.MemoryStruct
{
	public interface IWindowProbeScanner : IWindow
	{
		IListViewAndControl ScanResultView { get; }
	}

	public class WindowProbeScanner : Window, IWindowProbeScanner
	{
		public IListViewAndControl ScanResultView { set; get; }

		public WindowProbeScanner(IWindow @base)
			:
			base(@base)
		{
		}

		public WindowProbeScanner()
		{
		}
	}
}
