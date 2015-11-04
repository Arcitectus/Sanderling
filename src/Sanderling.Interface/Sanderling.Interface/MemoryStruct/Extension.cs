using System;
using System.Linq;
using System.Collections.Generic;
using BotEngine;
using Bib3.Geometrik;

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
			Bib3.RefNezDiferenz.Extension.EnumMengeRefAusNezAusWurzel(Parent, FromSensorToConsumerMessage.UITreeComponentTypeHandlePolicyCache);

		static public IEnumerable<IUIElement> EnumerateReferencedUIElementTransitive(
			this object Parent) =>
			EnumerateReferencedTransitive(Parent)
			?.OfType<IUIElement>();

		static public T CopyByPolicyMemoryMeasurement<T>(this T ToBeCopied)
			where T : class =>
			Bib3.RefBaumKopii.RefBaumKopiiStatic.ObjektKopiiErsctele(ToBeCopied, new Bib3.RefBaumKopii.Param(null, FromSensorToConsumerMessage.SerialisPolicyCache));

		static public IUIElement WithRegion(this IUIElement Base, RectInt Region) =>
			null == Base ? null : new UIElement(Base) { Region = Region };

		static public IUIElement WithRegionSizePivotAtCenter(this IUIElement Base, Vektor2DInt RegionSize) =>
			null == Base ? null : Base.WithRegion(Base.Region.WithSizePivotAtCenter(RegionSize));

		static public IUIElement WithRegionSizeBoundedMaxPivotAtCenter(this IUIElement Base, Vektor2DInt RegionSizeMax) =>
			null == Base ? null : Base.WithRegion(Base.Region.WithSizeBoundedMaxPivotAtCenter(RegionSizeMax));
	}
}
