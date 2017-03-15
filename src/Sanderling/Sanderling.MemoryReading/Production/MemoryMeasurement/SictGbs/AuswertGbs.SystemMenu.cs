using System;
using Sanderling.Interface.MemoryStruct;
using BotEngine.Common;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsSystemMenu
	{
		readonly public SictGbsAstInfoSictAuswert SystemMenuAst;

		public SictGbsAstInfoSictAuswert AstMainContainer
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstHeaderButtonClose
		{
			private set;
			get;
		}

		public IWindow Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertGbsSystemMenu(SictGbsAstInfoSictAuswert systemMenuAst)
		{
			this.SystemMenuAst = systemMenuAst;
		}

		virtual public void Berecne()
		{
			var SystemMenuAst = this.SystemMenuAst;

			if (null == SystemMenuAst)
				return;

			if (!(true == SystemMenuAst.SictbarMitErbe))
				return;

			AstMainContainer =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				SystemMenuAst, (kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					"sysmenu".EqualsIgnoreCase(kandidaat.Name),
					2, 1);

			if (!(AstMainContainer?.SictbarMitErbe ?? false))
				return;

			AstHeaderButtonClose =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMainContainer, (kandidaat) =>
					string.Equals("Icon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) &&
					true == kandidaat.SictbarMitErbe,
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
