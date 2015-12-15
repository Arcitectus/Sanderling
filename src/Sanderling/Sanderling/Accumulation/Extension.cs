using Bib3.Geometrik;
using System;
using System.Collections.Generic;
using System.Linq;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Accumulation
{
	static public class Extension
	{
		static public bool IsActive(this IShipUiModule ModuleAccu) =>
			ModuleAccu?.LastInstant?.Value?.Module?.RampActive ?? false;

		static public Vektor2DInt? PositionInShipUi(
			this MemoryStruct.IShipUiModule Module, MemoryStruct.IShipUi ShipUi) =>
			Module?.RegionCenter() - ShipUi?.Center?.RegionCenter();

		static public IEnumerable<IShipUiModule> WhereTooltip(
			this IEnumerable<IShipUiModule> Source,
			Func<Parse.IModuleButtonTooltip, bool> TooltipPredicate) =>
			Source?.Where(Module => TooltipPredicate(Module?.TooltipLast?.Value));

		static public T BestRead<T>(
			this IEnumerable<T> SetRead,
			Func<T, MemoryStruct.IUIElement> GetUIElementFromRead)
			where T : class =>
			SetRead?.OrderByDescending(Read => MemoryStruct.Extension.EnumerateReferencedUIElementTransitive(
				GetUIElementFromRead?.Invoke(Read))?.Count() ?? -1)?.FirstOrDefault();
	}
}
