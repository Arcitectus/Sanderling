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

            State.Tracker.Configure(this)//the object to track
                .IdentifyAs("Main Sanderling Window")//a string by which to identify the target object
                .AddProperties<Window>(w => w.Height, w => w.Width, w => w.Top, w => w.Left, w => w.WindowState)//properties to track
                .RegisterPersistTrigger(nameof(SizeChanged))//when to persist data to the store
                .Apply();//apply any previously stored data
        }

		public string TitleComputed =>
			"Sanderling v" + (TryFindResource("AppVersionId") ?? "");
	}
}
