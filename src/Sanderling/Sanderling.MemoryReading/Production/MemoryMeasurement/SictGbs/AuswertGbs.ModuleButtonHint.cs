using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsModuleButtonTooltip
	{
		public SictGbsAstInfoSictAuswert ModuleButtonHintAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert[] MengeCellAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert[][] ListeZaileMengeCellAst
		{
			private set;
			get;
		}

		public IContainer Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertGbsModuleButtonTooltip(SictGbsAstInfoSictAuswert moduleButtonHintNode)
		{
			this.ModuleButtonHintAst = moduleButtonHintNode;
		}

		/// <summary>
		/// 2014.04.12	Bsp:
		/// "Row0_Col0"
		/// </summary>
		static readonly string CellAstNameRegexPattern = "Row(\\d+)_Col(\\d+)";

		static public SictGbsAstInfoSictAuswert[][] ListeZaileMengeCellAstBerecneAusMengeAus(
			IEnumerable<SictGbsAstInfoSictAuswert> setNode)
		{
			if (null == setNode)
				return null;

			var listeCellAstMitRowIndexUndColumnIndex = new List<KeyValuePair<SictGbsAstInfoSictAuswert, KeyValuePair<int, int>>>();

			foreach (var node in setNode)
			{
				if (null == node)
					continue;

				var nodeNameRegexMatch = Regex.Match(node.Name ?? "", CellAstNameRegexPattern, RegexOptions.IgnoreCase);

				if (!nodeNameRegexMatch.Success)
					continue;

				var rowIndex = nodeNameRegexMatch.Groups[1].Value?.TryParseInt();
				var colIndex = nodeNameRegexMatch.Groups[2].Value?.TryParseInt();

				if (!rowIndex.HasValue || !colIndex.HasValue)
					continue;

				listeCellAstMitRowIndexUndColumnIndex.Add(
					new KeyValuePair<SictGbsAstInfoSictAuswert, KeyValuePair<int, int>>(
					node, new KeyValuePair<int, int>(rowIndex.Value, colIndex.Value)));
			}

			var listeGrupeRow =
				listeCellAstMitRowIndexUndColumnIndex
				.GroupBy((kandidaat) => kandidaat.Value.Key)
				.OrderBy((grupeRow) => grupeRow.Key)
				.ToArray();

			return
				listeGrupeRow
				?.Select((grupeRow) => grupeRow.Select((cellUndIndex) => cellUndIndex.Key)?.ToArray())
				?.ToArray();
		}

		static public KeyValuePair<Int64, string>? AusGbsAstIconMitTextIconIdentUndText(
			SictGbsAstInfoSictAuswert gbsAst)
		{
			if (null == gbsAst)
				return null;

			var iconAst =
				gbsAst?.SuuceFlacMengeAstFrüheste(AuswertGbs.Glob.GbsAstTypeIstEveIcon, 2, 1);

			var textAst =
				gbsAst?.SuuceFlacMengeAstFrüheste(AuswertGbs.Glob.GbsAstTypeIstEveLabel, 2, 1);

			var iconIdentNulbar = iconAst?.HerkunftAdrese;

			if (!iconIdentNulbar.HasValue)
				return null;

			var text = textAst?.LabelText();

			return new KeyValuePair<Int64, string>(iconIdentNulbar.Value, text);
		}

		public void Berecne()
		{
			if (!(ModuleButtonHintAst?.SictbarMitErbe ?? false))
				return;

			var icon =
				ModuleButtonHintAst.SuuceFlacMengeAst(k => k.PyObjTypNameIsIcon())
				?.Select(k => k.AlsSprite())
				?.ToArray();

			var mengePfaad =
				ModuleButtonHintAst.SuuceFlacMengeAstMitPfaad(t => true)
				?.ToArray();

			var mengePfaadIcon =
				mengePfaad?.Where(path => path?.LastOrDefault()?.PyObjTypNameIsIcon() ?? false)
				?.ToArray();

			var mengeIconSprite =
				mengePfaadIcon?.Select(path => path?.LastOrDefault()?.AlsSprite())
				?.ToArray();

			var listLabelString =
				ModuleButtonHintAst?.ExtraktMengeLabelString()
				?.OrdnungLabel()
				?.ToArrayIfNotEmpty();

			MengeCellAst =
				ModuleButtonHintAst?.SuuceFlacMengeAst((t) => true, null, 2, 1, true);

			ListeZaileMengeCellAst = ListeZaileMengeCellAstBerecneAusMengeAus(MengeCellAst);

			var containerBase = ModuleButtonHintAst.AlsContainer();

			var labelText = ModuleButtonHintAst.ExtraktMengeLabelString()?.OrdnungLabel()?.ToArray();

			Ergeebnis = new Container(ModuleButtonHintAst.AlsUIElementFalsUnglaicNullUndSictbar())
			{
				LabelText = labelText,
				Sprite = icon,
			};
		}
	}
}
