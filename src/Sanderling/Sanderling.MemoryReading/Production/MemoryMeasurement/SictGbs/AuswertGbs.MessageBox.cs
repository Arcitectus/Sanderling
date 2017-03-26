using System;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsMessageBox : SictAuswertGbsWindow
	{
		new static public MessageBox BerecneFürWindowAst(
			UINodeInfoInTree windowNode)
		{
			if (null == windowNode)
				return null;

			var WindowAuswert = new SictAuswertGbsMessageBox(windowNode);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisScpez;
		}

		public UINodeInfoInTree AstMainContainerBottom
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstMainContainerTopParent
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstMainContainerTopParentCaption
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstMainContainerBottomButtonGroup
		{
			private set;
			get;
		}

		public MessageBox ErgeebnisScpez
		{
			private set;
			get;
		}

		public SictAuswertGbsMessageBox(UINodeInfoInTree windowNode)
			:
			base(windowNode)
		{
		}

		override public void Berecne()
		{
			base.Berecne();

			var Ergeebnis = base.Ergeebnis;

			if (null == Ergeebnis)
				return;

			var AstMainContainer = base.AstMainContainer;

			AstMainContainerTopParent =
				AstMainContainer?.FirstMatchingNodeFromSubtreeBreadthFirst((kandidaat) => string.Equals("topParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			AstMainContainerTopParentCaption =
				AstMainContainerTopParent?.FirstMatchingNodeFromSubtreeBreadthFirst((kandidaat) => string.Equals("EveCaptionLarge", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			AstMainContainerBottom =
				AstMainContainer?.FirstMatchingNodeFromSubtreeBreadthFirst((kandidaat) => string.Equals("bottom", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			AstMainContainerBottomButtonGroup =
				AstMainContainerBottom?.FirstMatchingNodeFromSubtreeBreadthFirst((kandidaat) =>
					(string.Equals("EveButtonGroup", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) ||
					string.Equals("ButtonGroup", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase)),
				2, 1);

			if (null == AstMainContainerBottomButtonGroup)
				return;

			var TopCaptionText =
				(null == AstMainContainerTopParentCaption) ? null : AstMainContainerTopParentCaption.LabelText();

			string MainEditText = null;

			ErgeebnisScpez = new MessageBox(Ergeebnis)
			{
				TopCaptionText = TopCaptionText,
				MainEditText = MainEditText,
			};
		}
	}
}
