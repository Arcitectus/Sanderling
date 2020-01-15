using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public	class SictAuswertGbsHybridWindow	:	SictAuswertGbsMessageBox
	{
		new static public HybridWindow BerecneFürWindowAst(
			UINodeInfoInTree WindowAst)
		{
			if (null == WindowAst)
			{
				return null;
			}

			var WindowAuswert = new SictAuswertGbsHybridWindow(WindowAst);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisScpezHybridWindow;
		}

		public HybridWindow	ErgeebnisScpezHybridWindow
		{
			private set;
			get;
		}

		public SictAuswertGbsHybridWindow(UINodeInfoInTree WindowAst)
			:
			base(WindowAst)
		{
		}

		override	public	void Berecne()
		{
			base.Berecne();

			var ErgeebnisScpez = base.ErgeebnisScpez;

			if (null == ErgeebnisScpez)
			{
				return;
			}

			var ErgeebnisScpezHybridWindow = new HybridWindow(ErgeebnisScpez);

			this.ErgeebnisScpezHybridWindow = ErgeebnisScpezHybridWindow;
		}
	}
}
