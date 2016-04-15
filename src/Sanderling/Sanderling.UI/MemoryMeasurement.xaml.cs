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

			Detail.TreeViewView = BotEngine.UI.InspectTreeView.ViewRefNezDifConstruct(
				Interface.FromInterfaceResponse.SerialisPolicyCache,
				Bib3.FCL.GBS.Inspektor.AstHeaderBackgroundBrushParam.SctandardParam,
				null,
				null);
		}

		public void Present(FromProcessMeasurement<MemoryStruct.IMemoryMeasurement> measurement)
		{
			Summary?.Present(measurement);

			Detail?.TreeView?.Präsentiire(new[] { measurement?.Value });
		}
	}
}
