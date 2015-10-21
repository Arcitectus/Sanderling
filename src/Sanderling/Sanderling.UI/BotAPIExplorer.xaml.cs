using System.Windows.Controls;

namespace Sanderling.UI
{
	/// <summary>
	/// Interaction logic for BotAPIExplorer.xaml
	/// </summary>
	public partial class BotAPIExplorer : UserControl
	{
		public BotAPIExplorer()
		{
			InitializeComponent();

			TreeView.TreeViewView = new ApiTreeViewNodeView();
		}

		public void Present(Script.IHostToScript Api)
		{
			TreeView.TreeView.Präsentiire(Api);
		}
	}

}
