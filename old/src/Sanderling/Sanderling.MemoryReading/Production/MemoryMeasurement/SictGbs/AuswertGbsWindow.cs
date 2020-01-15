using System;
using System.Linq;
using Bib3;
using System.Text.RegularExpressions;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindow
	{
		static public Window BerecneFürWindowAst(
			UINodeInfoInTree windowNode)
		{
			if (null == windowNode)
				return null;

			var WindowAuswert = new SictAuswertGbsWindow(windowNode);

			WindowAuswert.Berecne();

			return WindowAuswert.Ergeebnis;
		}

		readonly public UINodeInfoInTree WindowNode;

		public UINodeInfoInTree AstMainContainer
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstMainContainerHeaderButtons
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] AstMainContainerHeaderButtonsMengeKandidaatButton
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstHeaderButtonClose
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstHeaderButtonMinimize
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstMainContainerMain
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstMainContainerHeaderParent
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstMainContainerHeaderParentCaptionParent
		{
			private set;
			get;
		}

		public UINodeInfoInTree MainContainerHeaderParentCaptionParentLabelAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree MainContainerHeaderParentCaptionParentIcon
		{
			private set;
			get;
		}

		public string HeaderCaptionText
		{
			private set;
			get;
		}

		public Window Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertGbsWindow(UINodeInfoInTree windowNode)
		{
			this.WindowNode = windowNode;
		}

		static Regex HeaderButtonTypeRegex = "ButtonIcon".AlsRegexIgnoreCaseCompiled();

		virtual public void Berecne()
		{
			var AstWindow = this.WindowNode;

			if (!(AstWindow?.VisibleIncludingInheritance ?? false))
				return;

			if (!(true == AstWindow.ListChild?.Any((kandidaat) => (null == kandidaat ? null : kandidaat.VisibleIncludingInheritance) ?? false)))
			{
				/*
				 * B:\Berict\Berict.Nuzer\[ZAK=2014.09.17.13.27.59,NB=25].Anwendung.Berict:
				 * 2014.09.17	Beobactung Probleem: Window "LSCStack" mit Ast direkt selbsct sictbar aber kaine sictbaare Ast enthaltend und daher vermuutlic für Nuzer unsictbar.
				 * Automaat versuuct diises leere Window zu scliise und blaibt damit in Sclaife hänge.
				 * */
				return;
			}

			AstMainContainer =
				AstWindow?.FirstMatchingNodeFromSubtreeBreadthFirst((kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("__maincontainer", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase)
					, 2, 1);

			AstMainContainerHeaderButtons =
				AstMainContainer?.FirstMatchingNodeFromSubtreeBreadthFirst((kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("headerButtons", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			AstMainContainerHeaderButtonsMengeKandidaatButton =
				AstMainContainerHeaderButtons?.MatchingNodesFromSubtreeBreadthFirst((kandidaat) => kandidaat.PyObjTypNameIsButton(),
				null, 2, 1);

			AstHeaderButtonClose =
				AstMainContainerHeaderButtonsMengeKandidaatButton?.SuuceFlacMengeAstFrüheste((kandidaat) =>
					string.Equals("close", kandidaat.Name ?? kandidaat.Hint, StringComparison.InvariantCultureIgnoreCase),
					2, 0);

			AstHeaderButtonMinimize =
				AstMainContainerHeaderButtonsMengeKandidaatButton?.SuuceFlacMengeAstFrüheste((kandidaat) =>
					string.Equals("minimize", kandidaat.Name ?? kandidaat.Hint, StringComparison.InvariantCultureIgnoreCase),
					2, 0);

			AstMainContainerMain =
				AstMainContainer?.FirstMatchingNodeFromSubtreeBreadthFirst((kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("main", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			var container = AstWindow.AlsContainer();

			if (null == container)
				return;

			AstMainContainerHeaderParent =
				AstMainContainer?.FirstMatchingNodeFromSubtreeBreadthFirst((kandidaat) => string.Equals("headerParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			AstMainContainerHeaderParentCaptionParent =
				AstMainContainerHeaderParent?.FirstMatchingNodeFromSubtreeBreadthFirst((kandidaat) => string.Equals("captionParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 3, 1);

			MainContainerHeaderParentCaptionParentLabelAst =
				AstMainContainerHeaderParentCaptionParent?.FirstMatchingNodeFromSubtreeBreadthFirst((kandidaat) => string.Equals("EveLabelSmall", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 3, 1);

			MainContainerHeaderParentCaptionParentIcon =
				AstMainContainerHeaderParentCaptionParent?.FirstMatchingNodeFromSubtreeBreadthFirst((kandidaat) =>
					(string.Equals("Icon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) ||
					string.Equals("EveIcon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase)),
					3, 1);

			HeaderCaptionText = MainContainerHeaderParentCaptionParentLabelAst?.SetText	?? AstWindow?.Caption;

			var HeaderButtonsVisible = AstMainContainerHeaderButtons?.VisibleIncludingInheritance;

			var HeaderButton =
				AstMainContainerHeaderButtons
				?.MatchingNodesFromSubtreeBreadthFirst(k =>
				(k?.VisibleIncludingInheritance ?? false) &&
				k.PyObjTypNameMatchesRegex(HeaderButtonTypeRegex))
				?.Select(k => k.AlsSprite())
				?.WhereNotDefault()
				?.ToArrayIfNotEmpty();

			Ergeebnis = new Window(container)
			{
				isModal = AstWindow?.isModal,
				Caption = HeaderCaptionText,
				HeaderButtonsVisible = HeaderButtonsVisible,
				HeaderButton = HeaderButton,
			};
		}
	}
}
