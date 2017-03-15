using System.Linq;
using System.Text.RegularExpressions;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindowPeopleAndPlaces : SictAuswertGbsWindow
	{
		new static public WindowPeopleAndPlaces BerecneFürWindowAst(
			SictGbsAstInfoSictAuswert windowAst)
		{
			if (null == windowAst)
				return null;

			var WindowAuswert = new SictAuswertGbsWindowPeopleAndPlaces(windowAst);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisScpez;
		}

		public WindowPeopleAndPlaces ErgeebnisScpez
		{
			private set;
			get;
		}

		public SictAuswertGbsWindowPeopleAndPlaces(SictGbsAstInfoSictAuswert windowAst)
			:
			base(windowAst)
		{
		}

		override public void Berecne()
		{
			base.Berecne();

			var BaseErgeebnis = base.Ergeebnis;

			if (null == BaseErgeebnis)
				return;

			var SetTab =
				AstMainContainerMain?.SuuceFlacMengeAstMitPfaad(k => k.PyObjTypNameMatchesRegexPatternIgnoreCase("tab"))?.ToArray();

			var SetScrollAstPfaad =
				AstMainContainerMain.SuuceFlacMengeAstMitPfaad(ast => ast.PyObjTypNameIsScroll())
				?.ToArray();

			var SetScrollAst =
				AstMainContainerMain.SuuceFlacMengeAst(ast => ast.PyObjTypNameIsScroll())
				?.ToArray();

			var tInspektSearchStringAst =
				AstMainContainer?.SuuceFlacMengeAstFrüheste(k => Regex.Match(k?.LabelText() ?? "", "search", RegexOptions.IgnoreCase).Success);

			var ListView = SetScrollAst?.FirstOrDefault()?.AlsListView<IListEntry>(SictAuswertGbsListViewport<IListEntry>.ListEntryKonstruktSctandard);

			ErgeebnisScpez = new WindowPeopleAndPlaces(BaseErgeebnis)
			{
				ListView = ListView,
			};
		}
	}
}
