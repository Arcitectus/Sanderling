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

		public Entity(Int64 id)
			: base(id)
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

		public PropertyGenTimespanInt64<AccumulatedT> InstantWithAgeStepCount(int ageStepCount) =>
			0 == ageStepCount ? LastInstant :
			HistoryListStep?.ElementAtOrDefault(HistoryListStep.Count - ageStepCount - 1);

		protected EntityWithHistory()
		{
		}

		public EntityWithHistory(
			Int64 id,
			PropertyGenTimespanInt64<AccumulatedT> instant,
			SharedT shared = default(SharedT))
			:
			base(id)
		{
			Accumulate(instant, shared);
		}

		public void Accumulate(
			PropertyGenTimespanInt64<AccumulatedT> instant,
			SharedT other = default(SharedT))
		{
			if (null == instant)
			{
				return;
			}

			++AccumulatedCount;

			HistoryListStep.Enqueue(instant);
			HistoryListStep.ListeKürzeBegin(HistoryLengthMax);

			LastInstant = instant;

			if (!object.Equals(default(AccumulatedT), instant.Value))
			{
				NotDefaultLastInstant = instant;
			}

			Accumulated(instant, other);
		}

		virtual protected void Accumulated(
			PropertyGenTimespanInt64<AccumulatedT> instant,
			SharedT shared)
		{
		}
	}

	public class EntityScoring<AccumulatedT, SharedT> : EntityWithHistory<AccumulatedT, SharedT>, Accumulation.IEntityScoring<AccumulatedT, SharedT>
	{
		protected EntityScoring()
		{
		}

		public EntityScoring(
			Int64 id,
			PropertyGenTimespanInt64<AccumulatedT> instant)
			:
			base(id, instant)
		{
		}

		public virtual int Score(AccumulatedT instant, SharedT shared)
		{
			return 0;
		}
	}

	public class EntityScoring<AccumulatedT> : EntityScoring<AccumulatedT, int>
	{

	}
}
