using Bib3;
using Bib3.Geometrik;
using Bib3.RefNezDiferenz;
using Sanderling.Interface.MemoryStruct;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sanderling
{
	static public class Extension
	{
		static public bool EachComponentLessThenOrEqual(
			this IShipHitpointsAndEnergy o0,
			IShipHitpointsAndEnergy o1)
		{
			if (o0 == o1)
			{
				return true;
			}

			if (null == o0 || null == o1)
			{
				return false;
			}

			return
				o0.Struct <= o1.Struct &&
				o0.Armor <= o1.Armor &&
				o0.Shield <= o1.Shield &&
				o0.Capacitor <= o1.Capacitor;
		}

		static public bool EveryComponentHasValue(this IShipHitpointsAndEnergy o) =>
			null == o ? false :
			(o.Struct.HasValue &&
			o.Armor.HasValue &&
			o.Shield.HasValue &&
			o.Capacitor.HasValue);

		static public IEnumerable<KeyValuePair<EntryT, EntryT[]>> SequenceGroupByPredicate<EntryT>(
			this IEnumerable<EntryT> sequenceEntryGroupOrItem,
			Func<EntryT, bool> callbackIsGroup)
			where EntryT : class
		{
			EntryT Group = default(EntryT);
			EntryT[] InGroupListItem = null;

			foreach (var Entry in sequenceEntryGroupOrItem.EmptyIfNull())
			{
				if (callbackIsGroup?.Invoke(Entry) ?? false)
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
			this IEnumerable<EntryT> sequenceEntryGroupOrItem)
			where EntryT : class
			where GroupT : class, EntryT
			=>
			SequenceGroupByPredicate(sequenceEntryGroupOrItem, entry => entry is GroupT)
			?.Select(group => new KeyValuePair<GroupT, EntryT[]>(group.Key as GroupT, group.Value));

		static public IEnumerable<KeyValuePair<DroneViewEntryGroup, IDroneViewEntryItem[]>> ListDroneViewEntryGrouped(
			this IEnumerable<IListEntry> list) =>
			list
			?.OfType<DroneViewEntry>()
			?.SequenceGroupByType<DroneViewEntry, DroneViewEntryGroup>()
			?.Select(group => new KeyValuePair<DroneViewEntryGroup, IDroneViewEntryItem[]>(group.Key, group.Value?.OfType<DroneViewEntryItem>()?.ToArray()));

		static public IEnumerable<KeyValuePair<IListEntry, IListEntry[]>> ListViewEntryGrouped(
			this IEnumerable<IListEntry> list) =>
			SequenceGroupByPredicate(list, entry => entry?.IsGroup ?? false);

		static public RectInt WithSizeExpandedPivotAtCenter(
			this RectInt beforeExpansion,
			int expansion) =>
			beforeExpansion.WithSizeExpandedPivotAtCenter(new Vektor2DInt(expansion, expansion));

		static public IEnumerable<RectInt> SubstractionRemainder(
			this RectInt minuend,
			RectInt subtrahend) => minuend.Diferenz(subtrahend);

		static public Vektor2DInt RandomPointInRectangle(
			this RectInt rectangle,
			Random random) =>
			new Vektor2DInt(
				rectangle.Min0 + (random.Next() % Math.Max(1, rectangle.Max0 - rectangle.Min0)),
				rectangle.Min1 + (random.Next() % Math.Max(1, rectangle.Max1 - rectangle.Min1)));

		static public IEnumerable<RectInt> SubstractionRemainder(
			this RectInt minuend,
			IEnumerable<RectInt> setSubtrahend)
		{
			var Diference = new[] { minuend };

			foreach (var Subtrahend in setSubtrahend.EmptyIfNull())
			{
				Diference =
					Diference?.Select(diferencePortion => diferencePortion.SubstractionRemainder(Subtrahend))?.ConcatNullable()?.ToArray();
			}

			return Diference;
		}

		/// <summary>
		/// only evaluates the InTreeIndex of the UIElements, not their 2D regions.
		/// </summary>
		/// <param name="elementBehind"></param>
		/// <param name="uiTree"></param>
		/// <returns>the upmost nodes of all subtrees which are in front of <paramref name="elementBehind"/></returns>
		static public IEnumerable<IUIElement> GetUpmostUIElementOfSubtreeInFront(
			this IUIElement elementBehind,
			object uiTree)
		{
			if (null == elementBehind || null == uiTree)
			{
				yield break;
			}

			var Queue = new Queue<object>();

			Queue.Enqueue(uiTree);

			var NodeVisited = new Dictionary<object, bool>();

			while (0 < Queue.Count)
			{
				var Node = Queue.Dequeue();

				if (NodeVisited.ContainsKey(Node))
				{
					continue;
				}

				NodeVisited[Node] = true;

				var NodeAsUIElement = Node as IUIElement;

				if (null == NodeAsUIElement)
				{
					Queue.EnqueueSeq(Node.EnumRefClrVonObjekt(Interface.FromInterfaceResponse.UITreeComponentTypeHandlePolicyCache));
				}
				else
				{
					if (NodeAsUIElement.InTreeIndex == elementBehind.InTreeIndex)
					{
						continue;
					}

					if ((NodeAsUIElement.InTreeIndex ?? int.MinValue) < (elementBehind.InTreeIndex ?? int.MinValue))
					{
						Queue.EnqueueSeq(Node.EnumRefClrVonObjekt(Interface.FromInterfaceResponse.UITreeComponentTypeHandlePolicyCache));
					}
					else
					{
						yield return NodeAsUIElement;
					}
				}
			}
		}

		static public bool IsOccludingModal(this IUIElement uiElement) =>
			(uiElement as IWindow)?.isModal ?? false;

		static public IEnumerable<IUIElement> GetOccludingUIElementModal(
			this IUIElement occludedElement,
			object uiTree) =>
			uiTree?.EnumerateReferencedUIElementTransitive()
			?.Where(IsOccludingModal)
			?.TakeWhile(occludingUIElement => occludedElement.InTreeIndex < occludingUIElement?.InTreeIndex);

		static public IEnumerable<KeyValuePair<IUIElement, RectInt[]>> GetOccludingUIElementAndRemainingRegion(
			this IUIElement occludedElement,
			object uiTree)
			=>
			occludedElement.GetUpmostUIElementOfSubtreeInFront(uiTree)

			//	Assume that children of OccludedElement do not participate in Occlusion
			?.Where(candidateOccluding => (occludedElement?.ChildLastInTreeIndex ?? 0) < candidateOccluding?.InTreeIndex)

			?.Select(occludingElement => new KeyValuePair<IUIElement, RectInt[]>(
				occludingElement, occludedElement.Region.SubstractionRemainder(occludingElement.Region).ToArray()))
			//	only take elements where the remaining region is smaller than the region of the OccludedElement.
			?.Where(occludingElementAndRemainingRegion =>
				(occludingElementAndRemainingRegion.Value?.Select(subregion => subregion.Area())?.Sum() ?? 0) < occludedElement.Region.Area());

		static public IEnumerable<RectInt> GetOccludedUIElementRemainingRegion(
			this IUIElement occludedElement,
			object uiTree,
			Func<IUIElement, bool> callbackExclude = null) =>
			occludedElement.Region.SubstractionRemainder(
			GetOccludingUIElementAndRemainingRegion(occludedElement, uiTree)
			?.Where(occludingElementAndRemainingRegion => !(callbackExclude?.Invoke(occludingElementAndRemainingRegion.Key) ?? false))
			?.Select(occludingElementAndRemainingRegion => occludingElementAndRemainingRegion.Key.Region));

	}
}
