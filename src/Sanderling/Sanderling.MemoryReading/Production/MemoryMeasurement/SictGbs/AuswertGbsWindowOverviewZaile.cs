using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using BotEngine.Interface;
using Sanderling.Interface.MemoryStruct;
using BotEngine.Common;
using Bib3.Geometrik;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindowOverviewZaile
	{
		public const string MainIconPyTypeName = "SpaceObjectIcon";

		readonly public SictGbsAstInfoSictAuswert WindowOverviewZaile;

		/// <summary>
		/// Diiser wert werd ainfac nur in das Zaileergeebnis kopiirt.
		/// </summary>
		readonly public string WindowOverviewTypeSelectionName;

		readonly public IEnumerable<KeyValuePair<string, KeyValuePair<int, int>>> MengeSortHeaderTitelUndLaage;

		public SictGbsAstInfoSictAuswert AstIconContainer
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert[] MengeFillAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstIconContainerIconMain
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert RightAlignedIconContainerAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert[] RightAlignedIconContainerMengeIconAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstIconContainerIconTargetingIndicator
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstIconContainerIconAttackingMe
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstIconContainerIconHostile
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstIconContainerIconMyActiveTargetIndicator
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstIconContainerIconTargetedByMeIndicator
		{
			private set;
			get;
		}

		public OverviewEntry Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertGbsWindowOverviewZaile(
			SictGbsAstInfoSictAuswert FensterOverviewZaile,
			string WindowOverviewTypeSelectionName,
			IEnumerable<KeyValuePair<string, KeyValuePair<int, int>>> MengeSortHeaderTitelUndLaage)
		{
			this.WindowOverviewZaile = FensterOverviewZaile;
			this.WindowOverviewTypeSelectionName = WindowOverviewTypeSelectionName;
			this.MengeSortHeaderTitelUndLaage = MengeSortHeaderTitelUndLaage;
		}

		public void Berecne()
		{
			var WindowOverviewZaile = this.WindowOverviewZaile;

			if (null == WindowOverviewZaile)
			{
				return;
			}

			if (!(true == WindowOverviewZaile.SictbarMitErbe))
			{
				return;
			}

			var ZaileMengeChild = WindowOverviewZaile.ListeChild;

			if (null == ZaileMengeChild)
			{
				return;
			}

			var ListeLabel =
				ZaileMengeChild
				.Where((Kandidaat) =>
					{
						if (null == Kandidaat)
						{
							return false;
						}

						var KandidaatPyObjTypName = Kandidaat.PyObjTypName;

						return string.Equals("OverviewLabel", KandidaatPyObjTypName, StringComparison.InvariantCultureIgnoreCase);
					})
				.Where((Kandidaat) => Kandidaat.LaageInParent.HasValue)
				.OrderBy((Kandidaat) => Kandidaat.LaageInParent.Value.A)
				.ToArray();

			/*
			 * 2013.10.20
			 * Mit Patch 2013.10.20 Rubikon Änderung Kandidaat.PyObjTypName: "PyFill" -> "Fill"
			 * */
			MengeFillAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAst(
				WindowOverviewZaile,
				(Kandidaat) =>
					(string.Equals("PyFill", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) ||
					string.Equals("Fill", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase)) &&
					true == Kandidaat.SictbarMitErbe &&
					null != Kandidaat.Color,
				null, 2, 1);

			AstIconContainer =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				WindowOverviewZaile, (Kandidaat) => string.Equals(MainIconPyTypeName, Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2);

			RightAlignedIconContainerAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				WindowOverviewZaile, (Kandidaat) => string.Equals("rightAlignedIconContainer", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			RightAlignedIconContainerMengeIconAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAst(
				RightAlignedIconContainerAst, (Kandidaat) =>
					(string.Equals("Icon", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) ||
					string.Equals("EveIcon", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase)),
					null, 2, 1);

			/*
			 * 
			 * 2014.04.12
			 * 
			 * "EVE Online: Rubicon 1.4.4 Released on Tuesday, May 13th, 2014"

			AstIconContainerIconMain =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstIconContainer, (Kandidaat) => string.Equals("mainIcon", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			 * */

			AstIconContainerIconMain =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstIconContainer, (Kandidaat) => string.Equals("iconSprite", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			AstIconContainerIconTargetingIndicator =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstIconContainer, (Kandidaat) => string.Equals("targeting", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			AstIconContainerIconTargetedByMeIndicator =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstIconContainer, (Kandidaat) => string.Equals("targetedByMeIndicator", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			AstIconContainerIconMyActiveTargetIndicator =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstIconContainer, (Kandidaat) => string.Equals("myActiveTargetIndicator", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			AstIconContainerIconAttackingMe =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstIconContainer, (Kandidaat) => string.Equals("attackingMe", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			AstIconContainerIconHostile =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstIconContainer, (Kandidaat) => string.Equals("hostile", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			var ListeZeleGbsAstUndScpalteTitel =
				SictAuswertGbsWindowOverview.MengeGbsAstZuScpalteIdentBerecneAusMengeGbsAstLaageUndMengeScpalteTitelUndLaage(
				MengeSortHeaderTitelUndLaage,
				ListeLabel);

			var ListeZeleBescriftungUndScpalteTitel =
				ListeZeleGbsAstUndScpalteTitel?.Select((ZeleGbsAstUndScpalteTitel) => new KeyValuePair<string, string>(
					ZeleGbsAstUndScpalteTitel.Key.Text, ZeleGbsAstUndScpalteTitel.Value))
				?.ToArray();

			Int64? IconMainTextureIdent = null;
			ColorORGB IconMainColor = null;

			if (null != AstIconContainerIconMain)
			{
				IconMainTextureIdent = AstIconContainerIconMain.TextureIdent0;
				IconMainColor = ColorORGB.VonVal(AstIconContainerIconMain.Color);
			}

			Int64[] RightAlignedIconMengeTextureIdent = null;

			if (null != RightAlignedIconContainerMengeIconAst)
			{
				RightAlignedIconMengeTextureIdent =
					RightAlignedIconContainerMengeIconAst
					.Select((IconAst) => IconAst.TextureIdent0)
					.Where((Kandidaat) => Kandidaat.HasValue)
					.Select((Kandidaat) => Kandidaat.Value)
					.ToArray();
			}
		}

		const string MainIconIndicatorKeyRegexPattern = @"\w+Indicator";

		static public IOverviewEntry OverviewEntryKonstrukt(
			SictGbsAstInfoSictAuswert EntryAst,
			IColumnHeader[] ListeScrollHeader,
			RectInt? regionConstraint)
		{
			if (!(EntryAst?.SictbarMitErbe ?? false))
				return null;

			var ListEntryAuswert = new SictAuswertGbsListEntry(EntryAst, ListeScrollHeader, regionConstraint, ListEntryTrenungZeleTypEnum.Ast);

			ListEntryAuswert.Berecne();

			var ListEntry = ListEntryAuswert.ErgeebnisListEntry;

			if (null == ListEntry)
				return null;

			var MainIconAst =
				EntryAst?.SuuceFlacMengeAstFrüheste(c => c?.PyObjTypNameMatchesRegexPatternIgnoreCase(MainIconPyTypeName) ?? false);

			var RightIconContainer =
				EntryAst?.SuuceFlacMengeAstFrüheste(k =>
				k.PyObjTypNameIsContainer() &&
				//	2015.09.01:	Name = "rightAlignedIconContainer"
				Regex.Match(k?.Name ?? "", "right.*icon", RegexOptions.IgnoreCase).Success);

			var RightIcon =
				RightIconContainer?.SuuceFlacMengeAst(AuswertGbs.Glob.PyObjTypNameIsIcon)
				?.Select(AuswertGbs.Extension.AlsSprite)
				?.WhereNotDefault()
				?.OrdnungLabel()
				?.ToArray();

			var MainIconSetIndicatorName =
				MainIconAst?.DictListKeyStringValueNotEmpty?.Where(key => key.RegexMatchSuccessIgnoreCase(MainIconIndicatorKeyRegexPattern))
				?.ToArrayIfNotEmpty();

			return new OverviewEntry(ListEntry)
			{
				MainIconSetIndicatorName = MainIconSetIndicatorName,
				RightIcon = RightIcon,
			};
		}
	}
}
