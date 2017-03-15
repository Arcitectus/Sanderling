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
		readonly public SictGbsAstInfoSictAuswert NeocomAst;

		public SictGbsAstInfoSictAuswert NeocomMainContAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert NeocomMainContButtonContAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert NeocomClockLabelAst
		{
			private set;
			get;
		}

		public IUIElementText NeocomClockBescriftung
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert NeocomCharContAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert NeocomCharPicAst
		{
			private set;
			get;
		}

		public Neocom Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertNeocom(SictGbsAstInfoSictAuswert NeocomAst)
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

			if (!(true == NeocomAst.SictbarMitErbe))
			{
				return;
			}

			NeocomMainContAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				NeocomAst, (Kandidaat) =>
					string.Equals("mainCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			NeocomMainContButtonContAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				NeocomMainContAst, (Kandidaat) =>
					string.Equals("buttonCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			var EveMenuButton =
				NeocomMainContAst?.SuuceFlacMengeAstFrüheste(k => k.PyObjTypNameMatchesRegex(EveMenuButtonPyTypeRegex))
				?.AlsUIElementFalsUnglaicNullUndSictbar();

			var CharButton =
				//	2015.08.23 ShipFitting+FittingManagement+Lobby.AgentEntry:	Name = "charSheetBtn"
				NeocomMainContAst?.SuuceFlacMengeAstFrüheste(k => k.PyObjTypNameIsButton() && k.NameMatchesRegexPatternIgnoreCase("charSheet"))
				?.AlsUIElementFalsUnglaicNullUndSictbar();

			var NeocomListButtonAst =
				NeocomMainContButtonContAst
				.SuuceFlacMengeAst(k => k.PyObjTypNameIsButton())
				?.ToArray();

			var NeocomListButton =
				NeocomListButtonAst
				?.Where(ButtonAst => ButtonAst?.SictbarMitErbe ?? false)
				?.Select(ButtonAst => new UIElementText(ButtonAst.AlsUIElementFalsUnglaicNullUndSictbar(), ButtonAst?.Name))
				?.OrdnungLabel()
				?.ToArray();

			var Button =
				NeocomListButtonAst
				?.Select(ButtonAst => ButtonAst?.SuuceFlacMengeAstFrüheste(k => k.PyObjTypNameIsSprite()))
				?.WhereNotDefault()
				?.Select(Extension.AlsSprite)
				?.OrdnungLabel()
				?.ToArray();

			NeocomClockLabelAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				NeocomMainContAst, (Kandidaat) =>
					string.Equals("clockLabel", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					5, 1);

			if (null != NeocomClockLabelAst)
			{
				NeocomClockBescriftung = new UIElementText(
					NeocomClockLabelAst.AlsUIElementFalsUnglaicNullUndSictbar(), NeocomClockLabelAst.LabelText()?.RemoveXmlTag());
			}

			NeocomCharContAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				NeocomMainContAst, (Kandidaat) =>
					string.Equals("charCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					3, 1);

			Ergeebnis = new Neocom(NeocomAst.AlsUIElementFalsUnglaicNullUndSictbar())
			{
				EveMenuButton = EveMenuButton,
				CharButton = CharButton,
				Button = Button,
				Clock = NeocomClockBescriftung,
			};
		}
	}
}
