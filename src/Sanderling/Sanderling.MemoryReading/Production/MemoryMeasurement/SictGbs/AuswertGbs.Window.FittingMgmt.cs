using Sanderling.Interface.MemoryStruct;
using BotEngine.Common;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindowFittingMgmt : SictAuswertGbsWindow
	{
		new static public WindowFittingMgmt BerecneFürWindowAst(
			SictGbsAstInfoSictAuswert windowAst)
		{
			if (null == windowAst)
				return null;

			var WindowAuswert = new SictAuswertGbsWindowFittingMgmt(windowAst);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisWindowFittingMgmt;
		}

		public SictGbsAstInfoSictAuswert LeftSideAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert LeftMainPanelAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert LeftMainPanelScrollAst
		{
			private set;
			get;
		}

		public WindowFittingMgmt ErgeebnisWindowFittingMgmt
		{
			private set;
			get;
		}

		public SictAuswertGbsWindowFittingMgmt(SictGbsAstInfoSictAuswert windowAst)
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
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMainContainerMain, (kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					"leftside".EqualsIgnoreCase(kandidaat.Name),
					2, 1);

			LeftMainPanelAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				LeftSideAst, (kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					"leftMainPanel".EqualsIgnoreCase(kandidaat.Name),
					2, 1);

			LeftMainPanelScrollAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				LeftMainPanelAst, (kandidaat) =>
					"Scroll".EqualsIgnoreCase(kandidaat.PyObjTypName),
					3, 1);

			var FittingViewportAuswert = new SictAuswertGbsListViewport<IListEntry>(LeftMainPanelScrollAst, SictAuswertGbsListViewport<IListEntry>.ListEntryKonstruktSctandard);

			FittingViewportAuswert.Berecne();

			ErgeebnisWindowFittingMgmt = new WindowFittingMgmt(base.Ergeebnis)
			{
				FittingView = FittingViewportAuswert?.Ergeebnis,
			};
		}
	}
}
