using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindowItemSell : SictAuswertGbsWindow
	{
		new static public WindowItemSell BerecneFürWindowAst(
			SictGbsAstInfoSictAuswert windowAst)
		{
			if (null == windowAst)
				return null;

			var WindowAuswert = new SictAuswertGbsWindowItemSell(windowAst);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisScpez;
		}

		public WindowItemSell ErgeebnisScpez;

		public SictAuswertGbsWindowItemSell(SictGbsAstInfoSictAuswert windowNode)
			:
			base(windowNode)
		{
		}

		override public void Berecne()
		{
			base.Berecne();

			var BaseErgeebnis = base.Ergeebnis;

			if (null == BaseErgeebnis)
				return;

			this.ErgeebnisScpez = new WindowItemSell(BaseErgeebnis);
		}
	}
}
