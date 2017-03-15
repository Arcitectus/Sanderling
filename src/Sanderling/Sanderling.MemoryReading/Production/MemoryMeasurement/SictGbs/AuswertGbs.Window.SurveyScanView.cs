using System;
using System.Linq;
using Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindowSurveyScanView : SictAuswertGbsWindow
	{
		new static public WindowSurveyScanView BerecneFürWindowAst(
			SictGbsAstInfoSictAuswert windowAst)
		{
			if (null == windowAst)
				return null;

			var WindowAuswert = new SictAuswertGbsWindowSurveyScanView(windowAst);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisWindowSurveyScanView;
		}

		public SictGbsAstInfoSictAuswert ScrollAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert ListAst
		{
			private set;
			get;
		}

		public SictAuswertGbsListViewport<IListEntry> ListAuswert
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert ButtonGroupAst
		{
			private set;
			get;
		}

		public WindowSurveyScanView ErgeebnisWindowSurveyScanView
		{
			private set;
			get;
		}

		public SictAuswertGbsWindowSurveyScanView(SictGbsAstInfoSictAuswert windowAst)
			:
			base(windowAst)
		{
		}

		static public IListEntry SurveyScanViewEntryKonstrukt(
			SictGbsAstInfoSictAuswert entryAst,
			IColumnHeader[] listeScrollHeader,
			RectInt? regionConstraint)
		{
			if (!(entryAst?.SictbarMitErbe ?? false))
				return null;

			var ChildTransitive = entryAst.MengeChildAstTransitiiveHüle()?.ToArray();

			var ListEntryAuswert = new SictAuswertGbsListEntry(entryAst, listeScrollHeader, regionConstraint, ListEntryTrenungZeleTypEnum.InLabelTab);

			ListEntryAuswert.Berecne();

			return ListEntryAuswert?.ErgeebnisListEntry;
		}

		override public void Berecne()
		{
			base.Berecne();

			if (null == base.Ergeebnis)
			{
				return;
			}

			ScrollAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMainContainerMain, (kandidaat) =>
					string.Equals("Scroll", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			ButtonGroupAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMainContainer, (kandidaat) =>
					string.Equals("ButtonGroup", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
					3, 1);

			if (null == ScrollAst)
				return;

			ListAuswert = new SictAuswertGbsListViewport<IListEntry>(ScrollAst, SictAuswertGbsWindowSurveyScanView.SurveyScanViewEntryKonstrukt);

			ListAuswert.Berecne();

			ErgeebnisWindowSurveyScanView =
				new WindowSurveyScanView(base.Ergeebnis)
				{
					ListView = ListAuswert.Ergeebnis,
				};
		}
	}
}
