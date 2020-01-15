using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bib3;
using BotEngine.EveOnline.Sensor;
using Sanderling.Interface.MemoryStruct;
using BotEngine.Common;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertNeocom
	{
		readonly public UINodeInfoInTree NeocomAst;

		public UINodeInfoInTree NeocomMainContAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree NeocomMainContButtonContAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree NeocomClockLabelAst
		{
			private set;
			get;
		}

		public IUIElementText NeocomClockBescriftung
		{
			private set;
			get;
		}

		public UINodeInfoInTree NeocomCharContAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree NeocomCharPicAst
		{
			private set;
			get;
		}

		public Neocom Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertNeocom(UINodeInfoInTree NeocomAst)
		{
			this.NeocomAst = NeocomAst;
		}

		static Regex EveMenuButtonPyTypeRegex = "ButtonEveMenu".AlsRegexIgnoreCaseCompiled();

		public void Berecne()
		{
			if (null == NeocomAst)
			{
				return;
			}

			if (!(true == NeocomAst.VisibleIncludingInheritance))
			{
				return;
			}

			NeocomMainContAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				NeocomAst, (Kandidaat) =>
					string.Equals("mainCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			NeocomMainContButtonContAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				NeocomMainContAst, (Kandidaat) =>
					string.Equals("buttonCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			var EveMenuButton =
				NeocomMainContAst?.FirstMatchingNodeFromSubtreeBreadthFirst(k => k.PyObjTypNameMatchesRegex(EveMenuButtonPyTypeRegex))
				?.AsUIElementIfVisible();

			var CharButton =
				//	2015.08.23 ShipFitting+FittingManagement+Lobby.AgentEntry:	Name = "charSheetBtn"
				NeocomMainContAst?.FirstMatchingNodeFromSubtreeBreadthFirst(k => k.PyObjTypNameIsButton() && k.NameMatchesRegexPatternIgnoreCase("charSheet"))
				?.AsUIElementIfVisible();

			var NeocomListButtonAst =
				NeocomMainContButtonContAst
				.MatchingNodesFromSubtreeBreadthFirst(k => k.PyObjTypNameIsButton())
				?.ToArray();

			var NeocomListButton =
				NeocomListButtonAst
				?.Where(ButtonAst => ButtonAst?.VisibleIncludingInheritance ?? false)
				?.Select(ButtonAst => new UIElementText(ButtonAst.AsUIElementIfVisible(), ButtonAst?.Name))
				?.OrdnungLabel()
				?.ToArray();

			var Button =
				NeocomListButtonAst
				?.Select(ButtonAst => ButtonAst?.FirstMatchingNodeFromSubtreeBreadthFirst(k => k.PyObjTypNameIsSprite()))
				?.WhereNotDefault()
				?.Select(Extension.AlsSprite)
				?.OrdnungLabel()
				?.ToArray();

			NeocomClockLabelAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				NeocomMainContAst, (Kandidaat) =>
					string.Equals("clockLabel", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					5, 1);

			if (null != NeocomClockLabelAst)
			{
				NeocomClockBescriftung = new UIElementText(
					NeocomClockLabelAst.AsUIElementIfVisible(), NeocomClockLabelAst.LabelText()?.RemoveXmlTag());
			}

			NeocomCharContAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				NeocomMainContAst, (Kandidaat) =>
					string.Equals("charCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					3, 1);

			Ergeebnis = new Neocom(NeocomAst.AsUIElementIfVisible())
			{
				EveMenuButton = EveMenuButton,
				CharButton = CharButton,
				Button = Button,
				Clock = NeocomClockBescriftung,
			};
		}
	}
}
