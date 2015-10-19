using Bib3;

namespace Sanderling.Accumulation
{
	public interface IEntityWithHistory<AccumulatedT>
	{
		int AccumulatedCount { get; }
		FieldGenMitIntervalInt64<AccumulatedT> LastInstant { get; }
		FieldGenMitIntervalInt64<AccumulatedT> NotDefaultLastInstant { get; }
	}

	public interface IEntityScoring<in AccumulatedT, in SharedT>
	{
		int Score(AccumulatedT Instant, SharedT Shared);
	}

}