using System.Linq;
using Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;
using System.Text.RegularExpressions;
using BotEngine.Interface;
using System;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsMenu
	{
		public const string MenuEntryPyTypeName = "MenuEntryView";

		static public Menu ReadMenu(SictGbsAstInfoSictAuswert menuNode)
		{
			if (!(menuNode?.SictbarMitErbe ?? false))
				return null;

			var setEntryNode =
				menuNode.SuuceFlacMengeAst(
				kandidaat => kandidaat?.PyObjTypNameMatchesRegexPatternIgnoreCase(MenuEntryPyTypeName) ?? false,
				null, 3, 1);

			var baseElement = menuNode.AlsUIElementFalsUnglaicNullUndSictbar();

			var setEntry =
				setEntryNode
				?.Select(kandidaatAst => ReadMenuEntry(kandidaatAst, baseElement?.Region ?? RectInt.Empty)).ToArray();

			var listEntry = setEntry?.OrdnungLabel()?.ToArray();

			return new Menu(baseElement)
			{
				Entry = listEntry,
			};
		}

		static public MenuEntry ReadMenuEntry(
			SictGbsAstInfoSictAuswert entryNode,
			RectInt regionConstraint)
		{
			if (!(entryNode?.SictbarMitErbe ?? false))
				return null;

			var fillAst =
				entryNode.SuuceFlacMengeAstFrüheste(kandidaat => string.Equals("Fill", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1) ??
				entryNode.SuuceFlacMengeAstFrüheste(kandidaat => Regex.Match(kandidaat.PyObjTypName ?? "", "Underlay", RegexOptions.IgnoreCase).Success, 2, 1);

			var fillColor = fillAst == null ? null : ColorORGB.VonVal(fillAst.Color);

			var entryHighlight =
				null != fillColor ? (200 < fillColor.OMilli) : (bool?)null;

			return entryNode.MenuEntry(regionConstraint, entryHighlight);
		}
	}
}
