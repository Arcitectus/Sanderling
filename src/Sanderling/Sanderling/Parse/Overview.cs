using Bib3.Geometrik;
using BotEngine.Common;
using System;
using System.Linq;
using MemoryStruct = Sanderling.Interface.MemoryStruct;
using System.Collections.Generic;

namespace Sanderling.Parse
{
	public interface IOverviewEntry : MemoryStruct.IOverviewEntry, IListEntry
	{
		MemoryStruct.ISprite MainIcon { set; get; }

		bool? MainIconIsRed { get; }

		bool? IsAttackingMe { get; }

		bool? IsJammingMe { get; }

		bool? IsHostile { get; }

		bool? MeTargeted { get; }

		bool? MeTargeting { get; }

		bool? MeActiveTarget { get; }
	}

	public interface IWindowOverview : MemoryStruct.IWindowOverview
	{
		new MemoryStruct.IListViewAndControl<IOverviewEntry> ListView { get; }
	}

	public class OverviewEntry : ListEntry, IOverviewEntry
	{
		MemoryStruct.IOverviewEntry Raw;

		public IEnumerable<string> MainIconSetIndicatorName => Raw?.MainIconSetIndicatorName;

		public MemoryStruct.ISprite[] RightIcon => Raw?.RightIcon;

		public MemoryStruct.ISprite MainIcon { set; get; }

		public bool? MainIconIsRed { set; get; }

		public bool? IsAttackingMe { set; get; }

		public bool? IsJammingMe { set; get; }

		public bool? IsHostile { set; get; }

		public bool? MeTargeted { set; get; }

		public bool? MeTargeting { set; get; }

		public bool? MeActiveTarget { set; get; }

		public OverviewEntry()
		{
		}

		public OverviewEntry(MemoryStruct.IOverviewEntry Raw)
			:
			base(Raw)
		{
			this.Raw = Raw;

			MainIcon = Raw?.SetSprite?.FirstOrDefault(Sprite => Sprite?.Name == "iconSprite");

			MainIconIsRed = MainIcon?.Color?.IsRed();

			var MainIconContainsIndicatorWithNameMatchingRegexPattern = new Func<string, bool>(regexPattern =>
				Raw?.MainIconSetIndicatorName?.Any(indicatorName => indicatorName.RegexMatchSuccessIgnoreCase(regexPattern)) ?? false);

			var ContainsRightIconWithHintMatchingRegexPattern = new Func<string, bool>(regexPattern =>
				Raw?.RightIcon?.Any(Sprite => (Sprite?.HintText).RegexMatchSuccessIgnoreCase(regexPattern)) ?? false);

			IsAttackingMe = MainIconContainsIndicatorWithNameMatchingRegexPattern("attacking.*me");
			IsHostile = MainIconContainsIndicatorWithNameMatchingRegexPattern("hostile");
			MeTargeting = MainIconContainsIndicatorWithNameMatchingRegexPattern("targeting");
			MeTargeted = MainIconContainsIndicatorWithNameMatchingRegexPattern("targetedByMe");
			MeActiveTarget = MainIconContainsIndicatorWithNameMatchingRegexPattern("myActiveTarget");

			IsJammingMe = ContainsRightIconWithHintMatchingRegexPattern("jamming.*me");
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

		public RectInt Region => Raw?.Region ?? RectInt.Empty;

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
