using Bib3;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Accumulation
{
	public interface IShipUiModuleAndContext
	{
		MemoryStruct.IShipUiModule Module { get; }

		Vektor2DInt? Position { get; }
	}

	public interface IShipUiModule : IEntityWithHistory<IShipUiModuleAndContext>, IEntityScoring<IShipUiModuleAndContext, Parse.IMemoryMeasurement>
	{
		FieldGenMitIntervalInt64<Parse.IModuleButtonTooltip> TooltipLast { get; }
	}
}
