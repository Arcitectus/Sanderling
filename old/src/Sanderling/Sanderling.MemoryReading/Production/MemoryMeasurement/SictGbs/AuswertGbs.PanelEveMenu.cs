using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Optimat.EveOnline.AuswertGbs
{
	/// <summary>
	/// Wii 2014.00.27 in Layer "l_abovemain" gesictet.
	/// </summary>
	public class SictAuswertGbsPanelEveMenu : SictAuswertGbsPanelGroup
	{
		public SictAuswertGbsPanelEveMenu(UINodeInfoInTree PanelGroupAst)
			:
			base(PanelGroupAst,
			"main")
		{
		}

		override	public	void Berecne()
		{
			base.Berecne();
		}
	}
}
