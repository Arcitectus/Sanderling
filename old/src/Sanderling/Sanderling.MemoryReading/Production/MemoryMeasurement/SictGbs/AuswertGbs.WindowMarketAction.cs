using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	/// <summary>
	/// 2015.08.19
	/// Kan "Modify Order" oder "Buy ...." enthalte. Unterscaidung zwisce diise baide Verwendunge könte z.B. anhand Bescriftung des Button oder andere Bescriftunge in Window vorgenome werde.
	/// </summary>
	public class SictAuswertGbsWindowMarketAction : SictAuswertGbsWindow
	{
		new static public WindowMarketAction BerecneFürWindowAst(
			UINodeInfoInTree windowAst)
		{
			if (null == windowAst)
				return null;

			var WindowAuswert = new SictAuswertGbsWindowMarketAction(windowAst);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisScpez;
		}

		public WindowMarketAction ErgeebnisScpez;

		public SictAuswertGbsWindowMarketAction(UINodeInfoInTree windowNode)
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

			ErgeebnisScpez = new WindowMarketAction(BaseErgeebnis);
		}
	}
}
