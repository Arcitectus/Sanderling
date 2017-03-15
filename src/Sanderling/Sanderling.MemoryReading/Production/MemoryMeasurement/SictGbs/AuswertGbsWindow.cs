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
			SictGbsAstInfoSictAuswert windowNode)
		{
			if (null == windowNode)
				return null;

			var WindowAuswert = new SictAuswertGbsWindow(windowNode);

			WindowAuswert.Berecne();

			return WindowAuswert.Ergeebnis;
		}

		readonly public SictGbsAstInfoSictAuswert WindowNode;

		public SictGbsAstInfoSictAuswert AstMainContainer
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstMainContainerHeaderButtons
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert[] AstMainContainerHeaderButtonsMengeKandidaatButton
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstHeaderButtonClose
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstHeaderButtonMinimize
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstMainContainerMain
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstMainContainerHeaderParent
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstMainContainerHeaderParentCaptionParent
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert MainContainerHeaderParentCaptionParentLabelAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert MainContainerHeaderParentCaptionParentIcon
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

		public SictAuswertGbsWindow(SictGbsAstInfoSictAuswert windowNode)
		{
			this.WindowNode = windowNode;
		}

		static Regex HeaderButtonTypeRegex = "ButtonIcon".AlsRegexIgnoreCaseCompiled();

		virtual public void Berecne()
		{
			var AstWindow = this.WindowNode;

			if (!(AstWindow?.SictbarMitErbe ?? false))
				return;

			if (!(true == AstWindow.ListeChild?.Any((kandidaat) => (null == kandidaat ? null : kandidaat.SictbarMitErbe) ?? false)))
			{
				/*
				 * B:\Berict\Berict.Nuzer\[ZAK=2014.09.17.13.27.59,NB=25].Anwendung.Berict:
				 * 2014.09.17	Beobactung Probleem: Window "LSCStack" mit Ast direkt selbsct sictbar aber kaine sictbaare Ast enthaltend und daher vermuutlic für Nuzer unsictbar.
				 * Automaat versuuct diises leere Window zu scliise und blaibt damit in Sclaife hänge.
				 * */
				return;
			}

			AstMainContainer =
				AstWindow?.SuuceFlacMengeAstFrüheste((kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("__maincontainer", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase)
					, 2, 1);

			AstMainContainerHeaderButtons =
				AstMainContainer?.SuuceFlacMengeAstFrüheste((kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("headerButtons", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			AstMainContainerHeaderButtonsMengeKandidaatButton =
				AstMainContainerHeaderButtons?.SuuceFlacMengeAst((kandidaat) => kandidaat.PyObjTypNameIsButton(),
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
				AstMainContainer?.SuuceFlacMengeAstFrüheste((kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("main", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			var container = AstWindow.AlsContainer();

			if (null == container)
				return;

			AstMainContainerHeaderParent =
				AstMainContainer?.SuuceFlacMengeAstFrüheste((kandidaat) => string.Equals("headerParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			AstMainContainerHeaderParentCaptionParent =
				AstMainContainerHeaderParent?.SuuceFlacMengeAstFrüheste((kandidaat) => string.Equals("captionParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 3, 1);

			MainContainerHeaderParentCaptionParentLabelAst =
				AstMainContainerHeaderParentCaptionParent?.SuuceFlacMengeAstFrüheste((kandidaat) => string.Equals("EveLabelSmall", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 3, 1);

			MainContainerHeaderParentCaptionParentIcon =
				AstMainContainerHeaderParentCaptionParent?.SuuceFlacMengeAstFrüheste((kandidaat) =>
					(string.Equals("Icon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) ||
					string.Equals("EveIcon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase)),
					3, 1);

			HeaderCaptionText = MainContainerHeaderParentCaptionParentLabelAst?.SetText	?? AstWindow?.Caption;

			var HeaderButtonsVisible = AstMainContainerHeaderButtons?.SictbarMitErbe;

			var HeaderButton =
				AstMainContainerHeaderButtons
				?.SuuceFlacMengeAst(k =>
				(k?.SictbarMitErbe ?? false) &&
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
