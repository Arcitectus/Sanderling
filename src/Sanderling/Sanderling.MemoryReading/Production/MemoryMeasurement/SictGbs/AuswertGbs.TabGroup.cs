using System.Linq;
using Sanderling.Interface.MemoryStruct;
using BotEngine.Common;
using Bib3.Geometrik;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsTabGroup
	{
		public UINodeInfoInTree TabGroupAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] MengeTabAst
		{
			private set;
			get;
		}

		public SictAuswertGbsTab[] MengeTabAuswert
		{
			private set;
			get;
		}

		public string FittingBezaicner
		{
			private set;
			get;
		}

		public bool? Selected
		{
			private set;
			get;
		}

		public TabGroup Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertGbsTabGroup(UINodeInfoInTree tabGroupAst)
		{
			this.TabGroupAst = tabGroupAst;
		}

		public void Berecne()
		{
			var TabGroupAst = this.TabGroupAst;

			if (null == TabGroupAst)
				return;

			if (!(true == TabGroupAst.VisibleIncludingInheritance))
				return;

			MengeTabAst =
				Optimat.EveOnline.AuswertGbs.Extension.MatchingNodesFromSubtreeBreadthFirst(
				TabGroupAst, (kandidaat) =>
					"Tab".EqualsIgnoreCase(kandidaat.PyObjTypName),
					null, 2, 1, true);

			MengeTabAuswert =
				MengeTabAst?.Select(
				(tabAst) =>
				{
					var TabAuswert = new SictAuswertGbsTab(tabAst);
					TabAuswert.Berecne();
					return TabAuswert;
				})
				?.ToArray();

			var MengeTab =
				MengeTabAuswert
				?.Select((tabAuswert) => tabAuswert.Ergeebnis)
				?.Where((tab) => null != tab)
				?.ToArray();

			var ListeTab =
				MengeTab
				?.OrderBy(tab => tab.Region.Center().A)
				?.ToArray();

			var TabSelected =
				(null == MengeTab) ? null :
				MengeTab.FirstOrDefault((kandidaatTab) =>
					MengeTab.All((kandidaatKonkurent) => kandidaatKonkurent == kandidaatTab ||
						(kandidaatKonkurent.LabelColorOpacityMilli ?? 0) < kandidaatTab.LabelColorOpacityMilli - 100));

			var Ergeebnis = new TabGroup(TabGroupAst.AsUIElementIfVisible()) { ListTab = ListeTab };

			this.Ergeebnis = Ergeebnis;
		}
	}
}
