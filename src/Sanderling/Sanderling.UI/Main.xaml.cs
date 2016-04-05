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

		public void ConfigFromModelToView(ExeConfig Config) =>
			Interface.LicenseView?.LicenseClientConfigViewModel?.PropagateFromClrMemberToDependencyProperty(Config?.LicenseClient?.CompletedWithDefault());

		public ExeConfig ConfigFromViewToModel() =>
			new ExeConfig()
			{
				LicenseClient = Interface.LicenseView?.LicenseClientConfigViewModel?.PropagateFromDependencyPropertyToClrMember(),
			};
	}
}
