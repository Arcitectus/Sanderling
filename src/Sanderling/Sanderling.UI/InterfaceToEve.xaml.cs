using BotEngine;
using System.Windows.Controls;

namespace Sanderling.UI
{
	/// <summary>
	/// Interaction logic for InterfaceToEve.xaml
	/// </summary>
	public partial class InterfaceToEve : UserControl
	{
		public InterfaceToEve()
		{
			InitializeComponent();

			ProcessChoice?.PreferenceWriteToUI(new ChooseWindowProcessPreference() { FilterMainModuleFileName = "ExeFile.exe" });
		}
	}
}
