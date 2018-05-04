using BotEngine.UI;
using System.Windows.Controls;

namespace Sanderling.UI
{
	public partial class Main : UserControl
	{
		public Main()
		{
			InitializeComponent();
		}

		public BotSharp.UI.Wpf.IDE DevelopmentEnvironment => BotsNavigation?.DevelopmentEnvironment;

		public void BotMotionDisable() => ToggleButtonMotionEnable?.LeftButtonDown();

		public void BotMotionEnable() => ToggleButtonMotionEnable?.RightButtonDown();
	}
}
