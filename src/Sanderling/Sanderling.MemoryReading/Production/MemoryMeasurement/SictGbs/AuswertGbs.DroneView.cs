using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sanderling.Interface.MemoryStruct;
using BotEngine.Common;
using Bib3.Geometrik;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindowDroneView : SictAuswertGbsWindow
	{
		new static public WindowDroneView BerecneFürWindowAst(
			UINodeInfoInTree windowNode)
		{
			if (null == windowNode)
				return null;

			var WindowAuswert = new SictAuswertGbsWindowDroneView(windowNode);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisScpez;
		}

		UINodeInfoInTree ListViewportAst;

		SictAuswertGbsListViewport<IListEntry> ListViewportAuswert;

		public WindowDroneView ErgeebnisScpez
		{
			private set;
			get;
		}

		public SictAuswertGbsWindowDroneView(UINodeInfoInTree windowNode)
			:
			base(windowNode)
		{
		}

		/// <summary>
		/// 2014.02.25	Bsp:
		/// "gauge_shield"
		/// "gauge_armor"
		/// "gauge_struct"
		/// </summary>
		static readonly string DroneEntryGaugeScpezAstNameRegexPattern = "gauge_(\\w+)";

		static int? AusDroneEntryGaugeTreferpunkteRelMili(
			UINodeInfoInTree droneEntryGaugeAst)
		{
			var MengeFillAst =
				droneEntryGaugeAst.MatchingNodesFromSubtreeBreadthFirst(
				kandidaat => true == kandidaat.VisibleIncludingInheritance && "Fill".EqualsIgnoreCase(kandidaat.PyObjTypName),
				null,
				1, 1,
				true);

			if (null == MengeFillAst)
				return null;

			var BarDamageNictAst =
				MengeFillAst
				?.Where(kandidaat => "droneGaugeBar".EqualsIgnoreCase(kandidaat.Name))
				?.FirstOrDefault();

			var BarDamageAst =
				MengeFillAst
				?.Where(kandidaat => "droneGaugeBarDmg".EqualsIgnoreCase(kandidaat.Name))
				?.FirstOrDefault();

			if (null == BarDamageNictAst || null == BarDamageAst)
				return null;

			var BarDamageAstGrööse = BarDamageAst.Grööse;
			var BarDamageNictAstGrööse = BarDamageNictAst.Grööse;

			if (!BarDamageAstGrööse.HasValue || !BarDamageNictAstGrööse.HasValue)
				return null;

			var TreferpunkteAntail = (int)BarDamageNictAstGrööse.Value.A;
			var TreferpunkteNictAntail = (int)BarDamageAstGrööse.Value.A;

			var GesamtGrööse = TreferpunkteAntail + TreferpunkteNictAntail;

			if (GesamtGrööse < 1)
				return null;

			var TreferpunkteNormiirtMili = (TreferpunkteAntail * 1000) / GesamtGrööse;

			return TreferpunkteNormiirtMili;
		}

		/// <summary>
		/// 2015.07.28
		/// Bescriftung für Item welces meerere Drone repräsentiirt:
		/// "Hobgoblin I (2)"
		/// </summary>
		/// <param name="entryAst"></param>
		/// <param name="listeScrollHeader"></param>
		/// <returns></returns>
		static public DroneViewEntry DroneEntryKonstrukt(
			UINodeInfoInTree entryAst,
			IColumnHeader[] listeScrollHeader,
			RectInt? regionConstraint)
		{
			if (!(entryAst?.VisibleIncludingInheritance ?? false))
				return null;

			var listEntryAuswert = new SictAuswertGbsListEntry(entryAst, listeScrollHeader, regionConstraint, ListEntryTrenungZeleTypEnum.Ast);

			listEntryAuswert.Berecne();

			var listEntry = listEntryAuswert.ErgeebnisListEntry;

			if (null == listEntry)
				return null;

			var LabelGröösteAst = entryAst?.LargestLabelInSubtree();

			var labelGrööste = LabelGröösteAst.AsUIElementTextIfTextNotEmpty();

			var isGroup = listEntry?.IsGroup ?? false;

			if (isGroup)
			{
				var Caption = labelGrööste;

				return new DroneViewEntryGroup(listEntry)
				{
					Caption = labelGrööste,
				};
			}
			else
			{
				var MengeContainerAst =
					entryAst.MatchingNodesFromSubtreeBreadthFirst(
					kandidaat => kandidaat.PyObjTypNameIsContainer(),
					null,
					3, 1);

				var GaugesAst =
					MengeContainerAst.SuuceFlacMengeAstFrüheste(
					kandidaat => string.Equals("gauges", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
					1, 0);

				var MengeGaugeScpezContainerAst =
					GaugesAst.MatchingNodesFromSubtreeBreadthFirst(
					kandidaat => kandidaat.PyObjTypNameIsContainer(),
					null,
					1, 1,
					true);

				var DictZuTypSictStringTreferpunkte = new Dictionary<string, int?>();

				if (null != MengeGaugeScpezContainerAst)
				{
					foreach (var GaugeScpezContainerAst in MengeGaugeScpezContainerAst)
					{
						if (null == GaugeScpezContainerAst)
							continue;

						var GaugeScpezContainerAstName = GaugeScpezContainerAst.Name;

						if (null == GaugeScpezContainerAstName)
							continue;

						var nameMatch = GaugeScpezContainerAstName?.RegexMatchIfSuccess(DroneEntryGaugeScpezAstNameRegexPattern, RegexOptions.IgnoreCase);

						var typSictString = nameMatch?.Groups[1].Value;

						if (null == typSictString)
							continue;

						DictZuTypSictStringTreferpunkte[typSictString] = AusDroneEntryGaugeTreferpunkteRelMili(GaugeScpezContainerAst);
					}
				}

				var TreferpunkteStruct = DictZuTypSictStringTreferpunkte.FirstOrDefault(kandidaat => kandidaat.Key.ToLower().Contains("struct"));
				var TreferpunkteArmor = DictZuTypSictStringTreferpunkte.FirstOrDefault(kandidaat => kandidaat.Key.ToLower().Contains("armor"));
				var TreferpunkteShield = DictZuTypSictStringTreferpunkte.FirstOrDefault(kandidaat => kandidaat.Key.ToLower().Contains("shield"));

				var Treferpunkte = new ShipHitpointsAndEnergy
				{
					Struct = TreferpunkteStruct.Value,
					Armor = TreferpunkteArmor.Value,
					Shield = TreferpunkteShield.Value,
				};

				return new DroneViewEntryItem(listEntry)
				{
					Hitpoints = Treferpunkte,
				};
			}
		}

		override public void Berecne()
		{
			base.Berecne();

			if (null == Ergeebnis)
				return;

			ListViewportAst =
				AstMainContainerMain?.MatchingNodesFromSubtreeBreadthFirst(kandidaat => kandidaat?.PyObjTypNameIsScroll() ?? false)?.LargestNodeInSubtree();

			ListViewportAuswert = new SictAuswertGbsListViewport<IListEntry>(ListViewportAst, DroneEntryKonstrukt);

			ListViewportAuswert.Read();

			ErgeebnisScpez = new WindowDroneView(Ergeebnis)
			{
				ListView = ListViewportAuswert?.Result,
			};
		}
	}
}
