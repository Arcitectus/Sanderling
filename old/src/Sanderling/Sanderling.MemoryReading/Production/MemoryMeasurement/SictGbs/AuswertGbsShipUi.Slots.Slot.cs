using System;
using System.Linq;
using System.Text.RegularExpressions;
using Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;
using BotEngine.Common;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsShipModuleButtonRamps
	{
		readonly public UINodeInfoInTree shipModuleButtonRampsNode;

		public UINodeInfoInTree LeftRampAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree RightRampAst
		{
			private set;
			get;
		}

		public double? LeftRampRotation
		{
			private set;
			get;
		}

		public double? RightRampRotation
		{
			private set;
			get;
		}

		public bool RampAktiiv
		{
			private set;
			get;
		}

		public int? RotatioonMili
		{
			private set;
			get;
		}


		public SictAuswertGbsShipModuleButtonRamps(
			UINodeInfoInTree shipModuleButtonRampsNode)
		{
			this.shipModuleButtonRampsNode = shipModuleButtonRampsNode;
		}

		static public bool RampRotatioonInGültigeBeraic(
			double rampRotation,
			out bool inRegioonAnim)
		{
			inRegioonAnim = false;

			if (!(0 <= rampRotation &&
				rampRotation <= Math.PI + 1e-2))
			{
				return false;
			}

			inRegioonAnim =
				1e-2 < rampRotation &&
				rampRotation < Math.PI - 1e-2;

			return true;
		}

		static public int? RotatioonMiliAusLeftRampUndRightRamp(
			double leftRampRotation,
			double rightRampRotation,
			out bool rampAktiiv)
		{
			bool LeftRampRotationInRegioonAnim;
			bool RightRampRotationInRegioonAnim;
			rampAktiiv = false;

			if (!RampRotatioonInGültigeBeraic(leftRampRotation, out LeftRampRotationInRegioonAnim))
				return null;

			if (!RampRotatioonInGültigeBeraic(rightRampRotation, out RightRampRotationInRegioonAnim))
				return null;

			if (LeftRampRotationInRegioonAnim && RightRampRotationInRegioonAnim)
			{
				//	Normaalerwaise isc nur aine von baiden Animiirt.
				rampAktiiv = true;
				return null;
			}

			var RotatioonMili = ((((int)(1000 - ((leftRampRotation + rightRampRotation) * 500) / Math.PI)) % 1000) + 1000) % 1000;

			rampAktiiv = 0 < Math.Abs(RotatioonMili);

			return RotatioonMili;
		}

		public void Berecne()
		{
			var shipModuleButtonRampsNode = this.shipModuleButtonRampsNode;

			if (!(shipModuleButtonRampsNode?.VisibleIncludingInheritance ?? false))
				return;

			var MengeTransformAst =
				shipModuleButtonRampsNode?.MatchingNodesFromSubtreeBreadthFirst(
				(kandidaat) => string.Equals("Transform", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 4, 1, true);

			LeftRampAst =
				MengeTransformAst?.SuuceFlacMengeAstFrüheste(
				(kandidaat) => string.Equals("leftRamp", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 1, 0);

			RightRampAst =
				MengeTransformAst?.SuuceFlacMengeAstFrüheste(
				(kandidaat) => string.Equals("rightRamp", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 1, 0);

			if (null == LeftRampAst || null == RightRampAst)
				return;

			LeftRampRotation = LeftRampAst.RotationFloat;
			RightRampRotation = RightRampAst.RotationFloat;

			/*
			2015.08.26
			Beobactung Singularity: aine Ramp hat Rotatioon grööser 0 und andere null.

			if (LeftRampRotation.HasValue && RightRampRotation.HasValue)
			*/
			if (0 < LeftRampRotation || 0 < RightRampRotation)
			{
				bool tRampAktiiv;

				RotatioonMili = RotatioonMiliAusLeftRampUndRightRamp(
					LeftRampRotation ?? 0,
					RightRampRotation ?? 0,
					out tRampAktiiv);

				this.RampAktiiv = tRampAktiiv;
			}
		}
	}

	public class SictAuswertGbsShipUiSlotsSlot
	{
		const string overloadOffHintRegexPattern = @"Turn\s*On\s*Overload";
		const string overloadOnHintRegexPattern = @"Turn\s*Off\s*Overload";
		public const string ModuleButtonPyTypeName = "ModuleButton";

		readonly public UINodeInfoInTree slotNode;

		public UINodeInfoInTree ModuleButtonAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree ModuleButtonIconAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree ModuleButtonQuantityAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree ModuleButtonQuantityLabelAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstMainShape
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] MengeKandidaatRampAst
		{
			private set;
			get;
		}

		public SictAuswertGbsShipModuleButtonRamps[] MengeKandidaatRampAuswert
		{
			private set;
			get;
		}

		public UINodeInfoInTree SpriteHiliteAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree SpriteGlowAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree SpriteBusyAst
		{
			private set;
			get;
		}

		public ShipUiModule ModuleRepr
		{
			private set;
			get;
		}

		public SictAuswertGbsShipUiSlotsSlot(UINodeInfoInTree slotNode)
		{
			this.slotNode = slotNode;
		}

		public void Berecne()
		{
			if (!(slotNode?.VisibleIncludingInheritance ?? false))
				return;

			ModuleButtonAst =
				slotNode?.FirstMatchingNodeFromSubtreeBreadthFirst(kandidaat => string.Equals(ModuleButtonPyTypeName, kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 1, 1);

			ModuleButtonIconAst =
				ModuleButtonAst?.FirstMatchingNodeFromSubtreeBreadthFirst((kandidaat) =>
					(string.Equals("Icon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) ||
					string.Equals("EveIcon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase)),
					1, 1);

			ModuleButtonQuantityAst =
				ModuleButtonAst?.FirstMatchingNodeFromSubtreeBreadthFirst((kandidaat) => string.Equals("quantityParent", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 1, 1);

			ModuleButtonQuantityLabelAst =
				ModuleButtonQuantityAst?.FirstMatchingNodeFromSubtreeBreadthFirst((kandidaat) => string.Equals("Label", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 1, 1);

			var MengeSpriteAst =
				slotNode?.MatchingNodesFromSubtreeBreadthFirst((kandidaat) =>
					true == kandidaat.VisibleIncludingInheritance &&
					string.Equals("Sprite", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 1, 1);

			SpriteHiliteAst =
				(null == MengeSpriteAst) ? null :
				MengeSpriteAst.FirstOrDefault((kandidaat) => string.Equals("hilite", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase));

			SpriteGlowAst =
				(null == MengeSpriteAst) ? null :
				MengeSpriteAst.FirstOrDefault((kandidaat) => string.Equals("glow", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase));

			SpriteBusyAst =
				(null == MengeSpriteAst) ? null :
				MengeSpriteAst.FirstOrDefault((kandidaat) => string.Equals("busy", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase));

			MengeKandidaatRampAst =
				slotNode?.MatchingNodesFromSubtreeBreadthFirst((kandidaat) =>
					true == kandidaat.VisibleIncludingInheritance &&
					Regex.Match(kandidaat.PyObjTypName ?? "", "ramps", RegexOptions.IgnoreCase).Success, 1, 1);

			MengeKandidaatRampAuswert =
				MengeKandidaatRampAst?.Select((kandidaatRampAst) =>
					{
						var Auswert = new SictAuswertGbsShipModuleButtonRamps(kandidaatRampAst);
						Auswert.Berecne();
						return Auswert;
					}).ToArray();

			var RampAuswert =
				MengeKandidaatRampAuswert
				?.FirstOrDefault((kandidaat) => null != kandidaat.LeftRampAst || kandidaat.RampAktiiv);

			AstMainShape =
				slotNode?.FirstMatchingNodeFromSubtreeBreadthFirst((kandidaat) => string.Equals("mainshape", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 1, 1);

			if (null == AstMainShape)
				return;

			bool? SpriteHiliteSictbar = null;

			bool? SpriteGlowSictbar = null;
			bool? SpriteBusySictbar = null;

			if (null != SpriteHiliteAst)
				SpriteHiliteSictbar = true == SpriteHiliteAst.VisibleIncludingInheritance;

			if (null != SpriteGlowAst)
				SpriteGlowSictbar = true == SpriteGlowAst.VisibleIncludingInheritance;

			if (null != SpriteBusyAst)
				SpriteBusySictbar = true == SpriteBusyAst.VisibleIncludingInheritance;

			var ModuleButtonSictbar = ModuleButtonAst?.VisibleIncludingInheritance;

			var ModuleButtonFlächeToggle =
				ModuleButtonAst.AsUIElementIfVisible().WithRegionSizePivotAtCenter(new Vektor2DInt(16, 16));

			var ModuleButtonIconTextureIdent = ModuleButtonIconAst?.TextureIdent0;

			var rampActive = ModuleButtonAst?.RampActive ?? RampAuswert?.RampAktiiv ?? false;
			var	rampRotationMilli = RampAuswert?.RotatioonMili;

			bool? overloadOn = null;

			var overloadButton =
				slotNode?.FirstMatchingNodeFromSubtreeBreadthFirst(node => node.PyObjTypNameIsSprite() && (node?.Name?.RegexMatchSuccessIgnoreCase("OverloadB(tn|utton)") ?? false));

			if (null != overloadButton)
			{
				if (overloadButton?.Hint?.RegexMatchSuccessIgnoreCase(overloadOffHintRegexPattern) ?? false)
					overloadOn = false;

				if (overloadButton?.Hint?.RegexMatchSuccessIgnoreCase(overloadOnHintRegexPattern) ?? false)
					overloadOn = true;
			}

			var ModuleRepr = new ShipUiModule(slotNode.AsUIElementIfVisible())
			{
				ModuleButtonVisible = ModuleButtonSictbar,
				ModuleButtonIconTexture = ModuleButtonIconTextureIdent?.AsObjectIdInMemory(),
				ModuleButtonQuantity = ModuleButtonQuantityLabelAst?.SetText,
				RampActive = rampActive,
				RampRotationMilli = rampRotationMilli,
				HiliteVisible = SpriteHiliteSictbar,
				GlowVisible = SpriteGlowSictbar,
				BusyVisible = SpriteBusySictbar,
				OverloadOn = overloadOn,
			};

			this.ModuleRepr = ModuleRepr;
		}
	}
}
