using System;
using System.Linq;
using System.Collections.Generic;
using BotEngine;
using Bib3.Geometrik;
using Bib3;

namespace Sanderling.Interface.MemoryStruct
{
	static public class Extension
	{
		static public IObjectIdInMemory AsObjectIdInMemory(this Int64 Id)
		{
			return new ObjectIdInMemory(new ObjectIdInt64(Id));
		}

		static public T Largest<T>(this IEnumerable<T> source)
			where T : class, IUIElement =>
			source.OrderByDescending(item => item?.Region.Area() ?? -1)
			?.FirstOrDefault();

		static public IEnumerable<object> EnumerateReferencedTransitive(
			this object Parent) =>
			Bib3.RefNezDiferenz.Extension.EnumMengeRefAusNezAusWurzel(Parent, FromInterfaceResponse.UITreeComponentTypeHandlePolicyCache);

		static public IEnumerable<IUIElement> EnumerateReferencedUIElementTransitive(
			this object Parent) =>
			EnumerateReferencedTransitive(Parent)
			?.OfType<IUIElement>();

		static public T CopyByPolicyMemoryMeasurement<T>(this T ToBeCopied)
			where T : class =>
			Bib3.RefBaumKopii.RefBaumKopiiStatic.ObjektKopiiErsctele(ToBeCopied, new Bib3.RefBaumKopii.Param(null, FromInterfaceResponse.SerialisPolicyCache));

		static public IUIElement WithRegion(this IUIElement Base, RectInt Region) =>
			null == Base ? null : new UIElement(Base) { Region = Region };

		static public IUIElement WithRegionSizePivotAtCenter(this IUIElement Base, Vektor2DInt RegionSize) =>
			null == Base ? null : Base.WithRegion(Base.Region.WithSizePivotAtCenter(RegionSize));

		static public IUIElement WithRegionSizeBoundedMaxPivotAtCenter(this IUIElement Base, Vektor2DInt RegionSizeMax) =>
			null == Base ? null : Base.WithRegion(Base.Region.WithSizeBoundedMaxPivotAtCenter(RegionSizeMax));

		static public Vektor2DInt? RegionCenter(
			this IUIElement UIElement) =>
			(UIElement?.Region)?.Center();

		static public Vektor2DInt? RegionSize(
			this IUIElement UIElement) =>
			(UIElement?.Region)?.Size();

		static public Vektor2DInt? RegionCornerLeftTop(
			this IUIElement UIElement) => UIElement?.Region.MinPoint();

		static public Vektor2DInt? RegionCornerRightBottom(
			this IUIElement UIElement) => UIElement?.Region.MaxPoint();

		static public IEnumerable<ITreeViewEntry> EnumerateChildNodeTransitive(
			this ITreeViewEntry TreeViewEntry) =>
			TreeViewEntry?.EnumerateNodeFromTreeBFirst(Node => Node.Child);

		static public IEnumerable<T> OrderByCenterDistanceToPoint<T>(
			this IEnumerable<T> Sequence,
			Vektor2DInt Point)
			where T : IUIElement =>
			Sequence?.OrderBy(element => (Point - element?.RegionCenter())?.LengthSquared() ?? Int64.MaxValue);

		static public IEnumerable<T> OrderByCenterVerticalDown<T>(
			this IEnumerable<T> Source)
			where T : IUIElement =>
			Source?.OrderBy(element => element?.RegionCenter()?.B ?? int.MaxValue);

		static public IEnumerable<T> OrderByNearestPointOnLine<T>(
			this IEnumerable<T> Sequence,
			Vektor2DInt LineVector,
			Func<T, Vektor2DInt?> GetPointRepresentingElement)
		{
			var LineVectorLength = LineVector.Length();

			if (null == GetPointRepresentingElement || LineVectorLength < 1)
				return Sequence;

			var LineVectorNormalizedMilli = (LineVector * 1000) / LineVectorLength;

			return
				Sequence?.Select(Element =>
				{
					Int64? LocationOnLine = null;

					var PointRepresentingElement = GetPointRepresentingElement(Element);

					if (PointRepresentingElement.HasValue)
					{
						LocationOnLine = PointRepresentingElement.Value.A * LineVectorNormalizedMilli.A + PointRepresentingElement.Value.B * LineVectorNormalizedMilli.B;
					}

					return new { Element, LocationOnLine = LocationOnLine };
				})
				?.OrderBy(ElementAndLocation => ElementAndLocation.LocationOnLine)
				?.Select(ElementAndLocation => ElementAndLocation.Element);
		}
	}
}
