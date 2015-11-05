using Bib3;
using Bib3.Geometrik;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Accumulation
{
	public interface IShipUiModuleAndContext
	{
		MemoryStruct.IShipUiModule Module { get; }

		Vektor2DInt? Location { get; }
	}

	public interface IShipUiModule : IEntityWithHistory<IShipUiModuleAndContext>, IEntityScoring<IShipUiModuleAndContext, Parse.IMemoryMeasurement>, MemoryStruct.IShipUiModule, IRepresentingMemoryObject
	{
		PropertyGenTimespanInt64<Parse.IModuleButtonTooltip> TooltipLast { get; }
	}
}
