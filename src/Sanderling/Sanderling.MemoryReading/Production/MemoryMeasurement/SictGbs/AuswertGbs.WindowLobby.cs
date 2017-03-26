using Bib3;
using Sanderling.Interface.MemoryStruct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3.Geometrik;
using BotEngine.Common;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindowStation : SictAuswertGbsWindow
	{
		new static public WindowStation BerecneFürWindowAst(
			UINodeInfoInTree windowAst)
		{
			if (null == windowAst)
				return null;

			var WindowAuswert = new SictAuswertGbsWindowStation(windowAst);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisScpez;
		}

		public string ButtonUndockLabelText
		{
			private set;
			get;
		}

		public UINodeInfoInTree AgentsPanelAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree AgentsPanelScrollAst
		{
			private set;
			get;
		}

		public ScrollReader AgentsPanelScrollAuswert
		{
			private set;
			get;
		}

		public UINodeInfoInTree AgentsPanelScrollContentAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] MengeAgentEntryHeaderKandidaatAst
		{
			private set;
			get;
		}

		public KeyValuePair<int, string>[] MengeAgentEntryHeaderLaageMitText
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] MengeAgentEntryKandidaatAst
		{
			private set;
			get;
		}

		public SictAuswertGbsAgentEntry[] MengeAgentEntryKandidaatAuswert
		{
			private set;
			get;
		}

		public WindowStation ErgeebnisScpez
		{
			private set;
			get;
		}

		public SictAuswertGbsWindowStation(UINodeInfoInTree astFensterStationLobby)
			:
			base(astFensterStationLobby)
		{
		}

		override public void Berecne()
		{
			base.Berecne();

			var undockButtonNode =
				AstMainContainerMain?.FirstMatchingNodeFromSubtreeBreadthFirst(c =>
				(c?.PyObjTypName?.RegexMatchSuccessIgnoreCase("UndockBtn") ?? false));

			var undockButtonLabelNode = undockButtonNode?.LargestLabelInSubtree();

			var undockButtonActionNode = undockButtonNode?.MatchingNodesFromSubtreeBreadthFirst(c =>
				c?.PyObjAddress != undockButtonNode?.PyObjAddress && (c?.PyObjTypNameIsContainer() ?? false))?.LargestNodeInSubtree();

			var serviceButtonContainerAst =
				AstMainContainer?.FirstMatchingNodeFromSubtreeBreadthFirst(k =>
				k.PyObjTypNameIsContainer() && k.NameMatchesRegexPatternIgnoreCase("service.*Button"));

			var serviceButton =
				serviceButtonContainerAst?.MatchingNodesFromSubtreeBreadthFirst(k => k.PyObjTypNameIsButton())
				?.Select(buttonAst => buttonAst?.FirstMatchingNodeFromSubtreeBreadthFirst(spriteAst => spriteAst.PyObjTypNameIsSprite()))
				?.WhereNotDefault()
				?.Select(Extension.AlsSprite)
				?.OrdnungLabel()
				?.ToArray();

			AgentsPanelAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				AstMainContainerMain, (kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("agentsPanel", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			AgentsPanelScrollAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				AgentsPanelAst, (kandidaat) =>
					string.Equals("Scroll", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			AgentsPanelScrollAuswert = new ScrollReader(AgentsPanelScrollAst);

			AgentsPanelScrollAuswert.Read();

			AgentsPanelScrollContentAst = AgentsPanelScrollAuswert.ClipperContentNode;

			MengeAgentEntryHeaderKandidaatAst =
				Optimat.EveOnline.AuswertGbs.Extension.MatchingNodesFromSubtreeBreadthFirst(
				AgentsPanelScrollContentAst, (kandidaat) =>
					string.Equals("Header", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
					null, 2, 1);

			MengeAgentEntryKandidaatAst =
				Optimat.EveOnline.AuswertGbs.Extension.MatchingNodesFromSubtreeBreadthFirst(
				AgentsPanelScrollContentAst, (kandidaat) =>
					string.Equals("AgentEntry", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
					null, 2, 1);

			if (null != MengeAgentEntryHeaderKandidaatAst)
			{
				MengeAgentEntryHeaderLaageMitText =
					MengeAgentEntryHeaderKandidaatAst
					.Select((gbsAst) =>
						{
							string Text = null;
							int Laage = -1;

							var GbsAstFläce = AuswertGbs.Glob.FläceAusGbsAstInfoMitVonParentErbe(gbsAst);

							if (null != GbsAstFläce?.Center())
								Laage = (int)GbsAstFläce.Value.Center().B;

							var Label = Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
								gbsAst,
								(kandidaat) => string.Equals("EveLabelMedium", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
								2, 1);

							if (null != Label)
								Text = Label.SetText;

							return new KeyValuePair<int, string>(Laage, Text);
						})
						.Where((kandidaat) => null != kandidaat.Value)
						.OrderBy((kandidaat) => kandidaat.Key)
						.ToArray();
			}

			if (null != MengeAgentEntryKandidaatAst)
			{
				MengeAgentEntryKandidaatAuswert =
					MengeAgentEntryKandidaatAst
					.Select((gbsAst) =>
						{
							var Auswert = new SictAuswertGbsAgentEntry(gbsAst);
							Auswert.Berecne();
							return Auswert;
						}).ToArray();
			}

			if (null != MengeAgentEntryKandidaatAuswert &&
				null != MengeAgentEntryHeaderLaageMitText)
			{
				foreach (var AgentEntryKandidaatAuswert in MengeAgentEntryKandidaatAuswert)
				{
					if (null == AgentEntryKandidaatAuswert.Ergeebnis)
						continue;

					var InGbsFläce = AgentEntryKandidaatAuswert.Ergeebnis.Region;

					if (null == InGbsFläce)
						continue;

					var Header =
						MengeAgentEntryHeaderLaageMitText
						.LastOrDefault((kandidaat) => kandidaat.Key < InGbsFläce.Center().B);
				}
			}

			ButtonUndockLabelText = undockButtonLabelNode?.LabelText();

			var mengeAgentEntry =
				MengeAgentEntryKandidaatAuswert
				?.Select((auswert) => auswert.Ergeebnis)
				?.WhereNotDefault()
				?.OrdnungLabel()
				?.ToArray();

			var agentEntryHeader =
				MengeAgentEntryHeaderKandidaatAst
				?.Select(headerAst => headerAst?.LargestLabelInSubtree()?.AsUIElementTextIfTextNotEmpty())
				?.WhereNotDefault()
				?.OrdnungLabel()
				?.ToArray();

			var undockButton =
				undockButtonActionNode?.AsUIElementIfVisible();

			var unDocking =
				(null == ButtonUndockLabelText) ? (bool?)null :
					Regex.Match(ButtonUndockLabelText, "abort undock", RegexOptions.IgnoreCase).Success ||
					Regex.Match(ButtonUndockLabelText, "undocking", RegexOptions.IgnoreCase).Success;

			var aboveServicesLabel =
				base.Ergeebnis?.LabelText
				?.Where(k => k.Region.Center().B < undockButtonNode?.LaagePlusVonParentErbeLaageB() + undockButtonNode?.GrööseB)
				?.ToArray();

			var Ergeebnis = new WindowStation(base.Ergeebnis)
			{
				AboveServicesLabel = aboveServicesLabel,
				UndockButton = undockButton,
				ServiceButton = serviceButton,
				AgentEntry = mengeAgentEntry,
				AgentEntryHeader = agentEntryHeader,
			};

			this.ErgeebnisScpez = Ergeebnis;
		}
	}
}
