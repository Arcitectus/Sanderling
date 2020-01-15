using Bib3;

namespace Sanderling.Accumulation
{
	public interface IEntityWithHistory<AccumulatedT>
	{
		int AccumulatedCount { get; }
		PropertyGenTimespanInt64<AccumulatedT> LastInstant { get; }
		PropertyGenTimespanInt64<AccumulatedT> NotDefaultLastInstant { get; }
	}

	public interface IEntityScoring<in AccumulatedT, in SharedT>
	{
		int Score(AccumulatedT instant, SharedT shared);
	}

	public interface IRepresentingMemoryObject
	{
		Interface.MemoryStruct.IObjectIdInMemory RepresentedMemoryObject { get; }
	}
}