using BotEngine.Common;
using BotEngine.UI;
using System.Windows.Controls;

namespace Sanderling.UI
{
	public partial class Main : UserControl
	{
		static public ISingleValueStore<string> LicenseKeyStore;

		public Main()
		{
			InitializeComponent();

			Interface.LicenseDataContext.LicenseKeyStore = LicenseKeyStore;
		}

		public BotSharp.UI.Wpf.IDE DevelopmentEnvironment => BotsNavigation?.DevelopmentEnvironment;

		public void BotMotionDisable() => ToggleButtonMotionEnable?.LeftButtonDown();

		public void BotMotionEnable() => ToggleButtonMotionEnable?.RightButtonDown();
	}
}
