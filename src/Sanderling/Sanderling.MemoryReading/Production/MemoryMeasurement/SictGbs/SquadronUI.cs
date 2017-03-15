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

		static public ISquadronsUI AsSquadronsUI(this SictGbsAstInfoSictAuswert squadronsNode)
		{
			if (!(squadronsNode?.SictbarMitErbe ?? false))
				return null;

			var squadronsContainer = squadronsNode?.AlsContainer();

			var setSquadronUINode =
				squadronsNode?.SuuceFlacMengeAst(node => node.PyObjTypNameMatchesRegexPatternIgnoreCase("SquadronUI"));

			var buttonFromPyTypeName = new Func<string, IUIElement>(pyTypeNameRegexPattern =>
				squadronsNode?.SuuceFlacMengeAstFrüheste(node => node.PyObjTypNameMatchesRegexPatternIgnoreCase(pyTypeNameRegexPattern))?.AlsUIElementFalsUnglaicNullUndSictbar());

			return new SquadronsUI(squadronsContainer)
			{
				SetSquadron = setSquadronUINode?.Select(node => node?.AsSquadronUI())?.WhereNotDefault()?.OrderBy(squadronUI => squadronUI?.RegionCenter()?.A)?.ToArrayIfNotEmpty(),
				LaunchAllButton = buttonFromPyTypeName("ButtonLaunchAll"),
				OpenBayButton = buttonFromPyTypeName("ButtonOpenBay"),
				RecallAllButton = buttonFromPyTypeName("ButtonRecallAll"),
			};
		}

		static public ISquadronUI AsSquadronUI(this SictGbsAstInfoSictAuswert squadronUINode)
		{
			if (!(squadronUINode?.SictbarMitErbe ?? false))
				return null;

			return new SquadronUI(squadronUINode?.AlsUIElementFalsUnglaicNullUndSictbar())
			{
				SetAbilityIcon =
					squadronUINode?.SuuceFlacMengeAst(node => node.PyObjTypNameMatchesRegexPatternIgnoreCase(AbilityIconPyTypeName))
					?.Select(AsSquadronAbilityIcon)?.WhereNotDefault()?.OrderByCenterVerticalDown()?.ToArrayIfNotEmpty(),

				Squadron =
					squadronUINode?.SuuceFlacMengeAstFrüheste(node => (node?.SictbarMitErbe ?? false) && node.PyObjTypNameMatchesRegexPatternIgnoreCase("SquadronCont"))?.AsSquadronContainer(),
			};
		}

		static public ISquadronAbilityIcon AsSquadronAbilityIcon(this SictGbsAstInfoSictAuswert squadronAbilityIconNode)
		{
			if (!(squadronAbilityIconNode?.SictbarMitErbe ?? false))
				return null;

			var quantityLabel =
				squadronAbilityIconNode
				?.SuuceFlacMengeAstFrüheste(node => node.PyObjTypNameIsContainer() && (node.Name?.RegexMatchSuccessIgnoreCase("quantityParent") ?? false))
				?.GröösteLabel();

			return new SquadronAbilityIcon(squadronAbilityIconNode.AlsUIElementFalsUnglaicNullUndSictbar().WithRegionSizeBoundedMaxPivotAtCenter(new Vektor2DInt(26, 26)))
			{
				Quantity = quantityLabel?.LabelText()?.Trim()?.TryParseInt(),
				RampActive = squadronAbilityIconNode?.RampActive,
			};
		}

		static public ISquadronContainer AsSquadronContainer(this SictGbsAstInfoSictAuswert squadronContainerNode)
		{
			if (!(squadronContainerNode?.SictbarMitErbe ?? false))
				return null;

			var squadronNumberLabel =
				squadronContainerNode?.SuuceFlacMengeAstFrüheste(node => node.PyObjTypNameMatchesRegexPatternIgnoreCase("SquadronNumber"))
				?.GröösteLabel();

			var isSelected =
				squadronContainerNode
				?.SuuceFlacMengeAstFrüheste(n => n?.Name?.RegexMatchSuccessIgnoreCase("SelectHilight") ?? false)
				?.SictbarMitErbe;

			return new SquadronContainer(squadronContainerNode.AlsContainer())
			{
				SquadronNumber = squadronNumberLabel?.LabelText()?.TryParseInt(),
				Health = squadronContainerNode?.SuuceFlacMengeAstFrüheste(node => node.PyObjTypNameMatchesRegexPatternIgnoreCase(FightersHealthGaugePyTypeName)).AsSquadronHealth(),
				IsSelected = isSelected,
				Hint = squadronContainerNode?.Hint,
			};
		}

		static public ISquadronHealth AsSquadronHealth(this SictGbsAstInfoSictAuswert squadronHealthNode)
		{
			if (!(squadronHealthNode?.SictbarMitErbe ?? false))
				return null;

			return new SquadronHealth
			{
				SquadronSizeCurrent = squadronHealthNode?.SquadronSize,
				SquadronSizeMax = squadronHealthNode?.SquadronMaxSize,
			};
		}
	}
}
