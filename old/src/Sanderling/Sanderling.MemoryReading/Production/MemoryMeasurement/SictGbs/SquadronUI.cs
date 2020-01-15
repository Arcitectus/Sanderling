using Bib3;
using Bib3.Geometrik;
using BotEngine.Common;
using Optimat.EveOnline.AuswertGbs;
using Sanderling.Interface.MemoryStruct;
using System;
using System.Linq;

namespace BotEngine.EveOnline.Sensor.Option.MemoryMeasurement.SictGbs
{
	static public class SquadronUIExtension
	{
		public const string FightersHealthGaugePyTypeName = "FightersHealthGauge";

		public const string AbilityIconPyTypeName = "AbilityIcon";

		static public ISquadronsUI AsSquadronsUI(this UINodeInfoInTree squadronsNode)
		{
			if (!(squadronsNode?.VisibleIncludingInheritance ?? false))
				return null;

			var squadronsContainer = squadronsNode?.AlsContainer();

			var setSquadronUINode =
				squadronsNode?.MatchingNodesFromSubtreeBreadthFirst(node => node.PyObjTypNameMatchesRegexPatternIgnoreCase("SquadronUI"));

			var buttonFromPyTypeName = new Func<string, IUIElement>(pyTypeNameRegexPattern =>
				squadronsNode?.FirstMatchingNodeFromSubtreeBreadthFirst(node => node.PyObjTypNameMatchesRegexPatternIgnoreCase(pyTypeNameRegexPattern))?.AsUIElementIfVisible());

			return new SquadronsUI(squadronsContainer)
			{
				SetSquadron = setSquadronUINode?.Select(node => node?.AsSquadronUI())?.WhereNotDefault()?.OrderBy(squadronUI => squadronUI?.RegionCenter()?.A)?.ToArrayIfNotEmpty(),
				LaunchAllButton = buttonFromPyTypeName("ButtonLaunchAll"),
				OpenBayButton = buttonFromPyTypeName("ButtonOpenBay"),
				RecallAllButton = buttonFromPyTypeName("ButtonRecallAll"),
			};
		}

		static public ISquadronUI AsSquadronUI(this UINodeInfoInTree squadronUINode)
		{
			if (!(squadronUINode?.VisibleIncludingInheritance ?? false))
				return null;

			return new SquadronUI(squadronUINode?.AsUIElementIfVisible())
			{
				SetAbilityIcon =
					squadronUINode?.MatchingNodesFromSubtreeBreadthFirst(node => node.PyObjTypNameMatchesRegexPatternIgnoreCase(AbilityIconPyTypeName))
					?.Select(AsSquadronAbilityIcon)?.WhereNotDefault()?.OrderByCenterVerticalDown()?.ToArrayIfNotEmpty(),

				Squadron =
					squadronUINode?.FirstMatchingNodeFromSubtreeBreadthFirst(node => (node?.VisibleIncludingInheritance ?? false) && node.PyObjTypNameMatchesRegexPatternIgnoreCase("SquadronCont"))?.AsSquadronContainer(),
			};
		}

		static public ISquadronAbilityIcon AsSquadronAbilityIcon(this UINodeInfoInTree squadronAbilityIconNode)
		{
			if (!(squadronAbilityIconNode?.VisibleIncludingInheritance ?? false))
				return null;

			var quantityLabel =
				squadronAbilityIconNode
				?.FirstMatchingNodeFromSubtreeBreadthFirst(node => node.PyObjTypNameIsContainer() && (node.Name?.RegexMatchSuccessIgnoreCase("quantityParent") ?? false))
				?.LargestLabelInSubtree();

			return new SquadronAbilityIcon(squadronAbilityIconNode.AsUIElementIfVisible().WithRegionSizeBoundedMaxPivotAtCenter(new Vektor2DInt(26, 26)))
			{
				Quantity = quantityLabel?.LabelText()?.Trim()?.TryParseInt(),
				RampActive = squadronAbilityIconNode?.RampActive,
			};
		}

		static public ISquadronContainer AsSquadronContainer(this UINodeInfoInTree squadronContainerNode)
		{
			if (!(squadronContainerNode?.VisibleIncludingInheritance ?? false))
				return null;

			var squadronNumberLabel =
				squadronContainerNode?.FirstMatchingNodeFromSubtreeBreadthFirst(node => node.PyObjTypNameMatchesRegexPatternIgnoreCase("SquadronNumber"))
				?.LargestLabelInSubtree();

			var isSelected =
				squadronContainerNode
				?.FirstMatchingNodeFromSubtreeBreadthFirst(n => n?.Name?.RegexMatchSuccessIgnoreCase("SelectHilight") ?? false)
				?.VisibleIncludingInheritance;

			return new SquadronContainer(squadronContainerNode.AlsContainer())
			{
				SquadronNumber = squadronNumberLabel?.LabelText()?.TryParseInt(),
				Health = squadronContainerNode?.FirstMatchingNodeFromSubtreeBreadthFirst(node => node.PyObjTypNameMatchesRegexPatternIgnoreCase(FightersHealthGaugePyTypeName)).AsSquadronHealth(),
				IsSelected = isSelected,
				Hint = squadronContainerNode?.Hint,
			};
		}

		static public ISquadronHealth AsSquadronHealth(this UINodeInfoInTree squadronHealthNode)
		{
			if (!(squadronHealthNode?.VisibleIncludingInheritance ?? false))
				return null;

			return new SquadronHealth
			{
				SquadronSizeCurrent = squadronHealthNode?.SquadronSize,
				SquadronSizeMax = squadronHealthNode?.SquadronMaxSize,
			};
		}
	}
}
