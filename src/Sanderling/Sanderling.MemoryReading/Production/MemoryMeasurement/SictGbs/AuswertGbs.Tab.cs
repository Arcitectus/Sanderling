using BotEngine.Interface;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsTab
	{
		readonly public SictGbsAstInfoSictAuswert TabAst;

		public SictGbsAstInfoSictAuswert LabelClipperAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert LabelAst
		{
			private set;
			get;
		}

		public string LabelText
		{
			private set;
			get;
		}

		public ColorORGB LabelColor
		{
			private set;
			get;
		}

		public Tab Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertGbsTab(SictGbsAstInfoSictAuswert tabAst)
		{
			this.TabAst = tabAst;
		}

		virtual public void Berecne()
		{
			if (null == TabAst)
				return;

			if (!(true == TabAst.SictbarMitErbe))
				return;

			LabelAst = TabAst.GröösteLabel(3);

			if (null == LabelAst)
				return;

			LabelColor = ColorORGB.VonVal(LabelAst.Color);
			LabelText = LabelAst.LabelText();

			if (null == LabelText || null == LabelColor)
				return;

			var LabelColorOpazitäätMili = LabelColor.OMilli;

			var Label = new UIElementText(LabelAst.AlsUIElementFalsUnglaicNullUndSictbar(), LabelText);

			Ergeebnis = new Tab(TabAst.AlsUIElementFalsUnglaicNullUndSictbar())
			{
				Label = Label,
				LabelColorOpacityMilli = LabelColorOpazitäätMili,
			};
		}
	}
}
