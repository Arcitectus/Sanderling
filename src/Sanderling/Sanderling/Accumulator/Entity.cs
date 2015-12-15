using Bib3;
using BotEngine;
using System;
using System.Collections.Generic;
using System.Linq;

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
		readonly Queue<PropertyGenTimespanInt64<AccumulatedT>> HistoryListStep = new Queue<PropertyGenTimespanInt64<AccumulatedT>>();

		protected int HistoryLengthMax;

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

		public PropertyGenTimespanInt64<AccumulatedT> InstantWithAgeStepCount(int AgeStepCount) =>
			0 == AgeStepCount ? LastInstant :
			HistoryListStep?.ElementAtOrDefault(HistoryListStep.Count - AgeStepCount - 1);

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

			HistoryListStep.Enqueue(Instant);
			HistoryListStep.ListeKürzeBegin(HistoryLengthMax);

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
