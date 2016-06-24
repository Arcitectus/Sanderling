using BotEngine;
using BotEngine.Interface;
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

		public void Present(FromProcessMeasurement<Interface.MemoryStruct.IMemoryMeasurement> measurement)
		{
			var sessionDurationRemainingTooShort = !(measurement?.Value).SessionDurationRemainingSufficientToStayExposed();

			SessionDurationRemainingTextBox.Text = (measurement?.Value?.SessionDurationRemaining?.ToString() ?? "????");
			SessionDurationRemainingTooShortGroup.Visibility = (sessionDurationRemainingTooShort && null != measurement) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;

			Measurement?.Present(measurement);
		}
	}
}
