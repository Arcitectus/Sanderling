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
		static public IObjectIdInMemory AsObjectIdInMemory(this Int64 id)
		{
			return new ObjectIdInMemory(new ObjectIdInt64(id));
		}

		static public T Largest<T>(this IEnumerable<T> source)
			where T : class, IUIElement =>
			source.OrderByDescending(item => item?.Region.Area() ?? -1)
			?.FirstOrDefault();

		static public IEnumerable<object> EnumerateReferencedTransitive(
			this object parent) =>
			Bib3.RefNezDiferenz.Extension.EnumMengeRefAusNezAusWurzel(parent, FromInterfaceResponse.UITreeComponentTypeHandlePolicyCache);

		static public IEnumerable<IUIElement> EnumerateReferencedUIElementTransitive(
			this object parent) =>
			EnumerateReferencedTransitive(parent)
			?.OfType<IUIElement>();

		static public T CopyByPolicyMemoryMeasurement<T>(this T toBeCopied)
			where T : class =>
			Bib3.RefBaumKopii.RefBaumKopiiStatic.ObjektKopiiErsctele(toBeCopied, new Bib3.RefBaumKopii.Param(null, FromInterfaceResponse.SerialisPolicyCache));

		static public IUIElement WithRegion(this IUIElement @base, RectInt region) =>
			null == @base ? null : new UIElement(@base) { Region = region };

		static public IUIElement WithRegionSizePivotAtCenter(this IUIElement @base, Vektor2DInt regionSize) =>
			null == @base ? null : @base.WithRegion(@base.Region.WithSizePivotAtCenter(regionSize));

		static public IUIElement WithRegionSizeBoundedMaxPivotAtCenter(this IUIElement @base, Vektor2DInt regionSizeMax) =>
			null == @base ? null : @base.WithRegion(@base.Region.WithSizeBoundedMaxPivotAtCenter(regionSizeMax));

		static public Vektor2DInt? RegionCenter(
			this IUIElement uiElement) =>
			(uiElement?.Region)?.Center();

		static public Vektor2DInt? RegionSize(
			this IUIElement uiElement) =>
			(uiElement?.Region)?.Size();

		static public Vektor2DInt? RegionCornerLeftTop(
			this IUIElement uiElement) => uiElement?.Region.MinPoint();

		static public Vektor2DInt? RegionCornerRightBottom(
			this IUIElement uiElement) => uiElement?.Region.MaxPoint();

		static public IEnumerable<ITreeViewEntry> EnumerateChildNodeTransitive(
			this ITreeViewEntry treeViewEntry) =>
			treeViewEntry?.EnumerateNodeFromTreeBFirst(node => node.Child);

		static public IEnumerable<T> OrderByCenterDistanceToPoint<T>(
			this IEnumerable<T> sequence,
			Vektor2DInt point)
			where T : IUIElement =>
			sequence?.OrderBy(element => (point - element?.RegionCenter())?.LengthSquared() ?? Int64.MaxValue);

		static public IEnumerable<T> OrderByCenterVerticalDown<T>(
			this IEnumerable<T> source)
			where T : IUIElement =>
			source?.OrderBy(element => element?.RegionCenter()?.B ?? int.MaxValue);

		static public IEnumerable<T> OrderByNearestPointOnLine<T>(
			this IEnumerable<T> sequence,
			Vektor2DInt lineVector,
			Func<T, Vektor2DInt?> getPointRepresentingElement)
		{
			var LineVectorLength = lineVector.Length();

			if (null == getPointRepresentingElement || LineVectorLength < 1)
				return sequence;

			var LineVectorNormalizedMilli = (lineVector * 1000) / LineVectorLength;

			return
				sequence?.Select(element =>
				{
					Int64? LocationOnLine = null;

					var PointRepresentingElement = getPointRepresentingElement(element);

					if (PointRepresentingElement.HasValue)
					{
						LocationOnLine = PointRepresentingElement.Value.A * LineVectorNormalizedMilli.A + PointRepresentingElement.Value.B * LineVectorNormalizedMilli.B;
					}

					return new { Element = element, LocationOnLine = LocationOnLine };
				})
				?.OrderBy(elementAndLocation => elementAndLocation.LocationOnLine)
				?.Select(elementAndLocation => (T)elementAndLocation.Element);
		}
	}
}
