using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bib3.Geometrik;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Parse
{
	public interface IOverviewEntry : MemoryStruct.IOverviewEntry
	{

	}

	public interface IWindowOverview : MemoryStruct.IWindowOverview
	{
		new MemoryStruct.IListViewAndControl<IOverviewEntry> ListView { get; }
	}

	public class WindowOverview : IWindowOverview
	{
		public MemoryStruct.IWindowOverview Raw;

		public MemoryStruct.IListViewAndControl<IOverviewEntry> ListView { set; get; }

		public MemoryStruct.IUIElementText[] ButtonText => Raw?.ButtonText;

		public string Caption => Raw?.Caption;

		public int? ChildLastInTreeIndex => Raw?.ChildLastInTreeIndex;

		public MemoryStruct.ISprite[] HeaderButton => Raw?.HeaderButton;

		public bool? HeaderButtonsVisible => Raw?.HeaderButtonsVisible;

		public long Id => Raw?.Id ?? 0;

		public MemoryStruct.IUIElementInputText[] InputText => Raw?.InputText;

		public int? InTreeIndex => Raw?.InTreeIndex;

		public bool? isModal => Raw?.isModal;

		public MemoryStruct.IUIElementText[] LabelText => Raw?.LabelText;

		public MemoryStruct.Tab[] PresetTab => Raw?.PresetTab;

		public OrtogoonInt Region => Raw?.Region ?? OrtogoonInt.Empty;

		public MemoryStruct.IUIElement RegionInteraction => Raw?.RegionInteraction;

		public MemoryStruct.ISprite[] Sprite => Raw?.Sprite;

		public string ViewportOverallLabelString => Raw?.ViewportOverallLabelString;

		MemoryStruct.IListViewAndControl<MemoryStruct.IOverviewEntry> MemoryStruct.IWindowOverview.ListView => ListView;
	}
}
