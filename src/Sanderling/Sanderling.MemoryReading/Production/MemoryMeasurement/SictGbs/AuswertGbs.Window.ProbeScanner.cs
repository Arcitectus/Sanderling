using System.Linq;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindowProbeScanner : SictAuswertGbsWindow
	{
		new static public WindowProbeScanner BerecneFürWindowAst(
			SictGbsAstInfoSictAuswert windowAst)
		{
			if (null == windowAst)
				return null;

			var windowAuswert = new SictAuswertGbsWindowProbeScanner(windowAst);

			windowAuswert.Berecne();

			return windowAuswert.ErgeebnisWindowProbeScanner;
		}

		public WindowProbeScanner ErgeebnisWindowProbeScanner
		{
			private set;
			get;
		}

		public SictAuswertGbsWindowProbeScanner(SictGbsAstInfoSictAuswert windowAst)
			:
			base(windowAst)
		{
		}

		override public void Berecne()
		{
			base.Berecne();

			if (null == Ergeebnis)
				return;

			var scanResultScrollAst =
				AstMainContainerMain
				?.SuuceFlacMengeAst(node => node?.PyObjTypNameIsScroll() ?? false)
				?.OrderBy(node => node.LaagePlusVonParentErbeLaageB() ?? int.MinValue)
				?.LastOrDefault();

			var listView =
				scanResultScrollAst?.AlsListView(
				(node, setHeader, regionConstraint) => SictAuswertGbsListViewport<IListEntry>.ListEntryKonstruktSctandard(node, setHeader, regionConstraint, ListEntryTrenungZeleTypEnum.Ast));

			ErgeebnisWindowProbeScanner =
				new WindowProbeScanner(Ergeebnis)
				{
					ScanResultView = listView,
				};
		}
	}
}
