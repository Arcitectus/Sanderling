using Bib3;
using Bib3.RefNezDiferenz;
using Sanderling.Interface.MemoryStruct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanderling
{
	static public class Extension
	{
		static public Vektor2DInt? RegionCenter(
			this UIElement UIElement) =>
			(UIElement?.Region)?.ZentrumLaage;

		static public Vektor2DInt? RegionSize(
			this UIElement UIElement) =>
			(UIElement?.Region)?.Grööse;

		static public Vektor2DInt? RegionCornerLeftTop(
			this UIElement UIElement) => UIElement?.Region.PunktMin;

		static public Vektor2DInt? RegionCornerRightBottom(
			this UIElement UIElement) => UIElement?.Region.PunktMax;

		static public Int64 Length(this Vektor2DInt Vector) => Vector.Betraag;

		static public Int64 LengthSquared(this Vektor2DInt Vector) => Vector.BetraagQuadriirt;

		static public IEnumerable<T> OrderByCenterDistanceToPoint<T>(
			this IEnumerable<T> Sequence,
			Vektor2DInt Point)
			where T : UIElement =>
			Sequence?.OrderBy(element => (Point - element?.RegionCenter())?.LengthSquared() ?? Int64.MaxValue);

		static public IEnumerable<T> OrderByCenterVerticalDown<T>(
			this IEnumerable<T> Source)
			where T : UIElement =>
			Source?.OrderBy(element => element?.RegionCenter()?.B ?? int.MaxValue);

		static public IEnumerable<TreeViewEntry> EnumerateChildNodeTransitive(
			this TreeViewEntry TreeViewEntry) =>
			TreeViewEntry?.BaumEnumFlacListeKnoote(Node => Node.Child);

		static public T Largest<T>(this IEnumerable<T> source)
			where T : UIElement =>
			source.OrderByDescending(item => item?.Region.Betraag ?? -1)
			?.FirstOrDefault();

		static public UIElement CopyWithRegionSubstituted(
			this UIElement UIElement,
			OrtogoonInt RegionSubstitute)
		{
			if (null == UIElement)
			{
				return null;
			}

			return new UIElement(UIElement, RegionSubstitute, UIElement.InTreeIndex);
		}

		static public UIElement CopyWithRegionSizeSubstituted(
			this UIElement UIElement,
			Vektor2DInt SizeSubstitute)
		{
			if (null == UIElement)
			{
				return null;
			}

			return UIElement.CopyWithRegionSubstituted(UIElement.Region.GrööseGeseztAngelpunktZentrum(SizeSubstitute));
		}

		static public bool EachComponentLessThenOrEqual(
			this ShipHitpointsAndEnergy O0,
			ShipHitpointsAndEnergy O1)
		{
			if (O0 == O1)
			{
				return true;
			}

			if (null == O0 || null == O1)
			{
				return false;
			}

			return
				O0.Struct <= O1.Struct &&
				O0.Armor <= O1.Armor &&
				O0.Shield <= O1.Shield &&
				O0.Capacitor <= O1.Capacitor;
		}

		static public bool EveryComponentHasValue(this ShipHitpointsAndEnergy O) =>
			null == O ? false :
			(O.Struct.HasValue &&
			O.Armor.HasValue &&
			O.Shield.HasValue &&
			O.Capacitor.HasValue);

		static public IEnumerable<KeyValuePair<EntryT, EntryT[]>> SequenceGroupByPredicate<EntryT>(
			this IEnumerable<EntryT> SequenceEntryGroupOrItem,
			Func<EntryT, bool> CallbackIsGroup)
			where EntryT : class
		{
			EntryT Group = default(EntryT);
			EntryT[] InGroupListItem = null;

			foreach (var Entry in SequenceEntryGroupOrItem.EmptyIfNull())
			{
				if (CallbackIsGroup?.Invoke(Entry) ?? false)
				{
					if (null != InGroupListItem)
					{
						yield return new KeyValuePair<EntryT, EntryT[]>(Group, InGroupListItem);
					}

					InGroupListItem = new EntryT[0];
					Group = Entry;
				}
				else
				{
					InGroupListItem = InGroupListItem.EmptyIfNull().Concat(Entry.Yield()).ToArray();
				}
			}

			if (null != InGroupListItem)
			{
				if (null != InGroupListItem)
				{
					yield return new KeyValuePair<EntryT, EntryT[]>(Group, InGroupListItem);
				}
			}
		}

		static public IEnumerable<KeyValuePair<GroupT, EntryT[]>> SequenceGroupByType<EntryT, GroupT>(
			this IEnumerable<EntryT> SequenceEntryGroupOrItem)
			where EntryT : class
			where GroupT : class, EntryT
			=>
			SequenceGroupByPredicate(SequenceEntryGroupOrItem, entry => entry is GroupT)
			?.Select(Group => new KeyValuePair<GroupT, EntryT[]>(Group.Key as GroupT, Group.Value));

		static public IEnumerable<KeyValuePair<DroneViewEntryGroup, DroneViewEntryItem[]>> ListDroneViewEntryGrouped(
			this IEnumerable<ListEntry> List) =>
			List
			?.OfType<DroneViewEntry>()
			?.SequenceGroupByType<DroneViewEntry, DroneViewEntryGroup>()
			?.Select(Group => new KeyValuePair<DroneViewEntryGroup, DroneViewEntryItem[]>(Group.Key, Group.Value?.OfType<DroneViewEntryItem>()?.ToArray()));

		static public IEnumerable<KeyValuePair<ListEntry, ListEntry[]>> ListViewEntryGrouped(
			this IEnumerable<ListEntry> List) =>
			SequenceGroupByPredicate(List, entry => entry?.IsGroup ?? false);

		static public OrtogoonInt Expanded(
			this OrtogoonInt BeforeExpansion,
			Vektor2DInt Expansion) =>
			BeforeExpansion.GrööseGeseztAngelpunktZentrum(BeforeExpansion.Grööse + Expansion);

		static public OrtogoonInt Expanded(
			this OrtogoonInt BeforeExpansion,
			int Expansion) =>
			BeforeExpansion.Expanded(new Vektor2DInt(Expansion, Expansion));

		static public IEnumerable<OrtogoonInt> SubstractionRemainder(
			this OrtogoonInt Minuend,
			OrtogoonInt Subtrahend) => Minuend.Diferenz(Subtrahend);

		static public Vektor2DInt RandomPointInRectangle(
			this OrtogoonInt Rectangle,
			Random Random) =>
			new Vektor2DInt(
				Rectangle.Min0 + (Random.Next() % Math.Max(1, Rectangle.Max0 - Rectangle.Min0)),
				Rectangle.Min1 + (Random.Next() % Math.Max(1, Rectangle.Max1 - Rectangle.Min1)));

		static public IEnumerable<OrtogoonInt> SubstractionRemainder(
			this OrtogoonInt Minuend,
			IEnumerable<OrtogoonInt> SetSubtrahend)
		{
			var Diference = new[] { Minuend };

			foreach (var Subtrahend in SetSubtrahend.EmptyIfNull())
			{
				Diference =
					Diference?.Select(DiferencePortion => DiferencePortion.SubstractionRemainder(Subtrahend))?.ConcatNullable()?.ToArray();
			}

			return Diference;
		}

		/// <summary>
		/// only evaluates the InTreeIndex of the UIElements, not their 2D regions.
		/// </summary>
		/// <param name="ElementBehind"></param>
		/// <param name="UITree"></param>
		/// <returns>the upmost nodes of all subtrees which are in front of <paramref name="ElementBehind"/></returns>
		static public IEnumerable<UIElement> GetUpmostUIElementOfSubtreeInFront(
			this UIElement ElementBehind,
			object UITree)
		{
			if (null == ElementBehind || null == UITree)
			{
				yield break;
			}

			var Queue = new Queue<object>();

			Queue.Enqueue(UITree);

			var NodeVisited = new Dictionary<object, bool>();

			while (0 < Queue.Count)
			{
				var Node = Queue.Dequeue();

				if (NodeVisited.ContainsKey(Node))
				{
					continue;
				}

				NodeVisited[Node] = true;

				var NodeAsUIElement = Node as UIElement;

				if (null == NodeAsUIElement)
				{
					Queue.EnqueueSeq(Node.EnumRefClrVonObjekt(Interface.FromSensorToConsumerMessage.UITreeComponentTypeHandlePolicyCache));
				}
				else
				{
					if (NodeAsUIElement.InTreeIndex == ElementBehind.InTreeIndex)
					{
						continue;
					}

					if ((NodeAsUIElement.InTreeIndex ?? int.MinValue) < (ElementBehind.InTreeIndex ?? int.MinValue))
					{
						Queue.EnqueueSeq(Node.EnumRefClrVonObjekt(Interface.FromSensorToConsumerMessage.UITreeComponentTypeHandlePolicyCache));
					}
					else
					{
						yield return NodeAsUIElement;
					}
				}
			}
		}

		static public bool IsOccludingModal(
			this UIElement UIElement) =>
			((UIElement as Window)?.isModal ?? false) || UIElement is SystemMenu;

		static public IEnumerable<UIElement> GetOccludingUIElementModal(
			this UIElement OccludedElement,
			object UITree) =>
			UITree?.EnumerateReferencedUIElementTransitive()
			?.Where(IsOccludingModal)
			?.TakeWhile(OccludingUIElement => OccludedElement.InTreeIndex < OccludingUIElement?.InTreeIndex);

		static public IEnumerable<KeyValuePair<UIElement, OrtogoonInt[]>> GetOccludingUIElementAndRemainingRegion(
			this UIElement OccludedElement,
			object UITree)
			=>
			OccludedElement.GetUpmostUIElementOfSubtreeInFront(UITree)
			?.Select(OccludingElement => new KeyValuePair<UIElement, OrtogoonInt[]>(
				OccludingElement, OccludedElement.Region.SubstractionRemainder(OccludingElement.Region).ToArray()))
			//	only take elements where the remaining region is smaller than the region of the OccludedElement.
			?.Where(OccludingElementAndRemainingRegion =>
				(OccludingElementAndRemainingRegion.Value?.Select(subregion => subregion.Betraag)?.Sum() ?? 0) < OccludedElement.Region.Betraag);

		static public IEnumerable<OrtogoonInt> GetOccludedUIElementRemainingRegion(
			this UIElement OccludedElement,
			object UITree) =>
			OccludedElement.Region.SubstractionRemainder(
			GetOccludingUIElementAndRemainingRegion(OccludedElement, UITree)
			?.Select(OccludingElementAndRemainingRegion => OccludingElementAndRemainingRegion.Key.Region));

	}
}
