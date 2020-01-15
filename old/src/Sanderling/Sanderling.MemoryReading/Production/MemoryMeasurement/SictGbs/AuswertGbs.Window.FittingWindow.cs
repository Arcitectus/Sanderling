using System;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using BotEngine.Interface;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindowFittingWindowDefenceStatsRowCellDamageType
	{
		readonly public UINodeInfoInTree CellAst;

		public SictAuswertGbsWindowFittingWindowDefenceStatsRowCellDamageType(UINodeInfoInTree cellAst)
		{
			this.CellAst = cellAst;
		}

		public UINodeInfoInTree LabelAst
		{
			private set;
			get;
		}

		public string LabelAstText
		{
			private set;
			get;
		}

		public Int64? ResistanceMili
		{
			private set;
			get;
		}

		public ColorORGB DamageTypColor
		{
			private set;
			get;
		}

		static readonly string ResistanceRegexPattern = "([\\d]+)\\s*" + Regex.Escape("%");

		public void Berecne()
		{
			LabelAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				CellAst, (kandidaat) =>
					string.Equals("EveLabelSmall", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			if (null != LabelAst)
			{
				LabelAstText = LabelAst.SetText;
			}

			var ResistanceLabelMatch = Regex.Match(LabelAstText ?? "", ResistanceRegexPattern);

			if (ResistanceLabelMatch.Success)
			{
				ResistanceMili = ResistanceLabelMatch.Groups[1].Value?.TryParseInt64(Bib3.Glob.NumberFormat) * 10;
			}

			var MengeFillAst =
				Optimat.EveOnline.AuswertGbs.Extension.MatchingNodesFromSubtreeBreadthFirst(
				CellAst, (kandidaat) =>
					string.Equals("PyFill", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
					null, 2, 1);

			if (null != MengeFillAst)
			{
				var MengeFillAstColor =
					MengeFillAst
					.Select((ast) => ast.Color)
					.Where((color) => null == color ? false : color.Value.AleUnglaicNul())
					.ToArray();

				DamageTypColor =
					ColorORGB.VonVal(
					MengeFillAstColor
					.OrderBy((color) => color.Value.OMilli ?? 0)
					.LastOrDefault());
			}
		}
	}

	public class SictAuswertGbsWindowShipFitting : SictAuswertGbsWindow
	{
		new static public WindowShipFitting	BerecneFürWindowAst(
			UINodeInfoInTree windowAst)
		{
			if (null == windowAst)
				return null;

			var WindowAuswert = new SictAuswertGbsWindowShipFitting(windowAst);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisScpez;
		}

		public UINodeInfoInTree FittingAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree SlotParentAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree RightSideAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] MengeFittingSlotAst
		{
			private set;
			get;
		}

		public WindowShipFitting ErgeebnisScpez
		{
			private set;
			get;
		}

		public SictAuswertGbsWindowShipFitting(UINodeInfoInTree windowAst)
			:
			base(windowAst)
		{
		}

		override public void Berecne()
		{
			base.Berecne();

			if (null == base.Ergeebnis)
				return;

			FittingAst =
			Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
			AstMainContainerMain, (kandidaat) =>
				string.Equals("Fitting", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			SlotParentAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				FittingAst, (kandidaat) =>
					kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("slotParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			ErgeebnisScpez = new WindowShipFitting(base.Ergeebnis);
		}
	}
}
