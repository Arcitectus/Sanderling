using Bib3;
using BotEngine.Interface.HtmlAgilityPack;
using System.Collections.Generic;
using MemoryStruct = Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using BotEngine.Common;

namespace Sanderling.Parse
{
	public interface IWindowAgentDialogue : MemoryStruct.IWindowAgentDialogue
	{
		IDialogueMission Mission { get; }

		new IWindowAgentPane LeftPane { get; }

		new IWindowAgentPane RightPane { get; }

		MemoryStruct.IUIElement CloseButton { get; }

		MemoryStruct.IUIElement AcceptButton { get; }

		MemoryStruct.IUIElement DeclineButton { get; }

		MemoryStruct.IUIElement QuitButton { get; }

		MemoryStruct.IUIElement CompleteButton { get; }

		MemoryStruct.IUIElement DelayButton { get; }

		bool? DeclineWithoutStandingLossAvailable { get; }

		int? DeclineWithoutStandingLossWait { get; }
	}

	public interface IWindowAgentPane : MemoryStruct.IWindowAgentPane
	{
		string Caption { get; }

		IDialogueMissionObjective Objective { get; }

		IDialogueMissionReward Reward { get; }
	}

	public interface IDialogueMission
	{
		string Title { get; }

		IDialogueMissionObjective Objective { get; }

		IDialogueMissionReward Reward { get; }
	}

	public interface IDialogueMissionReward
	{
		IDialogueMissionRewardAtom Base { get; }

		IDialogueMissionRewardAtom Bonus { get; }
	}

	public interface IDialogueMissionRewardAtom
	{
		string Html { get; }

		int? ISK { get; }

		int? LP { get; }
	}

	public enum DialogueMissionObjectiveAtomTypeEnum
	{
		None = 0,
		Location = 1,
		LocationDropOff = 2,
		LocationPickUp = 3,
		Item = 4,
		Cargo = 5,
	}

	public interface IDialogueMissionObjective
	{
		string Html { get; }

		DialogueMissionObjectiveAtomTypeEnum? TypeEnum { get; }

		IDialogueMissionLocation Location { get; }

		IDialogueMissionObjective[] SetComponent { get; }

		DialogueMissionObjectiveItem Item { get; }

		bool? Complete { get; }
	}

	public interface IDialogueMissionLocation
	{
		string Name { get; }

		string SystemName { get; }

		int? SecurityLevelMilli { get; }
	}

	public class DialogueMissionObjective : IDialogueMissionObjective
	{
		public string Html { set; get; }

		public DialogueMissionObjectiveAtomTypeEnum? TypeEnum { set; get; }

		public IDialogueMissionLocation Location { set; get; }

		public DialogueMissionObjectiveItem Item { set; get; }

		public DialogueMissionObjective[] SetComponent { set; get; }

		IDialogueMissionObjective[] IDialogueMissionObjective.SetComponent => SetComponent;

		public bool? CompleteSelf { set; get; }

		public bool? CompleteFromComposition => 0 < SetComponent?.Length ? (SetComponent?.All(component => component?.Complete ?? true) ?? true) : (bool?)null;

		public bool? Complete => CompleteSelf ?? CompleteFromComposition;
	}

	public class DialogueMissionLocation : IDialogueMissionLocation
	{
		public string Name { set; get; }

		public Int32? SecurityLevelMilli { set; get; }

		public string SystemName { set; get; }
	}

	public class DialogueMissionReward : IDialogueMissionReward
	{
		public IDialogueMissionRewardAtom Base { set; get; }

		public IDialogueMissionRewardAtom Bonus { set; get; }
	}

	public class DialogueMissionRewardAtom : IDialogueMissionRewardAtom
	{
		public string Html { set; get; }

		public Int32? ISK { set; get; }

		public Int32? LP { set; get; }
	}

	public class DialogueMission : IDialogueMission
	{
		public IDialogueMissionObjective Objective { set; get; }

		public IDialogueMissionReward Reward { set; get; }

		public string Title { set; get; }
	}

	public partial class WindowAgentDialogue : IWindowAgentDialogue
	{
		public MemoryStruct.IWindowAgentDialogue Raw;

		public IDialogueMission Mission { set; get; }

		public IWindowAgentPane LeftPane { set; get; }

		public IWindowAgentPane RightPane { set; get; }

		public MemoryStruct.IUIElement CloseButton { set; get; }

		public MemoryStruct.IUIElement AcceptButton { set; get; }

		public MemoryStruct.IUIElement DeclineButton { set; get; }

		public MemoryStruct.IUIElement QuitButton { set; get; }

		public MemoryStruct.IUIElement CompleteButton { set; get; }

		public MemoryStruct.IUIElement DelayButton { set; get; }

		public bool? DeclineWithoutStandingLossAvailable { set; get; }

		public int? DeclineWithoutStandingLossWait { set; get; }
	}

	public partial class WindowAgentDialogue : IWindowAgentDialogue
	{
		public IEnumerable<MemoryStruct.IUIElementText> ButtonText => Raw?.ButtonText;

		public string Caption => Raw?.Caption;

		public Int32? ChildLastInTreeIndex => Raw?.ChildLastInTreeIndex;

		public MemoryStruct.ISprite[] HeaderButton => Raw?.HeaderButton;

		public bool? HeaderButtonsVisible => Raw?.HeaderButtonsVisible;

		public Int64 Id => Raw?.Id ?? 0;

		public IEnumerable<MemoryStruct.IUIElementInputText> InputText => Raw?.InputText;

		public Int32? InTreeIndex => Raw?.InTreeIndex;

		public bool? isModal => Raw?.isModal;

		public IEnumerable<MemoryStruct.IUIElementText> LabelText => Raw?.LabelText;

		public RectInt Region => Raw?.Region ?? RectInt.Empty;

		public MemoryStruct.IUIElement RegionInteraction => Raw?.RegionInteraction;

		public IEnumerable<MemoryStruct.ISprite> Sprite => Raw?.Sprite;

		MemoryStruct.IWindowAgentPane MemoryStruct.IWindowAgentDialogue.LeftPane => LeftPane;

		MemoryStruct.IWindowAgentPane MemoryStruct.IWindowAgentDialogue.RightPane => RightPane;
	}

	public partial class WindowAgentPane : IWindowAgentPane
	{
		public MemoryStruct.IWindowAgentPane Raw;

		public string Caption { set; get; }

		public IDialogueMissionObjective Objective { set; get; }

		public IDialogueMissionReward Reward { set; get; }
	}

	public partial class WindowAgentPane : IWindowAgentPane
	{
		public Int32? ChildLastInTreeIndex => Raw?.ChildLastInTreeIndex;

		public string Html => Raw?.Html;

		public Int64 Id => Raw?.Id ?? 0;

		public Int32? InTreeIndex => Raw?.InTreeIndex;

		public RectInt Region => Raw?.Region ?? RectInt.Empty;

		public MemoryStruct.IUIElement RegionInteraction => Raw?.RegionInteraction;
	}

	public class DialogueMissionObjectiveItem
	{
		public string Name;

		public int? Quantity;

		public int? VolumeMilli;
	}

	static public class DialogueMissionExtension
	{
		static public T ReturnValueOrDefaultIfThrows<T>(this Func<T> func)
		{
			try
			{
				if (null == func)
					return default(T);

				return func();
			}
			catch
			{
				return default(T);
			}
		}
		static public IEnumerable<IDialogueMissionObjective> SelfAndComponentTransitive(this IDialogueMissionObjective parent) =>
			parent.EnumerateNodeFromTreeBFirst(node => node.SetComponent);

		static public IWindowAgentDialogue Parse(this MemoryStruct.IWindowAgentDialogue window)
		{
			if (null == window)
				return null;

			IWindowAgentPane LeftPane = null;
			IWindowAgentPane RightPane = null;

			MemoryStruct.IUIElement AcceptButton = null;
			MemoryStruct.IUIElement DeclineButton = null;
			MemoryStruct.IUIElement CloseButton = null;
			MemoryStruct.IUIElement CompleteButton = null;
			MemoryStruct.IUIElement QuitButton = null;
			MemoryStruct.IUIElement DelayButton = null;

			DialogueMission Mission = null;

			try
			{
				LeftPane = window?.LeftPane?.Parse();
				RightPane = window?.RightPane?.Parse();

				var Objective = RightPane?.Objective;

				Mission =
					null == Objective ? null :
					new DialogueMission()
					{
						Title = LeftPane?.Caption,
						Objective = Objective,
						Reward = RightPane?.Reward,
					};

				var ButtonContainingTextIgnoringCase = new Func<string, MemoryStruct.IUIElement>(
					labelText => window?.ButtonText?.FirstOrDefault(button => button?.Text?.ToLower()?.Contains(labelText?.ToLower()) ?? false));

				AcceptButton = ButtonContainingTextIgnoringCase("Accept");
				DeclineButton = ButtonContainingTextIgnoringCase("Decline");
				CloseButton = ButtonContainingTextIgnoringCase("Close");
				CompleteButton = ButtonContainingTextIgnoringCase("Complete");
				QuitButton = ButtonContainingTextIgnoringCase("Quit");
				DelayButton = ButtonContainingTextIgnoringCase("Delay");
			}
			catch
			{
			}

			return new WindowAgentDialogue()
			{
				Raw = window,
				LeftPane = LeftPane,
				RightPane = RightPane,
				Mission = Mission,

				AcceptButton = AcceptButton,
				DeclineButton = DeclineButton,
				CloseButton = CloseButton,
				CompleteButton = CompleteButton,
				QuitButton = QuitButton,
				DelayButton = DelayButton,

				DeclineWithoutStandingLossAvailable = LeftPane?.Html?.DeclineWithoutStandingLossAvailableFromDialogueHtml(),
				DeclineWithoutStandingLossWait = LeftPane?.Html?.DeclineWithoutStandingLossWaitTimeFromDialogueHtml(),
			};
		}

		static public IEnumerable<HtmlAgilityPack.HtmlNode> SetNodeAfterNode(this HtmlAgilityPack.HtmlNode referenceNode) =>
			referenceNode.OwnerDocument?.DocumentNode?.Descendants()?.Where(node => referenceNode.StreamPosition < node.StreamPosition);

		static public bool AfterNode(this HtmlAgilityPack.HtmlNode node, HtmlAgilityPack.HtmlNode referenceNode) =>
			referenceNode?.StreamPosition < node?.StreamPosition;

		static public IWindowAgentPane Parse(this MemoryStruct.IWindowAgentPane pane)
		{
			if (null == pane)
				return null;

			var HtmlDocument = pane?.Html?.HAPDocumentFromHtml();

			var ListCaptionNode = HtmlDocument?.DocumentNode?.SelectNodes("//*[@id='subheader']");
			var CaptionNode = ListCaptionNode?.FirstOrDefault();

			var Caption = CaptionNode?.InnerText;

			var ObjectiveContainerNode = HtmlDocument?.DocumentNode?.SelectSingleNode("//table");

			var SetObjectiveAtomNode = ObjectiveContainerNode?.SelectNodes(".//tr");

			var SetObjectiveAtom =
				SetObjectiveAtomNode?.Select(ParseObjectiveAtom)?.ToArray();

			var Objective =
				null == ObjectiveContainerNode ? null :
				new DialogueMissionObjective()
				{
					Html = ObjectiveContainerNode?.OuterHtml,
					SetComponent = SetObjectiveAtom,
				};

			var RewardBaseCaptionNode = ListCaptionNode?.FirstOrDefault(node => node?.InnerText?.Trim()?.ToLower() == "rewards");

			var RewardBonusCaptionNode = ListCaptionNode?.FirstOrDefault(node => node?.InnerText?.RegexMatchSuccessIgnoreCase("bonus rewards") ?? false);

			var SetRewardCandidateNode = HtmlDocument?.DocumentNode?.SelectNodes("//table");

			var RewardBaseNode = SetRewardCandidateNode?.FirstOrDefault(node => node.AfterNode(RewardBaseCaptionNode));

			var RewardBonusNode = SetRewardCandidateNode?.FirstOrDefault(node => node.AfterNode(RewardBonusCaptionNode));

			var RewardBase = RewardBaseNode.RewardAtomFromHtmlNode();
			var RewardBonus = RewardBonusNode.RewardAtomFromHtmlNode();

			IDialogueMissionReward Reward = null;

			if (null != RewardBase || null != RewardBonus)
				Reward = new DialogueMissionReward()
				{
					Base = RewardBase,
					Bonus = RewardBonus,
				};

			return new WindowAgentPane()
			{
				Raw = pane,
				Caption = Caption,
				Objective = Objective,
				Reward = Reward,
			};
		}

		enum RewardTypeEnum
		{
			None,
			ISK,
			LP,
		}

		static IDictionary<string, RewardTypeEnum> RewardTypeFromImageSrc = new[]
		{
			new KeyValuePair<string, RewardTypeEnum>("icon:06_03", RewardTypeEnum.ISK),
			new KeyValuePair<string, RewardTypeEnum>("typeicon:29247", RewardTypeEnum.LP),
		}.ToDictionary();

		static public IDialogueMissionRewardAtom RewardAtomFromHtmlNode(this HtmlAgilityPack.HtmlNode htmlNode)
		{
			if (null == htmlNode)
				return null;

			var SetComponentTypeAndAmount = new Dictionary<RewardTypeEnum, int>();

			var SetComponentNode = htmlNode?.SelectNodes(".//tr");

			foreach (var Node in SetComponentNode.EmptyIfNull())
			{
				var ImageSrc = Node?.SelectSingleNode(".//img")?.GetAttributeValue("src", "");

				var RewardType = RewardTypeFromImageSrc.TryGetValueNullable(ImageSrc);

				var Amount = (int?)Node?.InnerText?.RegexMatchIfSuccess(Number.DefaultNumberFormatRegexAllowLeadingAndTrailingChars)?.Value?.NumberParseDecimal();

				if (!RewardType.HasValue || !Amount.HasValue)
					continue;

				SetComponentTypeAndAmount[RewardType.Value] = Amount.Value;
			}

			return new DialogueMissionRewardAtom()
			{
				Html = htmlNode?.OuterHtml,
				ISK = SetComponentTypeAndAmount?.TryGetValueNullable(RewardTypeEnum.ISK),
				LP = SetComponentTypeAndAmount?.TryGetValueNullable(RewardTypeEnum.LP),
			};
		}

		static public DialogueMissionObjective ParseObjectiveAtom(this HtmlAgilityPack.HtmlNode htmlNode)
		{
			if (null == htmlNode)
				return null;

			try
			{
				var ListTableCell = htmlNode?.SelectNodes(".//td");

				var CompletionCell = ListTableCell?.FirstOrDefault();

				bool? CompleteSelf = null;

				if (CompletionCell?.InnerHtml?.RegexMatchSuccessIgnoreCase(Regex.Escape("38_193")) ?? false)
					CompleteSelf = true;

				if (CompletionCell?.InnerHtml?.RegexMatchSuccessIgnoreCase(Regex.Escape("38_195")) ?? false)
					CompleteSelf = false;

				var TypeCell = ListTableCell?.ElementAtOrDefault(2);

				var LastCell = ListTableCell?.LastOrDefault();

				var TypeEnum = TypeCell?.InnerText?.Trim()?.ObjectiveAtomTypeEnumFromTableDialogueText();

				DialogueMissionLocation Location = null;
				DialogueMissionObjectiveItem Item = null;

				if (new[] { DialogueMissionObjectiveAtomTypeEnum.Location, DialogueMissionObjectiveAtomTypeEnum.LocationPickUp, DialogueMissionObjectiveAtomTypeEnum.LocationDropOff }.CastToNullable()
					.Contains(TypeEnum))
					Location = MissionLocationFromDialogue(LastCell);

				if (new[] { DialogueMissionObjectiveAtomTypeEnum.Item, DialogueMissionObjectiveAtomTypeEnum.Cargo }.CastToNullable()
					.Contains(TypeEnum))
					Item = ObjectiveItemFromDialogueText(LastCell?.InnerText);

				return new DialogueMissionObjective()
				{
					Html = htmlNode?.OuterHtml,
					TypeEnum = TypeEnum,
					Location = Location,
					Item = Item,
					CompleteSelf = CompleteSelf,
				};
			}
			catch
			{
				return null;
			}
		}

		static public IDictionary<string, DialogueMissionObjectiveAtomTypeEnum> DictObjectiveAtomTypeEnumFromTableDialogueText = new[]{
			new KeyValuePair<string, DialogueMissionObjectiveAtomTypeEnum>("Location", DialogueMissionObjectiveAtomTypeEnum.Location),
			new KeyValuePair<string, DialogueMissionObjectiveAtomTypeEnum>("Pickup Location", DialogueMissionObjectiveAtomTypeEnum.LocationPickUp),
			new KeyValuePair<string, DialogueMissionObjectiveAtomTypeEnum>("Drop-off Location", DialogueMissionObjectiveAtomTypeEnum.LocationDropOff),
			new KeyValuePair<string, DialogueMissionObjectiveAtomTypeEnum>("Cargo", DialogueMissionObjectiveAtomTypeEnum.Cargo),
			new KeyValuePair<string, DialogueMissionObjectiveAtomTypeEnum>("Item", DialogueMissionObjectiveAtomTypeEnum.Item),
		}.ToDictionary();

		static public DialogueMissionObjectiveAtomTypeEnum? ObjectiveAtomTypeEnumFromTableDialogueText(this string dialogueText) =>
			DictObjectiveAtomTypeEnumFromTableDialogueText?.TryGetValueNullable(dialogueText);

		static readonly string ObjectiveItemFromDialogueTextRegexPattern =
			@"(?<quant>\d+)\s*x" + @"\s*(?<name>[^" + Regex.Escape("(") + @"]+)(\((?<volume>[^\)]+)|)";

		static public DialogueMissionLocation MissionLocationFromDialogue(this HtmlAgilityPack.HtmlNode node)
		{
			var SecurityLevelMilli =
				node?.Descendants()?.Select(descendant => Number.NumberParseDecimalMilli(descendant?.InnerText?.Trim()))
				?.WhereNotDefault()
				?.FirstOrDefault();

			var NameNode = node?.SelectSingleNode(".//a");

			var Name = NameNode?.InnerText?.Trim();

			return new DialogueMissionLocation()
			{
				Name = Name,
				SecurityLevelMilli = (int?)SecurityLevelMilli,
				SystemName = Name?.AsLocation()?.SystemName,
			};
		}

		static public DialogueMissionObjectiveItem ObjectiveItemFromDialogueText(this string dialogueText)
		{
			var Match = dialogueText.RegexMatchIfSuccess(ObjectiveItemFromDialogueTextRegexPattern, RegexOptions.IgnoreCase);

			if (null == Match)
				return null;

			var VolumeValueMatch = Number.DefaultNumberFormatRegexAllowLeadingAndTrailingChars.Match(Match?.Groups["volume"]?.Value);

			var VolumeValueMilli = (int?)Number.NumberParseDecimalMilli(VolumeValueMatch?.Value);

			return new DialogueMissionObjectiveItem()
			{
				Quantity = Match.Groups["quant"].Value.TryParseInt(),
				Name = Match.Groups["name"].Value?.Trim(),
				VolumeMilli = VolumeValueMilli,
			};
		}

		static public bool LocationEquals(this IDialogueMissionLocation o0, IDialogueMissionLocation o1)
		{
			if (ReferenceEquals(o0, o1))
				return true;

			if (null == o0 || null == o1)
				return false;

			return
				o0.Name == o1.Name &&
				o0.SystemName == o1.SystemName &&
				o0.SecurityLevelMilli == o1.SecurityLevelMilli;
		}

		static public bool ItemEquals(this DialogueMissionObjectiveItem o0, DialogueMissionObjectiveItem o1)
		{
			if (ReferenceEquals(o0, o1))
				return true;

			if (null == o0 || null == o1)
				return false;

			return
				o0.Name == o1.Name &&
				o0.VolumeMilli == o1.VolumeMilli &&
				o0.Quantity == o1.Quantity;
		}

		static public bool? DeclineWithoutStandingLossAvailableFromDialogueHtml(this string html) =>
			html?.RegexMatchSuccessIgnoreCase("Declining a mission from a particular agent more than once every 4 hours (will|may) result in a loss of standing");

		static readonly KeyValuePair<string, int>[] DurationTextSetUnitRegexPatternAndValueInSecond = new KeyValuePair<string, int>[]
			{
				new KeyValuePair<string,    int>("hour", 60 * 60),
				new KeyValuePair<string,    int>("minute", 60),
				new KeyValuePair<string,    int>("second", 1),
			};

		static readonly string DurationTextComponentRegexPattern = "(\\d+)\\s*(\\w+)";

		static public int? SecondCountFromDurationText(this string durationText)
		{
			if (null == durationText)
				return null;

			var listComponentMatch =
				Regex.Matches(durationText, DurationTextComponentRegexPattern);

			int sum = 0;

			foreach (var componentMatch in (listComponentMatch?.Cast<Match>()).EmptyIfNull())
			{
				var componentValue = componentMatch.Groups[1].Value.TryParseInt();

				if (!componentValue.HasValue)
					return null;

				var setUnitMatch =
					DurationTextSetUnitRegexPatternAndValueInSecond
					.Where(unitNameAndValueInSecond => componentMatch.Groups[2].Value.RegexMatchSuccessIgnoreCase(unitNameAndValueInSecond.Key))
					.ToArray();

				if (!(1 == setUnitMatch.Length))
					return null;

				sum += componentValue.Value * setUnitMatch[0].Value;
			}

			return sum;
		}

		static public int? DeclineWithoutStandingLossWaitTimeFromDialogueHtml(this string html) =>
			html?.RegexMatchIfSuccess("Declining a mission from this agent within the next ([\\w\\d\\s]{1,40}) will result in a loss of standing")
			?.Groups[1]?.Value?.SecondCountFromDurationText();

	}
}
