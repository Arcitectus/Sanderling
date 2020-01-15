using Bib3;
using Sanderling.Accumulation;
using System.Collections.Generic;
using System.Linq;
using MemoryStruct = Sanderling.Interface.MemoryStruct;

namespace Sanderling.Accumulator
{
	static public class Extension
	{
		/// <summary>
		/// Distributes the elements from <paramref name="source"/> among <paramref name="destination"/>.
		/// The method "Accumulate" is called once on each element in <paramref name="destination"/>.
		/// </summary>
		/// <typeparam name="AccumulatedT"></typeparam>
		/// <typeparam name="SharedT"></typeparam>
		/// <typeparam name="DestEntityT"></typeparam>
		/// <param name="source"></param>
		/// <param name="shared"></param>
		/// <param name="destination"></param>
		/// <returns>subset of <paramref name="source"/> which was not assigned to elements in <paramref name="destination"/>.</returns>
		static public IEnumerable<PropertyGenTimespanInt64<AccumulatedT>> Distribute<AccumulatedT, SharedT, DestEntityT>(
			this IEnumerable<PropertyGenTimespanInt64<AccumulatedT>> source,
			SharedT shared,
			ICollection<DestEntityT> destination)
			where DestEntityT : EntityScoring<AccumulatedT, SharedT>
		{
			if (null == destination)
			{
				return source;
			}

			var SourceInstantConsumed = new HashSet<PropertyGenTimespanInt64<AccumulatedT>>();

			var DestinationEntityFed = new HashSet<DestEntityT>();

			var SourceRendered = source?.ToArray();

			var SetCombination =
				destination?.SelectMany(destinationEntity =>
				SourceRendered
				?.WhereNotDefault()
				?.Select(sourceInstant =>
				new { sourceInstant, destinationEntity, Score = destinationEntity?.Score(sourceInstant.Value, shared) ?? int.MinValue }))
				?.OrderByDescending(combi => combi.Score)
				?.ToArray();

			foreach (var Combination in SetCombination)
			{
				if (!(0 < Combination.Score))
				{
					break;
				}

				if (SourceInstantConsumed.Contains(Combination.sourceInstant))
				{
					continue;
				}

				if (DestinationEntityFed.Contains(Combination.destinationEntity))
				{
					continue;
				}

				Combination.destinationEntity.Accumulate(Combination.sourceInstant, shared);

				SourceInstantConsumed.Add(Combination.sourceInstant);
				DestinationEntityFed.Add(Combination.destinationEntity);
			}

			return SourceRendered.Except(SourceInstantConsumed);
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="AccumulationT"></typeparam>
		/// <typeparam name="AccumulatedT"></typeparam>
		/// <typeparam name="SharedT"></typeparam>
		/// <param name="sourceCandidate"></param>
		/// <param name="instantToFit"></param>
		/// <param name="other"></param>
		/// <returns>best fit according to method "Score" of element in <paramref name="sourceCandidate"/></returns>
		static public AccumulationT BestFitFromSet<AccumulationT, AccumulatedT, SharedT>(
			this IEnumerable<AccumulationT> sourceCandidate,
			AccumulatedT instantToFit,
			SharedT other = default(SharedT))
			where AccumulationT : class, Accumulation.IEntityScoring<AccumulatedT, SharedT> =>
			sourceCandidate?.Select(accumulation => new { accumulation, Score = accumulation?.Score(instantToFit, other) ?? int.MinValue })
			?.Where(scored => 0 < scored.Score)
			?.OrderByDescending(scored => scored.Score)
			?.FirstOrDefault()?.accumulation;

		static public Accumulation.IShipUiModuleAndContext AsAccuInstant(
			this MemoryStruct.IShipUiModule module,
			MemoryStruct.IShipUi shipUi) =>
			new ShipUiModuleAndContext() { Module = module, Location = module?.PositionInShipUi(shipUi), };

		static public Accumulation.IShipUiModule Accumulation(
			this Accumulation.IMemoryMeasurement accumulation,
			MemoryStruct.IShipUiModule module,
			Parse.IMemoryMeasurement memoryMeasurement) =>
			accumulation?.ShipUiModule?.BestFitFromSet(module.AsAccuInstant(memoryMeasurement?.ShipUi), memoryMeasurement);

	}
}
