using System;
using Sanderling.Interface.MemoryStruct;
using BotEngine.Common;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsSystemMenu
	{
		readonly public UINodeInfoInTree SystemMenuAst;

		public UINodeInfoInTree AstMainContainer
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstHeaderButtonClose
		{
			private set;
			get;
		}

		public IWindow Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertGbsSystemMenu(UINodeInfoInTree systemMenuAst)
		{
			this.SystemMenuAst = systemMenuAst;
		}

		virtual public void Berecne()
		{
			var SystemMenuAst = this.SystemMenuAst;

			if (null == SystemMenuAst)
				return;

			if (!(true == SystemMenuAst.VisibleIncludingInheritance))
				return;

			AstMainContainer =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				SystemMenuAst, (kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					"sysmenu".EqualsIgnoreCase(kandidaat.Name),
					2, 1);

			if (!(AstMainContainer?.VisibleIncludingInheritance ?? false))
				return;

			AstHeaderButtonClose =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				AstMainContainer, (kandidaat) =>
					string.Equals("Icon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) &&
					true == kandidaat.VisibleIncludingInheritance,
					2, 1);

			var HeaderButtonClose =
				AstHeaderButtonClose?.AlsSprite();

			var ErgeebnisWindow = SystemMenuAst.Window(true, null, null, new[] { HeaderButtonClose });

			this.Ergeebnis = new Window(ErgeebnisWindow)
			{
				isModal = true
			};
		}
	}
}
