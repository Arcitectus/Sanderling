using Bib3;
using BotEngine;
using System;

namespace Sanderling.Accumulator
{
	public class Entity : ObjectIdInt64
	{
		protected Entity()
		{
		}

		public Entity(Int64 Id)
			: base(Id)
		{
		}
	}

	public class EntityWithHistory<AccumulatedT, SharedT> : Entity, Accumulation.IEntityWithHistory<AccumulatedT>
	{
		public int AccumulatedCount
		{
			private set;
			get;
		}

		public FieldGenMitIntervalInt64<AccumulatedT> NotDefaultLastInstant
		{
			private set;
			get;
		}

		public FieldGenMitIntervalInt64<AccumulatedT> LastInstant
		{
			private set;
			get;
		}

		protected EntityWithHistory()
		{
		}

		public EntityWithHistory(
			Int64 Id,
			FieldGenMitIntervalInt64<AccumulatedT> Instant,
			SharedT Shared = default(SharedT))
			:
			base(Id)
		{
			Accumulate(Instant, Shared);
		}

		public void Accumulate(
			FieldGenMitIntervalInt64<AccumulatedT> Instant,
			SharedT Other = default(SharedT))
		{
			if (null == Instant)
			{
				return;
			}

			++AccumulatedCount;

			LastInstant = Instant;

			if (!object.Equals(default(AccumulatedT), Instant.Wert))
			{
				NotDefaultLastInstant = Instant;
			}

			Accumulated(Instant, Other);
		}

		virtual protected void Accumulated(
			FieldGenMitIntervalInt64<AccumulatedT> Instant,
			SharedT Shared)
		{
		}
	}

	public class EntityScoring<AccumulatedT, SharedT> : EntityWithHistory<AccumulatedT, SharedT>, Accumulation.IEntityScoring<AccumulatedT, SharedT>
	{
		protected EntityScoring()
		{
		}

		public EntityScoring(
			Int64 Id,
			FieldGenMitIntervalInt64<AccumulatedT> Instant)
			:
			base(Id, Instant)
		{
		}

		public virtual int Score(AccumulatedT Instant, SharedT Shared)
		{
			return 0;
		}
	}

	public class EntityScoring<AccumulatedT> : EntityScoring<AccumulatedT, int>
	{

	}
}
