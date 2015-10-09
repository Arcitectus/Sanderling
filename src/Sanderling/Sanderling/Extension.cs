using Bib3;
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

	}
}
