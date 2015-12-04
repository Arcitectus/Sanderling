using System.Windows;

namespace Sanderling.Exe
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
		}

		public string TitleComputed =>
			"Sanderling v" + (TryFindResource("AppVersionId") ?? "");
	}
}
