using BotEngine.UI;
using System.Windows.Controls;

namespace Sanderling.UI
{
	/// <summary>
	/// Interaction logic for Main.xaml
	/// </summary>
	public partial class Main : UserControl
	{
		public Main()
		{
			InitializeComponent();
		}

		public void BotMotionDisable() => ToggleButtonMotionEnable?.LeftButtonDown();

		public void BotMotionEnable() => ToggleButtonMotionEnable?.RightButtonDown();

		public LicenseClientConfig LicenseClientConfigControl => Interface?.LicenseClientConfig;

		public void ConfigFromModelToView(ExeConfig Config)
		{
			LicenseClientConfigControl.DataContext =
				new AutoDependencyPropertyComp<BotEngine.Client.LicenseClientConfig>(Config?.LicenseClient);
		}

		public ExeConfig ConfigFromViewToModel() =>
			new ExeConfig()
			{
				LicenseClient = (LicenseClientConfigControl.DataContext as AutoDependencyPropertyComp<BotEngine.Client.LicenseClientConfig>)
				?.PropagateFromDependencyPropertyToClrMember()
			};

	}
}
