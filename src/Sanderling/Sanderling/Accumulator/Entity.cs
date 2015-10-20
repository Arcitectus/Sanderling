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

		public PropertyGenTimespanInt64<AccumulatedT> NotDefaultLastInstant
		{
			private set;
			get;
		}

		public PropertyGenTimespanInt64<AccumulatedT> LastInstant
		{
			private set;
			get;
		}

		protected EntityWithHistory()
		{
		}

		public EntityWithHistory(
			Int64 Id,
			PropertyGenTimespanInt64<AccumulatedT> Instant,
			SharedT Shared = default(SharedT))
			:
			base(Id)
		{
			Accumulate(Instant, Shared);
		}

		public void Accumulate(
			PropertyGenTimespanInt64<AccumulatedT> Instant,
			SharedT Other = default(SharedT))
		{
			if (null == Instant)
			{
				return;
			}

			++AccumulatedCount;

			LastInstant = Instant;

			if (!object.Equals(default(AccumulatedT), Instant.Value))
			{
				NotDefaultLastInstant = Instant;
			}

			Accumulated(Instant, Other);
		}

		virtual protected void Accumulated(
			PropertyGenTimespanInt64<AccumulatedT> Instant,
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
			PropertyGenTimespanInt64<AccumulatedT> Instant)
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
