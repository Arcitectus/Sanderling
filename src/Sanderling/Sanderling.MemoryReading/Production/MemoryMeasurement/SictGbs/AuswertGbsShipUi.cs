using System;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;
using BotEngine.EveOnline.Sensor.Option.MemoryMeasurement.SictGbs;

namespace Optimat.EveOnline.AuswertGbs
{
	public class HitpointsAbsAndRel
	{
		public int? Max;

		public int? Current;

		public int? NormalizedMilli;
	}

	public class SictAuswertGbsShipUiEWarElement
	{
		readonly public SictGbsAstInfoSictAuswert EWarElementAst;

		public SictGbsAstInfoSictAuswert EWarButtonAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert IconAst
		{
			private set;
			get;
		}

		public ShipUiEWarElement Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertGbsShipUiEWarElement(SictGbsAstInfoSictAuswert eWarElementNode)
		{
			this.EWarElementAst = eWarElementNode;
		}

		public void Berecne()
		{
			EWarButtonAst =
				EWarElementAst?.SuuceFlacMengeAstFrüheste((kandidaat) => string.Equals("EwarButton", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			IconAst =
				EWarButtonAst?.SuuceFlacMengeAstFrüheste(kandidaat =>
					(string.Equals("Icon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) ||
					string.Equals("EveIcon", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase)),
					2, 1);

			if (!(IconAst?.SictbarMitErbe ?? false))
				return; //	Annaame diise EWar Anzaige isc nit aktiiv.

			var EWarTypeString = EWarElementAst?.Name;

			this.Ergeebnis = new ShipUiEWarElement()
			{
				EWarType = EWarTypeString,
				IconTexture = IconAst?.TextureIdent0?.AsObjectIdInMemory(),
			};
		}
	}

	public class SictAuswertGbsShipUi
	{
		/// <summary>
		/// 2015.09.01: "readoutCont"
		/// </summary>
		static Regex ReadoutContainerAstNameRegex = @"readout".AlsRegexIgnoreCaseCompiled();

		readonly public SictGbsAstInfoSictAuswert LayerShipUiNode;

		public SictGbsAstInfoSictAuswert ShipUIContainerAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert EwarUIContainerAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert[] EwarUIContainerMengeEWarElementKandidaatAst
		{
			private set;
			get;
		}

		public SictAuswertGbsShipUiEWarElement[] EwarUIContainerMengeEWarElementKandidaatAuswert
		{
			private set;
			get;
		}

		public ShipUiEWarElement[] EwarUIContainerMengeEWarElementKandidaatAuswertErgeebnis
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert ContainerPowerCoreAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert[] ContainerPowerCoreMengeMarkAst
		{
			private set;
			get;
		}

		public int? ContainerPowerCoreMengeMarkAinAnzaal
		{
			private set;
			get;
		}

		public int? ContainerPowerCoreMengeMarkAusAnzaal
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstIndicationContainer
		{
			private set;
			get;
		}

		public SictAuswertGbsShipUiSlots AuswertSlots
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert ButtonStopAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert TimersContainerAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert[] MengeTimerKandidaatAst
		{
			private set;
			get;
		}

		public ShipUi Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertGbsShipUi(SictGbsAstInfoSictAuswert layerShipUiNode)
		{
			this.LayerShipUiNode = layerShipUiNode;
		}

		static public string AusShipUiGaugeHintTextTailProzentRegexPattern = "\\:\\s*(\\d+)\\s*\\%";

		static public string AusShipUiGaugeHintTextTailTotalRegexPattern = "(\\d+) left of maximum (\\d+)";

		static public IShipUiTimer AlsTimer(SictGbsAstInfoSictAuswert node)
		{
			var container = node?.AlsContainer();

			if (null == container)
				return null;

			return new ShipUiTimer(container)
			{
				Name = node?.Name,
			};
		}

		public void Berecne()
		{
			if (!(LayerShipUiNode?.SictbarMitErbe ?? false))
				return;

			ShipUIContainerAst =
				LayerShipUiNode?.SuuceFlacMengeAstFrüheste(kandidaat =>
				string.Equals("ShipUIContainer", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) ||
				string.Equals("ShipHudContainer", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),    //	2015.05.00	Singularity
				2, 1);

			EwarUIContainerAst =
				LayerShipUiNode?.SuuceFlacMengeAstFrüheste(kandidaat =>
				string.Equals("EwarUIContainer", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) ||
				string.Equals("EwarContainer", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),    //	2015.05.00	Singularity
				2, 1);

			TimersContainerAst =
				LayerShipUiNode?.SuuceFlacMengeAstFrüheste(kandidaat =>
					kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("timers", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			var CapacitorContainerAst =
				LayerShipUiNode?.SuuceFlacMengeAstFrüheste(k => k.PyObjTypNameEqualsIgnoreCase("CapacitorContainer"));

			MengeTimerKandidaatAst =
				TimersContainerAst?.ListeChild;

			EwarUIContainerMengeEWarElementKandidaatAst =
				EwarUIContainerAst?.SuuceFlacMengeAst(kandidaat => true,
				null, 2, 1);

			EwarUIContainerMengeEWarElementKandidaatAuswert =
				EwarUIContainerMengeEWarElementKandidaatAst
				?.Select(kandidaatAst =>
					{
						var Auswert = new SictAuswertGbsShipUiEWarElement(kandidaatAst);
						Auswert.Berecne();
						return Auswert;
					}).ToArray();

			EwarUIContainerMengeEWarElementKandidaatAuswertErgeebnis =
				EwarUIContainerMengeEWarElementKandidaatAuswert
				?.Select(auswert => auswert.Ergeebnis)
				.WhereNotDefault()
				.ToArray();

			ContainerPowerCoreAst =
				ShipUIContainerAst?.SuuceFlacMengeAstFrüheste(kandidaat => string.Equals("powercore", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			var ContainerPowerCoreHint = ContainerPowerCoreAst?.Hint;

			ContainerPowerCoreMengeMarkAst =
				ContainerPowerCoreAst?.SuuceFlacMengeAst(kandidaat =>
					string.Equals("Sprite", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) &&
					string.Equals("pmark", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				null, 2, 1);

			if (null != ContainerPowerCoreMengeMarkAst)
			{
				ContainerPowerCoreMengeMarkAinAnzaal =
					ContainerPowerCoreMengeMarkAst
					?.Count((node) => 700 < node?.Color.Value.OMilli);
			}

			var FensterGaugeReadout =
				LayerShipUiNode?.SuuceFlacMengeAstFrüheste(kandidaat => string.Equals("gaugeReadout", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase));

			var UnderMainAst =
				LayerShipUiNode?.SuuceFlacMengeAstFrüheste(kandidaat => string.Equals("underMain", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase));

			var SpeedNeedleAst =
				UnderMainAst?.SuuceFlacMengeAstFrüheste((kandidaat) =>
					string.Equals("Transform", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) &&
					Regex.Match(kandidaat.Name ?? "", "speedNeedle", RegexOptions.IgnoreCase).Success);

			var HPGaugesAst =
				LayerShipUiNode?.SuuceFlacMengeAstFrüheste(
					k => string.Equals("HPGauges", k?.PyObjTypName, StringComparison.InvariantCultureIgnoreCase)) ?? LayerShipUiNode;

			var SpeedGaugeAst =
				LayerShipUiNode?.SuuceFlacMengeAstFrüheste(
					k => string.Equals("SpeedGauge", k?.PyObjTypName, StringComparison.InvariantCultureIgnoreCase)) ?? LayerShipUiNode;

			ButtonStopAst =
				LayerShipUiNode?.SuuceFlacMengeAstFrüheste(k => k?.NameEqualsIgnoreCase("stopButton") ?? false) ??
				//	2015.08.26 Singularity
				LayerShipUiNode?.SuuceFlacMengeAstFrüheste(k => k?.PyObjTypNameEqualsIgnoreCase("StopButton") ?? false);

			var ReadoutContainerAst =
				LayerShipUiNode?.SuuceFlacMengeAstFrüheste(k => (k?.PyObjTypNameIsContainer() ?? false) && (k?.NameMatchesRegex(ReadoutContainerAstNameRegex) ?? false));

			var ReadoutLabel =
				ReadoutContainerAst.ExtraktMengeLabelString()?.OrdnungLabel()?.ToArray();

			var SpeedLabel = SpeedGaugeAst?.GröösteLabel();

			var StructureGaugeSpriteAst =
				HPGaugesAst?.SuuceFlacMengeAstFrüheste(kandidaat =>
					string.Equals("structureGauge", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase));

			var ArmorGaugeSpriteAst =
				HPGaugesAst?.SuuceFlacMengeAstFrüheste(kandidaat =>
					string.Equals("armorGauge", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase));

			var ShieldGaugeSpriteAst =
				HPGaugesAst?.SuuceFlacMengeAstFrüheste(kandidaat =>
				   string.Equals("shieldGauge", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase));

			var StructureGaugeSpriteHint = StructureGaugeSpriteAst?.Hint;
			var ArmorGaugeSpriteHint = ArmorGaugeSpriteAst?.Hint;
			var ShieldGaugeSpriteHint = ShieldGaugeSpriteAst?.Hint;

			var SlotsAst =
				LayerShipUiNode?.SuuceFlacMengeAstFrüheste(kandidaat => string.Equals("slotsContainer", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 4) ??
				//	2015.08.25 Beobact Singularity
				LayerShipUiNode?.SuuceFlacMengeAstFrüheste(k => k.PyObjTypNameEqualsIgnoreCase("SlotsContainer"));

			AstIndicationContainer =
				LayerShipUiNode?.SuuceFlacMengeAstFrüheste(kandidaat => string.Equals("indicationContainer", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 4, 1);

			var indication = AstIndicationContainer?.AlsContainer();

			AuswertSlots = new SictAuswertGbsShipUiSlots(SlotsAst);

			AuswertSlots.Berecne();

			var MengeModule = AuswertSlots.ListModuleButton;

			ShipHitpointsAndEnergy ShipTreferpunkte = null;

			var ContainerPowerCoreMengeMarkAstAnzaal = ContainerPowerCoreMengeMarkAst?.Count();

			int? CapacitorCapacityNormiirtMili = null;

			if (0 < ContainerPowerCoreMengeMarkAstAnzaal)
			{
				CapacitorCapacityNormiirtMili = (ContainerPowerCoreMengeMarkAinAnzaal * 1000) / (int?)ContainerPowerCoreMengeMarkAstAnzaal;
			}

			CapacitorCapacityNormiirtMili = CapacitorCapacityNormiirtMili ?? ((int?)(CapacitorContainerAst?.LastSetCapacitorFloat * 1e+3));

			ShipTreferpunkte = new ShipHitpointsAndEnergy()
			{
				Struct = (int?)((LayerShipUiNode.StructureLevel ?? StructureGaugeSpriteAst?.LastValueFloat) * 1e+3),
				Armor = (int?)((LayerShipUiNode.ArmorLevel ?? ArmorGaugeSpriteAst?.LastValueFloat) * 1e+3),
				Shield = (int?)((LayerShipUiNode.ShieldLevel ?? ShieldGaugeSpriteAst?.LastValueFloat) * 1e+3),
				Capacitor = CapacitorCapacityNormiirtMili,
			};

			var ButtonStop =
				ButtonStopAst?.AlsUIElementFalsUnglaicNullUndSictbar();

			var ButtonSpeedMax =
				LayerShipUiNode?.SuuceFlacMengeAstFrüheste(k => k?.PyObjTypNameEqualsIgnoreCase("MaxSpeedButton") ?? false)
				.AlsUIElementFalsUnglaicNullUndSictbar();

			var ListeTimer =
				MengeTimerKandidaatAst?.Select(AlsTimer)?.OrdnungLabel()?.ToArray();

			var squadronsUINode =
				LayerShipUiNode
				?.SuuceFlacMengeAstFrüheste(node => node.PyObjTypNameMatchesRegexPatternIgnoreCase("SquadronsUI"));

			Ergeebnis = new ShipUi(null)
			{
				Center = (ContainerPowerCoreAst ?? CapacitorContainerAst).AlsUIElementFalsUnglaicNullUndSictbar().WithRegionSizePivotAtCenter(new Vektor2DInt(40, 40)),
				Indication = indication,
				HitpointsAndEnergy = ShipTreferpunkte,
				SpeedLabel = SpeedLabel?.AsUIElementTextIfTextNotEmpty(),
				EWarElement = EwarUIContainerMengeEWarElementKandidaatAuswertErgeebnis,
				Timer = ListeTimer,
				ButtonSpeed0 = ButtonStop,
				ButtonSpeedMax = ButtonSpeedMax,
				Module = MengeModule,
				SpeedMilli = (Int64?)(LayerShipUiNode?.Speed * 1e+3),
				Readout = ReadoutLabel,
				SquadronsUI = squadronsUINode?.AsSquadronsUI(),
			};
		}
	}
}
