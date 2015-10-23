using System;
using System.Collections.Generic;
using Bib3.Geometrik;
using BotEngine.Interface;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Parse
{
	public interface IOverviewEntry : MemoryStruct.IOverviewEntry, IListEntry
	{

	}

	public interface IWindowOverview : MemoryStruct.IWindowOverview
	{
		new MemoryStruct.IListViewAndControl<IOverviewEntry> ListView { get; }
	}

	public class OverviewEntry : ListEntry, IOverviewEntry
	{
		MemoryStruct.IOverviewEntry Raw;

		public MemoryStruct.ISprite[] RightIcon => Raw?.RightIcon;

		public OverviewEntry()
		{
		}

		public OverviewEntry(MemoryStruct.IOverviewEntry Raw)
			:
			base(Raw)
		{
			this.Raw = Raw;
		}
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

		WindowOverview()
		{
		}

		public WindowOverview(MemoryStruct.IWindowOverview Raw)
		{
			this.Raw = Raw;

			if (null == Raw)
			{
				return;
			}

			ListView = Raw?.ListView?.Map(OverviewExtension.Parse);
		}
	}

	static public class OverviewExtension
	{
		static public IWindowOverview Parse(this MemoryStruct.IWindowOverview WindowOverview) =>
			null == WindowOverview ? null : new WindowOverview(WindowOverview);

		static public IOverviewEntry Parse(this MemoryStruct.IOverviewEntry OverviewEntry) =>
			null == OverviewEntry ? null : new OverviewEntry(OverviewEntry);
	}
}
