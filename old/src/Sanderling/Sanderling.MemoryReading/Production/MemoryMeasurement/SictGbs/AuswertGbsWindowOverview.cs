using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindowOverview : SictAuswertGbsWindow
	{
		new static public WindowOverView BerecneFürWindowAst(
			UINodeInfoInTree windowNode)
		{
			if (null == windowNode)
				return null;

			var windowAuswert = new SictAuswertGbsWindowOverview(windowNode);

			windowAuswert.Berecne();

			return windowAuswert.ErgeebnisScpez;
		}

		public string TypeSelectionName
		{
			private set;
			get;
		}

		public UINodeInfoInTree TabGroupAst
		{
			private set;
			get;
		}

		public SictAuswertGbsTabGroup TabGroupAuswert
		{
			private set;
			get;
		}

		public UINodeInfoInTree ScrollAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree ViewportOverallLabelAst
		{
			private set;
			get;
		}

		public WindowOverView ErgeebnisScpez
		{
			private set;
			get;
		}

		public SictAuswertGbsWindowOverview(UINodeInfoInTree windowNode)
			:
			base(windowNode)
		{
		}

		/*
		 * 2013.08.18 Bsp:
		 * "Overview (mission)"
		 * */
		static readonly string AusHeaderCaptionTextTypeSelectionNameRegexPattern = Regex.Escape("Overview (") + "([^" + Regex.Escape(")") + "]+)";

		static readonly public NumberFormatInfo OverviewDistanceNumberFormatInfo = OverviewDistanceNumberFormatInfoErsctele();

		static public NumberFormatInfo OverviewDistanceNumberFormatInfoErsctele()
		{
			var FormatInfo = CultureInfo.InvariantCulture.NumberFormat.Clone() as NumberFormatInfo;

			FormatInfo.NumberGroupSeparator = ".";
			FormatInfo.NumberDecimalSeparator = ",";

			return FormatInfo;
		}

		static public KeyValuePair<UINodeInfoInTree, T>[]
			MengeGbsAstZuScpalteIdentBerecneAusMengeGbsAstLaageUndMengeScpalteTitelUndLaage<T>(
			IEnumerable<KeyValuePair<T, KeyValuePair<int, int>>> mengeScpalteIdentUndLaage,
			IEnumerable<UINodeInfoInTree> mengeLabelAst)
		{
			if (null == mengeLabelAst ||
				null == mengeScpalteIdentUndLaage)
			{
				return null;
			}

			var Liste = new List<KeyValuePair<UINodeInfoInTree, T>>();

			foreach (var LabelAst in mengeLabelAst)
			{
				if (null == LabelAst)
				{
					continue;
				}

				var LabelAstLaage = LabelAst.LaageInParent;
				var LabelAstGrööse = LabelAst.Grööse;

				if (!LabelAstLaage.HasValue)
				{
					continue;
				}

				if (!LabelAstGrööse.HasValue)
				{
					continue;
				}

				var LabelLaageLinx = (int)((LabelAstLaage).Value.A);
				var LabelLaageRecz = (int)((LabelAstLaage + LabelAstGrööse).Value.A);
				var LabelBraite = LabelLaageRecz - LabelLaageLinx;

				var ÜberlapungGrööste =
					BerecneÜberlapungGrööste(
					mengeScpalteIdentUndLaage,
					LabelLaageLinx,
					LabelBraite);

				if (!ÜberlapungGrööste.HasValue)
				{
					continue;
				}

				if (!(0 < ÜberlapungGrööste.Value.Value))
				{
					continue;
				}

				Liste.Add(new KeyValuePair<UINodeInfoInTree, T>(LabelAst, ÜberlapungGrööste.Value.Key));
			}

			return Liste.ToArray();
		}

		static public KeyValuePair<string, KeyValuePair<int, int>>[]
			MengeSortHeaderTitelUndLaageBerecneAusSortHeaderAst(
			UINodeInfoInTree inTabSortHeadersAst)
		{
			if (null == inTabSortHeadersAst)
			{
				return null;
			}

			var Liste = new List<KeyValuePair<string, KeyValuePair<int, int>>>();

			var MengeKandidaatSortHeaderAst =
				inTabSortHeadersAst.MatchingNodesFromSubtreeBreadthFirst(
				(kandidaat) => kandidaat.PyObjTypNameIsContainer(),
				null,
				3,
				1,
				false);

			foreach (var KandidaatSortHeaderAst in MengeKandidaatSortHeaderAst)
			{
				var KandidaatSortHeaderAstLaage = KandidaatSortHeaderAst.LaageInParent;
				var KandidaatSortHeaderAstGrööse = KandidaatSortHeaderAst.Grööse;

				if (!KandidaatSortHeaderAstLaage.HasValue ||
					!KandidaatSortHeaderAstGrööse.HasValue)
				{
					continue;
				}

				var KandidaatSortHeaderAstLaageLinx = (int)((KandidaatSortHeaderAstLaage).Value.A);
				var KandidaatSortHeaderAstLaageRecz = (int)((KandidaatSortHeaderAstLaage + KandidaatSortHeaderAstGrööse).Value.A);
				var KandidaatSortHeaderAstLaageBraite = KandidaatSortHeaderAstLaageRecz - KandidaatSortHeaderAstLaageLinx;

				var LabelAst =
					KandidaatSortHeaderAst.FirstMatchingNodeFromSubtreeBreadthFirst(
					(kandidaatLabelAst) => AuswertGbs.Glob.GbsAstTypeIstLabel(kandidaatLabelAst) && true == kandidaatLabelAst.VisibleIncludingInheritance,
					1);

				var ScpalteTitel = LabelAst?.LabelText();

				if (ScpalteTitel.IsNullOrEmpty())
					continue;

				Liste.Add(new KeyValuePair<string, KeyValuePair<int, int>>(
					ScpalteTitel,
					new KeyValuePair<int, int>(KandidaatSortHeaderAstLaageLinx, KandidaatSortHeaderAstLaageBraite)));
			}

			return Liste.ToArray();
		}

		static public KeyValuePair<T, int>?
			BerecneÜberlapungGrööste<T>(
			IEnumerable<KeyValuePair<T, KeyValuePair<int, int>>> mengeScpalteIdentUndLaageLinxUndBraite,
			int zeleLaageLinx,
			int zeleBraite)
		{
			if (null == mengeScpalteIdentUndLaageLinxUndBraite)
			{
				return null;
			}

			var ZeleLaageRecz = zeleLaageLinx + zeleBraite;

			KeyValuePair<T, int>? BisherGrööste = null;

			foreach (var ScpalteIdentUndLaageLinxUndBraite in mengeScpalteIdentUndLaageLinxUndBraite)
			{
				var ScpalteLaageLinx = ScpalteIdentUndLaageLinxUndBraite.Value.Key;
				var ScpalteLaageRecz = ScpalteLaageLinx + ScpalteIdentUndLaageLinxUndBraite.Value.Value;

				var ÜberlapungLinx = Math.Max(zeleLaageLinx, ScpalteLaageLinx);
				var ÜberlapungRecz = Math.Min(ZeleLaageRecz, ScpalteLaageRecz);

				var ÜberlapungGrööse = ÜberlapungRecz - ÜberlapungLinx;

				if (ÜberlapungGrööse < 1)
				{
					continue;
				}

				if (BisherGrööste.HasValue)
				{
					if (!(BisherGrööste.Value.Value < ÜberlapungGrööse))
					{
						continue;
					}
				}

				BisherGrööste = new KeyValuePair<T, int>(ScpalteIdentUndLaageLinxUndBraite.Key, ÜberlapungGrööse);
			}

			return BisherGrööste;
		}

		override public void Berecne()
		{
			base.Berecne();

			if (null == base.Ergeebnis)
				return;

			var TypeSelectionNameMatch = Regex.Match(HeaderCaptionText ?? "", AusHeaderCaptionTextTypeSelectionNameRegexPattern, RegexOptions.IgnoreCase);

			if (TypeSelectionNameMatch.Success)
			{
				TypeSelectionName = TypeSelectionNameMatch.Groups[1].Value;
			}

			TabGroupAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				AstMainContainerMain, (kandidaat) =>
					string.Equals("TabGroup", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) &&
					string.Equals("tabparent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			if (null != TabGroupAst)
			{
				TabGroupAuswert = new SictAuswertGbsTabGroup(TabGroupAst);
				TabGroupAuswert.Berecne();
			}

			var TabGroup = (null == TabGroupAuswert) ? null : TabGroupAuswert.Ergeebnis;

			ScrollAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				AstMainContainerMain, (kandidaat) =>
					string.Equals("BasicDynamicScroll", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) &&
					string.Equals("overviewscroll2", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			var ListAuswert = new SictAuswertGbsListViewport<IOverviewEntry>(ScrollAst, SictAuswertGbsWindowOverviewZaile.OverviewEntryKonstrukt);

			ListAuswert.Read();

			ViewportOverallLabelAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				ListAuswert.ScrollClipperContentNode, (kandidaat) => AuswertGbs.Glob.GbsAstTypeIstEveCaption(kandidaat));

			var ViewportOverallLabelString =
				(ViewportOverallLabelAst?.VisibleIncludingInheritance ?? false) ? ViewportOverallLabelAst?.LabelText() : null;

			var Ergeebnis = new WindowOverView(base.Ergeebnis)
			{
				PresetTab = TabGroup?.ListTab,
				ListView = ListAuswert?.Result,
				ViewportOverallLabelString = ViewportOverallLabelString,
			};

			this.ErgeebnisScpez = Ergeebnis;
		}

		const string TabNuzbarNitRegexPattern = @"^\s*\+\s*$";

		static public IEnumerable<Tab> ListeTabFiltertNuzbar(
			IEnumerable<Tab> mengeTab)
		{
			return
				mengeTab?.Where((kandidaat) =>
				{
					var TabLabelBescriftung = kandidaat.Label?.Text;

					if (TabLabelBescriftung.IsNullOrEmpty())
						return false;

					if (Regex.Match(TabLabelBescriftung, TabNuzbarNitRegexPattern, RegexOptions.IgnoreCase).Success)
					{
						return false;
					}

					return true;
				});
		}
	}
}
