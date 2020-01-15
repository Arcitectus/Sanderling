using BotEngine;
using BotEngine.Interface;
using BotEngine.UI;
using System.Windows.Controls;

namespace Sanderling.UI
{
	public partial class InterfaceToEve : UserControl
	{
		readonly public BotEngine.UI.ViewModel.License LicenseDataContext = new BotEngine.UI.ViewModel.License();

		public InterfaceToEve()
		{
			InitializeComponent();

			ProcessChoice?.PreferenceWriteToUI(new ChooseWindowProcessPreference { FilterMainModuleFileName = "ExeFile.exe" });
		}

		public void Present(
			SimpleInterfaceServerDispatcher interfaceServerDispatcher,
			FromProcessMeasurement<Interface.MemoryStruct.IMemoryMeasurement> measurement)
		{
			MeasurementLastHeader?.SetStatus(measurement.MemoryMeasurementLastStatusEnum());

			ProcessHeader?.SetStatus(ProcessChoice.ProcessStatusEnum());

			LicenseDataContext.Dispatcher = interfaceServerDispatcher;

			var sessionDurationRemainingTooShort = !(measurement?.Value).SessionDurationRemainingSufficientToStayExposed();

			SessionDurationRemainingTextBox.Text = (measurement?.Value?.SessionDurationRemaining?.ToString() ?? "????");
			SessionDurationRemainingTooShortGroup.Visibility = (sessionDurationRemainingTooShort && null != measurement) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Hidden;

			Measurement?.Present(measurement);
		}
	}
}
