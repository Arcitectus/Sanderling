using BotEngine;
using BotEngine.Interface;
using BotEngine.UI;
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

		public void Present(
			SimpleInterfaceServerDispatcher interfaceServerDispatcher,
			FromProcessMeasurement<Interface.MemoryStruct.IMemoryMeasurement> measurement)
		{
			MeasurementLastHeader?.SetStatus(measurement.MemoryMeasurementLastStatusEnum());

			LicenseHeader?.SetStatus(interfaceServerDispatcher.LicenseStatusEnum());
			ProcessHeader?.SetStatus(ProcessChoice.ProcessStatusEnum());

			LicenseView?.Present(interfaceServerDispatcher);

			var sessionDurationRemainingTooShort = !(measurement?.Value).SessionDurationRemainingSufficientToStayExposed();

			SessionDurationRemainingTextBox.Text = (measurement?.Value?.SessionDurationRemaining?.ToString() ?? "????");
			SessionDurationRemainingTooShortGroup.Visibility = (sessionDurationRemainingTooShort && null != measurement) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;

			Measurement?.Present(measurement);
		}
	}
}
