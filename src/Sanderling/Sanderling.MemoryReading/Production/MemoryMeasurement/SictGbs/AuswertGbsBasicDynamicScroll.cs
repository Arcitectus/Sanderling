using System;
using System.Linq;
using System.Text.RegularExpressions;
using Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;
using Bib3;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsScroll
	{
		public const string MainContainerUIElementName = "maincontainer";
		public const string ClipperUIElementName = "__clipper";
		public const string ClipperContentUIElementName = "__content";

		readonly public SictGbsAstInfoSictAuswert ScrollNode;

		public SictGbsAstInfoSictAuswert AstMainContainer
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert ScrollHeadersAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert[] MengeKandidaatColumnHeaderAst
		{
			private set;
			get;
		}

		public SictAuswertGbsListColumnHeader[] MengeKandidaatColumnHeaderAuswert
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstMainContainerScrollControls
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstMainContainerScrollControlsScrollHandle
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstMainContainerClipper
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert MainContainerClipperContentAst
		{
			private set;
			get;
		}

		public Scroll Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertGbsScroll(SictGbsAstInfoSictAuswert scrollNode)
		{
			this.ScrollNode = scrollNode;
		}

		static public Scroll Berecne(SictGbsAstInfoSictAuswert scrollAst)
		{
			var Inst = new SictAuswertGbsScroll(scrollAst);
			Inst.Berecne();

			return Inst?.Ergeebnis;
		}

		virtual public void Berecne()
		{
			if (!(ScrollNode?.SictbarMitErbe ?? false))
				return;

			AstMainContainer =
				ScrollNode?.SuuceFlacMengeAstFrüheste((kandidaat) => string.Equals(MainContainerUIElementName, kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 0);

			ScrollHeadersAst =
				ScrollNode?.SuuceFlacMengeAstFrüheste((kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					(Regex.Match(kandidaat.Name ?? "", "scrollHeader", RegexOptions.IgnoreCase).Success) ||
					Regex.Match(kandidaat.PyObjTypName ?? "", "SortHeader", RegexOptions.IgnoreCase).Success ||
					kandidaat.PyObjTypNameMatchesRegexPatternIgnoreCase("ScrollColumnHeader"),
					3, 1);

			MengeKandidaatColumnHeaderAst =
				ScrollHeadersAst?.SuuceFlacMengeAst((kandidaat) =>
					Regex.Match(kandidaat.PyObjTypName ?? "", "ColumnHeader", RegexOptions.IgnoreCase).Success ||
					kandidaat.PyObjTypNameIsContainer(),
					null, 2, 1)
				?.ToArray();

			MengeKandidaatColumnHeaderAuswert =
				MengeKandidaatColumnHeaderAst
				?.Select((ast) =>
				{
					var Auswert = new SictAuswertGbsListColumnHeader(ast);
					Auswert.Berecne();
					return Auswert;
				}).ToArray();

			var ListeColumnHeader =
				MengeKandidaatColumnHeaderAuswert
				?.Select((auswert) => auswert.Ergeebnis)
				?.Where((columnHeader) => null != columnHeader)
				?.Where((columnHeader) => !(columnHeader?.Text).IsNullOrEmpty())
				.TailmengeUnterste(ScrollNode)
				?.OrderBy((columnHeader) => columnHeader.Region.Center().A)
				?.GroupBy(header => header.Id)
				?.Select(headerGroup => headerGroup.FirstOrDefault())
				?.ToArray();

			AstMainContainerScrollControls =
				AstMainContainer?.SuuceFlacMengeAstFrüheste(kandidaat => string.Equals("ScrollControls", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			AstMainContainerScrollControlsScrollHandle =
				AstMainContainerScrollControls?.SuuceFlacMengeAstFrüheste(kandidaat => string.Equals("ScrollHandle", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
				3, 1);

			AstMainContainerClipper =
				AstMainContainer?.SuuceFlacMengeAstFrüheste(kandidaat => string.Equals(ClipperUIElementName, kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			MainContainerClipperContentAst =
				AstMainContainerClipper?.SuuceFlacMengeAstFrüheste(kandidaat =>
					kandidaat.PyObjTypNameIsContainer() ||
					string.Equals(ClipperContentUIElementName, kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			Ergeebnis = new Scroll(ScrollNode.AlsUIElementFalsUnglaicNullUndSictbar())
			{
				ColumnHeader = ListeColumnHeader,
				Clipper = AstMainContainerClipper.AlsUIElementFalsUnglaicNullUndSictbar(),
				ScrollHandleBound = AstMainContainerScrollControls.AlsUIElementFalsUnglaicNullUndSictbar(),
				ScrollHandle = AstMainContainerScrollControlsScrollHandle.AlsUIElementFalsUnglaicNullUndSictbar(),
			};
		}
	}
}
