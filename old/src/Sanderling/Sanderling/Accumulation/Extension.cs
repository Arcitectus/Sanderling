using Bib3.Geometrik;
using System;
using System.Collections.Generic;
using System.Linq;
using MemoryStruct = Sanderling.Interface.MemoryStruct;
using Sanderling.Interface.MemoryStruct;

namespace Sanderling.Accumulation
{
	static public class Extension
	{
		static public bool IsActive(this IShipUiModule moduleAccu) =>
			moduleAccu?.LastInstant?.Value?.Module?.RampActive ?? false;

		static public Vektor2DInt? PositionInShipUi(
			this MemoryStruct.IShipUiModule module, MemoryStruct.IShipUi shipUi) =>
			module?.RegionCenter() - shipUi?.Center?.RegionCenter();

		static public IEnumerable<IShipUiModule> WhereTooltip(
			this IEnumerable<IShipUiModule> source,
			Func<Parse.IModuleButtonTooltip, bool> tooltipPredicate) =>
			source?.Where(module => tooltipPredicate(module?.TooltipLast?.Value));

		static public T BestRead<T>(
			this IEnumerable<T> setRead,
			Func<T, MemoryStruct.IUIElement> getUIElementFromRead)
			where T : class =>
			setRead?.OrderByDescending(read => MemoryStruct.Extension.EnumerateReferencedUIElementTransitive(
				getUIElementFromRead?.Invoke(read))?.Count() ?? -1)?.FirstOrDefault();
	}
}
