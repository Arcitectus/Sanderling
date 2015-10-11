using BotEngine.Interface;
using System.Windows.Controls;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.UI
{
	/// <summary>
	/// Interaction logic for MemoryMeasurement.xaml
	/// </summary>
	public partial class MemoryMeasurement : UserControl
	{
		public MemoryMeasurement()
		{
			InitializeComponent();
		}

		public void Present(FromProcessMeasurement<MemoryStruct.MemoryMeasurement> Measurement)
		{
			Summary?.Present(Measurement);

			Detail?.TreeView?.Präsentiire(Measurement.Wert);
		}
	}
}
