using System;
using System.Text.RegularExpressions;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindowStack : SictAuswertGbsWindow
	{
		new static public WindowStack BerecneFürWindowAst(
			SictGbsAstInfoSictAuswert windowAst)
		{
			if (null == windowAst)
				return null;

			var WindowAuswert = new SictAuswertGbsWindowStack(windowAst);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisScpezStack;
		}

		public SictGbsAstInfoSictAuswert TabGroupAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert ContentAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert KandidaatWindowAktiivAst
		{
			private set;
			get;
		}

		public SictAuswertGbsTabGroup TabGroupAuswert
		{
			private set;
			get;
		}

		public WindowStack ErgeebnisScpezStack
		{
			private set;
			get;
		}

		public SictAuswertGbsWindowStack(SictGbsAstInfoSictAuswert windowStackAst)
			:
			base(windowStackAst)
		{
		}

		override public void Berecne()
		{
			base.Berecne();

			var AstMainContainer = base.AstMainContainer;

			if (null == AstMainContainer)
				return;

			if (!(true == AstMainContainer.SictbarMitErbe))
				return;

			TabGroupAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMainContainer, (kandidaat) =>
					string.Equals("TabGroup", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
					4, 1);

			ContentAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstMainContainer, (kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					Regex.Match(kandidaat.Name ?? "", "content", RegexOptions.IgnoreCase).Success,
					2, 1);

			KandidaatWindowAktiivAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				ContentAst, (kandidaat) =>
					true == kandidaat.SictbarMitErbe && null != kandidaat.Caption,
				2, 1);

			if (null != TabGroupAst)
			{
				TabGroupAuswert = new SictAuswertGbsTabGroup(TabGroupAst);
				TabGroupAuswert.Berecne();
			}

			var TabGroup =
				(null == TabGroupAuswert) ? null : TabGroupAuswert.Ergeebnis;

			var WindowAktiiv = AuswertGbs.Glob.WindowBerecneScpezTypFürGbsAst(KandidaatWindowAktiivAst);

			ErgeebnisScpezStack = new WindowStack(base.Ergeebnis)
			{
				Tab = TabGroup?.ListTab,
				TabSelectedWindow = WindowAktiiv,
			};
		}
	}

}
