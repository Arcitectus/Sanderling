using System;
using System.Collections.Generic;
using System.Linq;
using Bib3;
using Sanderling.Interface.MemoryStruct;
using System.Text.RegularExpressions;
using Bib3.Geometrik;
using BotEngine.Common;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsTarget
	{
		readonly public SictGbsAstInfoSictAuswert TargetAst;

		public SictGbsAstInfoSictAuswert AstBarAndImageCont
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstLabelContainer
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstAssignedPar
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AssignedContainerAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert[] MengeAssignedModuleOderDroneGrupeAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstSymboolActive
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstTargetHealthBars
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstTargetHealthBarsShield
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstTargetHealthBarsArmor
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstTargetHealthBarsHull
		{
			private set;
			get;
		}

		public ShipUiTarget Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertGbsTarget(SictGbsAstInfoSictAuswert targetElement)
		{
			this.TargetAst = targetElement;
		}

		static string AusTargetLabelStringEntferneFormiirung(string stringFormiirt)
		{
			return stringFormiirt?.RemoveXmlTag();
		}

		static public int? AusGbsAstTargetHealthBarTreferpunkteNormiirtMili(SictGbsAstInfoSictAuswert uiElement)
		{
			if (null == uiElement)
				return null;

			var LastStateFloat = uiElement.LastStateFloat;

			if (!LastStateFloat.HasValue)
				return null;

			return (int)(LastStateFloat.Value * 1e+3);
		}

		static ShipUiTargetAssignedGroup AusAstBerecneAssignedModuleOderDroneGrupe(
			SictGbsAstInfoSictAuswert ast)
		{
			if (null == ast)
				return null;

			bool? IstDrone = null;

			var SpriteAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				ast,
				(kandidaat) => string.Equals("Sprite", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
				2,
				1);

			var IconAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				ast,
				AuswertGbs.Glob.GbsAstTypeIstEveIcon,
				2,
				1);

			if (null != SpriteAst)
			{
				var SpriteAstHint = SpriteAst.Hint;

				if (null != SpriteAstHint)
					IstDrone = Regex.Match(SpriteAstHint, "Drone", RegexOptions.IgnoreCase).Success;
			}

			var IconTextureIdent = IconAst?.TextureIdent0;

			var ModuleOderDroneGrupe = new ShipUiTargetAssignedGroup(ast.AlsUIElementFalsUnglaicNullUndSictbar())
			{
				IconTexture = IconTextureIdent?.AsObjectIdInMemory(),
			};

			return ModuleOderDroneGrupe;
		}

		public void Berecne()
		{
			var TargetAst = this.TargetAst;

			if (null == TargetAst)
				return;

			AstBarAndImageCont = Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				TargetAst, (kandidaat) => string.Equals("barAndImageCont", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			AstLabelContainer = Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				TargetAst, (kandidaat) => string.Equals("labelContainer", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			AstAssignedPar = Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				TargetAst, (kandidaat) => string.Equals("assignedPar", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			AssignedContainerAst =
				TargetAst.SuuceFlacMengeAstFrüheste(
				(kandidaat) => kandidaat.PyObjTypNameIsContainer() && string.Equals("assigned", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				3, 1);

			MengeAssignedModuleOderDroneGrupeAst =
				AssignedContainerAst.SuuceFlacMengeAst(
				c => c.PyObjTypNameIsContainer() || c.PyObjTypNameMatchesRegexPatternIgnoreCase("Weapon"),
				null,
				1, 1,
				true);

			AstSymboolActive = Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstBarAndImageCont, (kandidaat) => string.Equals("ActiveTargetOnBracket", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 4);

			AstTargetHealthBars = Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstBarAndImageCont, (kandidaat) => string.Equals("TargetHealthBars", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 4);

			AstTargetHealthBarsShield = Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstTargetHealthBars, (kandidaat) => string.Equals("shieldBar", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			AstTargetHealthBarsArmor = Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstTargetHealthBars, (kandidaat) => string.Equals("armorBar", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			AstTargetHealthBarsHull = Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstTargetHealthBars, (kandidaat) => string.Equals("hullBar", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			var AusLabelContainerMengeLabel =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAst(
				AstLabelContainer, (kandidaat) => string.Equals("EveLabelSmall", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 2);

			if (null == AusLabelContainerMengeLabel)
				return;

			var Treferpunkte = new ShipHitpointsAndEnergy()
			{
				Struct = AusGbsAstTargetHealthBarTreferpunkteNormiirtMili(AstTargetHealthBarsHull),
				Armor = AusGbsAstTargetHealthBarTreferpunkteNormiirtMili(AstTargetHealthBarsArmor),
				Shield = AusGbsAstTargetHealthBarTreferpunkteNormiirtMili(AstTargetHealthBarsShield),
			};

			var AusLabelContainerMengeLabelMitText =
				AusLabelContainerMengeLabel
				.Select((kandidaat) => new KeyValuePair<SictGbsAstInfoSictAuswert, string>(kandidaat, kandidaat.SetText))
				.Where((kandidaat) => null != kandidaat.Value && kandidaat.Key.LaagePlusVonParentErbeLaage().HasValue)
				.ToArray();

			var AusLabelContainerMengeLabelMitTextOrdnet =
				AusLabelContainerMengeLabelMitText
				.OrderBy((kandidaat) => kandidaat.Key.LaagePlusVonParentErbeLaage().Value.B)
				.ToArray();

			if (AusLabelContainerMengeLabelMitTextOrdnet.Length < 2 ||
				4 < AusLabelContainerMengeLabelMitTextOrdnet.Length)
				return;

			var DistanceZaileIndex = AusLabelContainerMengeLabelMitTextOrdnet.Length - 1;

			var DistanceSictStringFormiirt = AusLabelContainerMengeLabelMitTextOrdnet.ElementAtOrDefault(DistanceZaileIndex).Value;

			var OoberhalbDistanceListeZaile =
				AusLabelContainerMengeLabelMitTextOrdnet
				.Take(AusLabelContainerMengeLabelMitTextOrdnet.Length - 1)
				.Select((zaileStringFormiirt) => AusTargetLabelStringEntferneFormiirung(zaileStringFormiirt.Value))
				.Where((zaileBescriftung) => !zaileBescriftung.IsNullOrEmpty())
				.ToArray();

			bool? Active = null;

			if (null != AstSymboolActive)
				Active = null != Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
					AstSymboolActive, (kandidaat) => true == kandidaat.SictbarMitErbe, 2, 1);

			var mengeAssignedModuleOderDroneGrupe =
				MengeAssignedModuleOderDroneGrupeAst
				?.Select((modulOderDroneGrupeAssignedAst) => AusAstBerecneAssignedModuleOderDroneGrupe(modulOderDroneGrupeAssignedAst))
				?.Where((modulOderDroneGrupeAssigned) => null != modulOderDroneGrupeAssigned)
				?.ToArrayIfNotEmpty();

			var RegioonInputFookusSeze =
				AstBarAndImageCont?.AlsUIElementFalsUnglaicNullUndSictbar();

			RegioonInputFookusSeze = RegioonInputFookusSeze?.WithRegionSizePivotAtCenter((RegioonInputFookusSeze.Region.Size() * 7) / 10);

			Ergeebnis = new ShipUiTarget(
				TargetAst.AlsUIElementFalsUnglaicNullUndSictbar())
			{
				LabelText = TargetAst?.ExtraktMengeLabelString()?.OrdnungLabel()?.ToArray(),
				IsSelected = Active,
				Hitpoints = Treferpunkte,
				RegionInteractionElement = RegioonInputFookusSeze,
				Assigned = mengeAssignedModuleOderDroneGrupe,
			};
		}
	}
}
