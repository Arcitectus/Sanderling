using System;
using System.Linq;
using System.Collections.Generic;
using BotEngine;

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
			source.OrderByDescending(item => item?.Region.Betraag ?? -1)
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
			Bib3.SictRefBaumKopii.ObjektKopiiErsctele(ToBeCopied, new Bib3.SictRefBaumKopiiParam(null, FromSensorToConsumerMessage.SerialisPolicyCache));
	}
}
