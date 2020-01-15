using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BotEngine.EveOnline.Sensor;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public	class SictAuswertGbsWindowTelecom	:	SictAuswertGbsWindow
	{
		new	static public WindowTelecom BerecneFürWindowAst(
			UINodeInfoInTree WindowAst)
		{
			if (null == WindowAst)
			{
				return null;
			}

			var WindowAuswert = new SictAuswertGbsWindowTelecom(WindowAst);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisScpez;
		}

		public UINodeInfoInTree AstMainContainerBottom
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstMainContainerBottomButtons
		{
			private set;
			get;
		}

		public WindowTelecom ErgeebnisScpez
		{
			private set;
			get;
		}

		public SictAuswertGbsWindowTelecom(UINodeInfoInTree AstWindow)
			:
			base(AstWindow)
		{
		}

		override	public	void Berecne()
		{
			base.Berecne();

			AstMainContainerBottom =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				AstMainContainer, (Kandidaat) => string.Equals("bottom", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			AstMainContainerBottomButtons =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				AstMainContainerBottom,
				(Kandidaat) =>
					(string.Equals("EveButtonGroup", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) ||
					string.Equals("ButtonGroup", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase))
					&&
					string.Equals("btnsmainparent", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			var Ergeebnis = new WindowTelecom(base.Ergeebnis);

			this.ErgeebnisScpez = Ergeebnis;
		}
	}
}
