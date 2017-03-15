using Sanderling.Interface.MemoryStruct;
using BotEngine.EveOnline.Sensor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Optimat.EveOnline.AuswertGbs
{
	/// <summary>
	/// Wii 2013.07.25 in Layer "l_abovemain" gesictet.
	/// </summary>
	public class SictAuswertGbsMessage
	{
		readonly public SictGbsAstInfoSictAuswert AstMessage;

		public SictGbsAstInfoSictAuswert AstLabel
		{
			private set;
			get;
		}

		public string AstLabelSetText
		{
			private set;
			get;
		}

		public string AstLabelText
		{
			private set;
			get;
		}

		public IUIElementText Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertGbsMessage(SictGbsAstInfoSictAuswert AstMessage)
		{
			this.AstMessage = AstMessage;
		}

		public void Berecne()
		{
			if (null == AstMessage)
			{
				return;
			}

			if (!(true == AstMessage.SictbarMitErbe))
			{
				return;
			}

			AstLabel =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMessage, (Kandidaat) => string.Equals("Label", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			if (null != AstLabel)
			{
				AstLabelSetText = AstLabel.SetText;
			}

			this.Ergeebnis = new UIElementText(AstMessage.AlsUIElementFalsUnglaicNullUndSictbar()) { Text = AstLabelText };
		}
	}
}
