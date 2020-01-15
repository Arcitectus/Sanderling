using System.Linq;
using System.Text.RegularExpressions;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindowPeopleAndPlaces : SictAuswertGbsWindow
	{
		new static public WindowPeopleAndPlaces BerecneFürWindowAst(
			UINodeInfoInTree windowAst)
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

		public SictAuswertGbsWindowPeopleAndPlaces(UINodeInfoInTree windowAst)
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
				AstMainContainerMain?.ListPathToNodeFromSubtreeBreadthFirst(k => k.PyObjTypNameMatchesRegexPatternIgnoreCase("tab"))?.ToArray();

			var SetScrollAstPfaad =
				AstMainContainerMain.ListPathToNodeFromSubtreeBreadthFirst(ast => ast.PyObjTypNameIsScroll())
				?.ToArray();

			var SetScrollAst =
				AstMainContainerMain.MatchingNodesFromSubtreeBreadthFirst(ast => ast.PyObjTypNameIsScroll())
				?.ToArray();

			var tInspektSearchStringAst =
				AstMainContainer?.FirstMatchingNodeFromSubtreeBreadthFirst(k => Regex.Match(k?.LabelText() ?? "", "search", RegexOptions.IgnoreCase).Success);

			var ListView = SetScrollAst?.FirstOrDefault()?.AlsListView<IListEntry>(SictAuswertGbsListViewport<IListEntry>.ListEntryKonstruktSctandard);

			ErgeebnisScpez = new WindowPeopleAndPlaces(BaseErgeebnis)
			{
				ListView = ListView,
			};
		}
	}
}
