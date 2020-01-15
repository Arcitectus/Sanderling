using Sanderling.Interface.MemoryStruct;
using BotEngine.EveOnline.Sensor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsAgentEntry
	{
		readonly public UINodeInfoInTree AstAgentEntry;

		public UINodeInfoInTree TextContAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree ButtonStartConversationAst
		{
			private set;
			get;
		}

		public IUIElement StartConversationButton
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstTextContText
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] AstTextContTextMengeLabel
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] AstTextContTextMengeLabelLinx
		{
			private set;
			get;
		}


		public LobbyAgentEntry Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertGbsAgentEntry(UINodeInfoInTree AstAgentEntry)
		{
			this.AstAgentEntry = AstAgentEntry;
		}

		/*
		 * 2013.07.30
		 * Bsp: "Level I - Security"
		 * */
		static string AusAgentEntryTextAgentTypUndLevelRegexPattern = "Level\\s+([IV]{1,3})\\s*" + Regex.Escape("-") + "\\s*(\\w+)";

		static string[] AgentLevelString = new string[] { null, "I", "II", "III", "IV", "V" };

		static KeyValuePair<string, int?>[] ListeLevelStringMitLevel =
			AgentLevelString
			.Select((String, Index) => new KeyValuePair<string, int?>(String, Index))
			.ToArray();

		static public KeyValuePair<string, int>? AgentTypUndLevel(string AusAgentEntryText)
		{
			if (null == AusAgentEntryText)
			{
				return null;
			}

			var Match = Regex.Match(AusAgentEntryText, AusAgentEntryTextAgentTypUndLevelRegexPattern, RegexOptions.IgnoreCase);

			if (!Match.Success)
			{
				return null;
			}

			var Typ = Match.Groups[2].Value;

			var LevelSictString = Match.Groups[1].Value;

			var Level =
				ListeLevelStringMitLevel
				.FirstOrDefault((Kandidaat) => string.Equals(Kandidaat.Key, LevelSictString, StringComparison.InvariantCultureIgnoreCase)).Value;

			return new KeyValuePair<string, int>(Typ, Level ?? -1);
		}

		public void Berecne()
		{
			if (null == AstAgentEntry)
			{
				return;
			}

			if (!(true == AstAgentEntry.VisibleIncludingInheritance))
			{
				return;
			}

			TextContAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				AstAgentEntry, (Kandidaat) => string.Equals("textCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			ButtonStartConversationAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				AstAgentEntry, (Kandidaat) =>
					string.Equals("ButtonIcon", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) &&
					Regex.Match(Kandidaat.Hint ?? "", "Conversation", RegexOptions.IgnoreCase).Success,
					2, 1);

			AstTextContText =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				TextContAst, (Kandidaat) => string.Equals("text", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			AstTextContTextMengeLabel =
				Optimat.EveOnline.AuswertGbs.Extension.MatchingNodesFromSubtreeBreadthFirst(
				AstTextContText, (Kandidaat) =>
					string.Equals("EveLabelMedium", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
					null, 2, 1);

			StartConversationButton = ButtonStartConversationAst.AsUIElementIfVisible();

			if (null != AstTextContTextMengeLabel)
			{
				AstTextContTextMengeLabel =
					AstTextContTextMengeLabel
					.OrderBy((Kandidaat) =>
						{
							var LaagePlusVonParentErbeLaage = Kandidaat.LaagePlusVonParentErbeLaage();

							if (!LaagePlusVonParentErbeLaage.HasValue)
							{
								return -1;
							}

							return LaagePlusVonParentErbeLaage.Value.B;
						})
					.ToArray();
			}

			if (null != AstTextContTextMengeLabel &&
				null != AstTextContText)
			{
				if (AstTextContText.Grööse.HasValue)
				{
					AstTextContTextMengeLabelLinx =
						AstTextContTextMengeLabel
						.Where((Kandidaat) =>
							{
								if (!Kandidaat.LaageInParent.HasValue)
								{
									return false;
								}

								return Kandidaat.LaageInParent.Value.A < AstTextContText.Grööse.Value.A * 0.5;
							})
						.ToArray();
				}
			}

			if (null == AstTextContTextMengeLabelLinx)
			{
				return;
			}

			string AgentName = null;
			string AgentTyp = null;
			int? AgentLevel = null;
			string ZaileTypUndLevelText = null;

			if (null != AstTextContTextMengeLabelLinx)
			{
				if (1 < AstTextContTextMengeLabelLinx.Length)
				{
					AgentName = AstTextContTextMengeLabelLinx.FirstOrDefault().SetText;
					ZaileTypUndLevelText = AstTextContTextMengeLabelLinx.LastOrDefault().SetText;
				}
			}

			var TypUndLevel = AgentTypUndLevel(ZaileTypUndLevelText);

			if (TypUndLevel.HasValue)
			{
				AgentTyp = TypUndLevel.Value.Key;
				AgentLevel = TypUndLevel.Value.Value;
			}

			var Ergeebnis = new LobbyAgentEntry(TextContAst.AsUIElementIfVisible())
			{
				LabelText = AstAgentEntry?.ExtraktMengeLabelString()?.OrdnungLabel()?.ToArray(),
				StartConversationButton = StartConversationButton,
			};

			this.Ergeebnis = Ergeebnis;
		}
	}
}
