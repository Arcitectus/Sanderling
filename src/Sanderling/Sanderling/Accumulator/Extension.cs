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
		/// Distributes the elements from <paramref name="Source"/> among <paramref name="Destination"/>.
		/// The method "Accumulate" is called once on each element in <paramref name="Destination"/>.
		/// </summary>
		/// <typeparam name="AccumulatedT"></typeparam>
		/// <typeparam name="SharedT"></typeparam>
		/// <typeparam name="DestEntityT"></typeparam>
		/// <param name="Source"></param>
		/// <param name="Shared"></param>
		/// <param name="Destination"></param>
		/// <returns>subset of <paramref name="Source"/> which was not assigned to elements in <paramref name="Destination"/>.</returns>
		static public IEnumerable<FieldGenMitIntervalInt64<AccumulatedT>> Distribute<AccumulatedT, SharedT, DestEntityT>(
			this IEnumerable<FieldGenMitIntervalInt64<AccumulatedT>> Source,
			SharedT Shared,
			ICollection<DestEntityT> Destination)
			where DestEntityT : EntityScoring<AccumulatedT, SharedT>
		{
			if (null == Destination)
			{
				return Source;
			}

			var SourceInstantConsumed = new HashSet<FieldGenMitIntervalInt64<AccumulatedT>>();

			var DestinationEntityFed = new HashSet<DestEntityT>();

			var SourceRendered = Source?.ToArray();

			var SetCombination =
				Destination?.SelectMany(DestinationEntity =>
				SourceRendered
				?.WhereNotDefault()
				?.Select(SourceInstant =>
				new { SourceInstant, DestinationEntity, Score = DestinationEntity?.Score(SourceInstant.Wert, Shared) ?? int.MinValue }))
				?.OrderByDescending(Combi => Combi.Score)
				?.ToArray();

			foreach (var Combination in SetCombination)
			{
				if (!(0 < Combination.Score))
				{
					break;
				}

				if (SourceInstantConsumed.Contains(Combination.SourceInstant))
				{
					continue;
				}

				if (DestinationEntityFed.Contains(Combination.DestinationEntity))
				{
					continue;
				}

				Combination.DestinationEntity.Accumulate(Combination.SourceInstant, Shared);

				SourceInstantConsumed.Add(Combination.SourceInstant);
				DestinationEntityFed.Add(Combination.DestinationEntity);
			}

			return SourceRendered.Except(SourceInstantConsumed);
		}

		/// <summary>
		/// </summary>
		/// <typeparam name="AccumulationT"></typeparam>
		/// <typeparam name="AccumulatedT"></typeparam>
		/// <typeparam name="SharedT"></typeparam>
		/// <param name="SourceCandidate"></param>
		/// <param name="InstantToFit"></param>
		/// <param name="Other"></param>
		/// <returns>best fit according to method "Score" of element in <paramref name="SourceCandidate"/></returns>
		static public AccumulationT BestFitFromSet<AccumulationT, AccumulatedT, SharedT>(
			this IEnumerable<AccumulationT> SourceCandidate,
			AccumulatedT InstantToFit,
			SharedT Other = default(SharedT))
			where AccumulationT : class, Accumulation.IEntityScoring<AccumulatedT, SharedT> =>
			SourceCandidate?.Select(Accumulation => new { Accumulation, Score = Accumulation?.Score(InstantToFit, Other) ?? int.MinValue })
			?.Where(scored => 0 < scored.Score)
			?.OrderByDescending(scored => scored.Score)
			?.FirstOrDefault()?.Accumulation;

		static public Accumulation.IShipUiModuleAndContext AsAccuInstant(
			this MemoryStruct.IShipUiModule Module,
			MemoryStruct.IShipUi ShipUi) =>
			new ShipUiModuleAndContext() { Module = Module, Location = Module?.PositionInShipUi(ShipUi), };

		static public Accumulation.IShipUiModule Accumulation(
			this Accumulation.IMemoryMeasurement Accumulation,
			MemoryStruct.IShipUiModule Module,
			Parse.IMemoryMeasurement MemoryMeasurement) =>
			Accumulation?.ShipUiModule?.BestFitFromSet(Module.AsAccuInstant(MemoryMeasurement?.ShipUi), MemoryMeasurement);

	}
}
