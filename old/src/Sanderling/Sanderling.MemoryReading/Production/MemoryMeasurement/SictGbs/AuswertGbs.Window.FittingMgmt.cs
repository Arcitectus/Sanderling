using Sanderling.Interface.MemoryStruct;
using BotEngine.Common;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindowFittingMgmt : SictAuswertGbsWindow
	{
		new static public WindowFittingMgmt BerecneFürWindowAst(
			UINodeInfoInTree windowAst)
		{
			if (null == windowAst)
				return null;

			var WindowAuswert = new SictAuswertGbsWindowFittingMgmt(windowAst);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisWindowFittingMgmt;
		}

		public UINodeInfoInTree LeftSideAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree LeftMainPanelAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree LeftMainPanelScrollAst
		{
			private set;
			get;
		}

		public WindowFittingMgmt ErgeebnisWindowFittingMgmt
		{
			private set;
			get;
		}

		public SictAuswertGbsWindowFittingMgmt(UINodeInfoInTree windowAst)
			:
			base(windowAst)
		{
		}

		override public void Berecne()
		{
			base.Berecne();

			if (null == base.Ergeebnis)
				return;

			LeftSideAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				AstMainContainerMain, (kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					"leftside".EqualsIgnoreCase(kandidaat.Name),
					2, 1);

			LeftMainPanelAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				LeftSideAst, (kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					"leftMainPanel".EqualsIgnoreCase(kandidaat.Name),
					2, 1);

			LeftMainPanelScrollAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				LeftMainPanelAst, (kandidaat) =>
					"Scroll".EqualsIgnoreCase(kandidaat.PyObjTypName),
					3, 1);

			var FittingViewportAuswert = new SictAuswertGbsListViewport<IListEntry>(LeftMainPanelScrollAst, SictAuswertGbsListViewport<IListEntry>.ListEntryKonstruktSctandard);

			FittingViewportAuswert.Read();

			ErgeebnisWindowFittingMgmt = new WindowFittingMgmt(base.Ergeebnis)
			{
				FittingView = FittingViewportAuswert?.Result,
			};
		}
	}
}
