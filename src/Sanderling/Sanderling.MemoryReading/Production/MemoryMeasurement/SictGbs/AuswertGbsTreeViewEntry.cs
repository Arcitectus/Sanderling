using System;
using System.Linq;
using System.Text.RegularExpressions;
using BotEngine.Interface;
using Sanderling.Interface.MemoryStruct;
using Sanderling.Interface;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertTreeViewEntry
	{
		readonly public UINodeInfoInTree TreeViewEntryAst;

		public UINodeInfoInTree TopContAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree TopContLabelAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree ChildContAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] MengeChildAst
		{
			private set;
			get;
		}

		public SictAuswertTreeViewEntry[] MengeChildAuswert
		{
			private set;
			get;
		}

		public UINodeInfoInTree TopContIconAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree LabelAst
		{
			private set;
			get;
		}

		public TreeViewEntry Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertTreeViewEntry(UINodeInfoInTree TreeViewEntryAst)
		{
			this.TreeViewEntryAst = TreeViewEntryAst;
		}

		public void Berecne()
		{
			var TreeViewEntryAst = this.TreeViewEntryAst;

			if (null == TreeViewEntryAst)
			{
				return;
			}

			TopContAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				TreeViewEntryAst, (Kandidaat) =>
					Kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("topCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			TopContLabelAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				TopContAst, (Kandidaat) => AuswertGbs.Glob.GbsAstTypeIstLabel(Kandidaat));

			ChildContAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				TreeViewEntryAst, (Kandidaat) =>
					Kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("childCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			MengeChildAst =
				Optimat.EveOnline.AuswertGbs.Extension.MatchingNodesFromSubtreeBreadthFirst(
				ChildContAst, (Kandidaat) =>
					Regex.Match(Kandidaat.PyObjTypName ?? "", "TreeViewEntry", RegexOptions.IgnoreCase).Success,
					null, 2, 1);

			TopContIconAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				TopContAst, (Kandidaat) =>
					(string.Equals("Icon", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) ||
					string.Equals("EveIcon", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase)),
					2, 1);

			var TopContSpacerAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				TopContAst, (Kandidaat) =>
					Kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("spacerCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			var ExpandCollapseToggleFläce = TopContSpacerAst.AsUIElementIfVisible();

			LabelAst = TopContLabelAst;

			if (null != MengeChildAst)
			{
				MengeChildAuswert =
					MengeChildAst
					.Select((Ast) =>
						{
							var Auswert = new SictAuswertTreeViewEntry(Ast);
							Auswert.Berecne();
							return Auswert;
						}).ToArray();
			}

			IUIElement TopContFläce =
				(null == TopContAst) ? null : new UIElement(
					TopContAst.AsUIElementIfVisible());

			var TopContLabel =
				(null == TopContLabelAst) ? null : new UIElementText(
					TopContLabelAst.AsUIElementIfVisible(), TopContLabelAst.LabelText());

			var TopContIconTyp =
				(null == TopContIconAst) ? null : TopContIconAst.TextureIdent0;

			var TopContIconColor =
				(null == TopContIconAst) ? null : TopContIconAst.Color;

			var LabelText =
				(null == LabelAst) ? null : LabelAst.LabelText();

			var MengeChild =
				(null == MengeChildAuswert) ? null :
				MengeChildAuswert
					.Select((Auswert) => Auswert.Ergeebnis)
					.Where((Kandidaat) => null != Kandidaat)
					.ToArray();

			var Ergeebnis = new TreeViewEntry(TreeViewEntryAst.AlsContainer())
			{
				ExpandToggleButton = ExpandCollapseToggleFläce,
				Child = MengeChild,
				IsSelected = TreeViewEntryAst?.isSelected,
			};

			this.Ergeebnis = Ergeebnis;
		}
	}
}
