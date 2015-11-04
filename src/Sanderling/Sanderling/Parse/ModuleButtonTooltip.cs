using Bib3;
using Bib3.Geometrik;
using BotEngine.Common;
using System;
using System.Linq;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Parse
{
	public interface IModuleButtonTooltip : MemoryStruct.IContainer
	{
		bool? IsWeapon { get; }
		bool? IsTargetPainter { get; }
		bool? IsAfterburner { get; }
		bool? IsMicroWarpDrive { get; }
		bool? IsMicroJumpDrive { get; }
		bool? IsShieldBooster { get; }
		bool? IsArmorRepairer { get; }
		bool? IsHardener { get; }
		bool? IsMiner { get; }
		bool? IsSurveyScanner { get; }

		int? RangeOptimal { get; }
		int? RangeMax { get; }
		int? RangeFalloff { get; }
	}

	public class ModuleButtonTooltip : IModuleButtonTooltip
	{
		static readonly string[] IsWeaponSetIndicatorLabelRegexPattern = new string[]{
			"Missile Launcher",
			"Rocket Launcher",
			"AutoCannon",
			"Railgun",
			"Gatling Rail",
			"Ion Blaster",
			"Neutron Blaster",
			"Electron Blaster",
			"Particle Accelerator",
			"Beam Laser",
			"Pulse Laser",
			"Cannon",
			"Artillery",
			"Howitzer",
		};

		static readonly string[] IsTargetPainterSetIndicatorLabelRegexPattern = new string[]{
			@"Target\s*Painter",
		};

		static readonly string[] IsHardenerSetIndicatorLabelRegexPattern = new string[]{
			@"Armor.*Hardener",
			@"(Invulnerability|Ward|Deflection|Dissipation)\s*Field",
			@"Damage\s*Control",
		};

		static readonly string[] IsShieldBoosterSetIndicatorLabelRegexPattern = new string[]{
			@"Shield\s*Boost",
		};

		static readonly string[] IsArmorRepairerSetIndicatorLabelRegexPattern = new string[]{
			@"Armor\s*Repair",
		};

		static readonly string[] IsMinerSetIndicatorLabelRegexPattern = new string[]{
			@"Miner",
			@"Mining\s*Laser",
		};

		MemoryStruct.IContainer Raw;

		public MemoryStruct.IUIElementText[] ButtonText => Raw?.ButtonText;

		public long Id => Raw?.Id ?? 0;

		public MemoryStruct.IUIElementInputText[] InputText => Raw?.InputText;

		public int? InTreeIndex => Raw?.InTreeIndex;

		public MemoryStruct.IUIElementText[] LabelText => Raw?.LabelText;

		public RectInt Region => Raw?.Region ?? RectInt.Empty;

		public MemoryStruct.IUIElement RegionInteraction => Raw?.RegionInteraction;

		public MemoryStruct.ISprite[] Sprite => Raw?.Sprite;

		public bool? IsWeapon { private set; get; }

		public bool? IsTargetPainter { private set; get; }

		public bool? IsAfterburner { private set; get; }

		public bool? IsMicroWarpDrive { private set; get; }

		public bool? IsMicroJumpDrive { private set; get; }

		public bool? IsShieldBooster { private set; get; }

		public bool? IsArmorRepairer { private set; get; }

		public bool? IsHardener { private set; get; }

		public bool? IsMiner { private set; get; }

		public bool? IsSurveyScanner { private set; get; }

		public int? RangeOptimal { private set; get; }

		public int? RangeMax { private set; get; }

		public int? RangeFalloff { private set; get; }

		public int? ChildLastInTreeIndex => Raw?.ChildLastInTreeIndex;

		public ModuleButtonTooltip(MemoryStruct.IContainer Raw)
		{
			this.Raw = Raw;

			if (null == Raw)
			{
				return;
			}

			var LabelRegexMatchSuccessIgnoreCase = new Func<string, bool>(
				Pattern => Raw?.LabelText?.Any(Label => Label?.Text?.RegexMatchSuccess(Pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ?? false) ?? false);

			var LabelAnyRegexMatchSuccessIgnoreCase = new Func<string[], bool>(
				SetPattern => SetPattern?.Any(LabelRegexMatchSuccessIgnoreCase) ?? false);

			IsWeapon = LabelAnyRegexMatchSuccessIgnoreCase(IsWeaponSetIndicatorLabelRegexPattern);

			IsTargetPainter = LabelAnyRegexMatchSuccessIgnoreCase(IsTargetPainterSetIndicatorLabelRegexPattern);

			IsHardener = LabelAnyRegexMatchSuccessIgnoreCase(IsHardenerSetIndicatorLabelRegexPattern);

			IsShieldBooster = LabelAnyRegexMatchSuccessIgnoreCase(IsShieldBoosterSetIndicatorLabelRegexPattern);
			IsArmorRepairer = LabelAnyRegexMatchSuccessIgnoreCase(IsArmorRepairerSetIndicatorLabelRegexPattern);

			IsMiner = LabelAnyRegexMatchSuccessIgnoreCase(IsMinerSetIndicatorLabelRegexPattern);
			IsSurveyScanner = LabelRegexMatchSuccessIgnoreCase(@"Survey\s*Scan");
		}
	}
}
