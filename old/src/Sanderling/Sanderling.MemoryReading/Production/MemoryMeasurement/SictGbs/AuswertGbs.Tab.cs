using BotEngine.Interface;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsTab
	{
		readonly public UINodeInfoInTree TabAst;

		public UINodeInfoInTree LabelClipperAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree LabelAst
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

		public SictAuswertGbsTab(UINodeInfoInTree tabAst)
		{
			this.TabAst = tabAst;
		}

		virtual public void Berecne()
		{
			if (null == TabAst)
				return;

			if (!(true == TabAst.VisibleIncludingInheritance))
				return;

			LabelAst = TabAst.LargestLabelInSubtree(3);

			if (null == LabelAst)
				return;

			LabelColor = ColorORGB.VonVal(LabelAst.Color);
			LabelText = LabelAst.LabelText();

			if (null == LabelText || null == LabelColor)
				return;

			var LabelColorOpazitäätMili = LabelColor.OMilli;

			var Label = new UIElementText(LabelAst.AsUIElementIfVisible(), LabelText);

			Ergeebnis = new Tab(TabAst.AsUIElementIfVisible())
			{
				Label = Label,
				LabelColorOpacityMilli = LabelColorOpazitäätMili,
			};
		}
	}
}
