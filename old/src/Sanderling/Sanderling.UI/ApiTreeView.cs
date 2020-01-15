using System;
using System.Collections.Generic;
using System.Linq;
using Fasterflect;
using System.Collections;
using Bib3;
using System.Reflection;
using Bib3.RefNezDiferenz;

namespace Sanderling.UI
{
	public class ApiTreeViewNodeTypeMemberView
	{
		public object Id;

		public MemberGetter Getter;
	}

	public class ApiTreeViewNodeTypeView
	{
		public ApiTreeViewNodeTypeMemberView[] MemberView;

		public bool AsSequence;

		public IEnumerable<KeyValuePair<object, object>> ListeContainedNodeIdAndValue(object nodeValue)
		{
			if (null == nodeValue)
			{
				return null;
			}

			if (AsSequence)
			{
				return
					(nodeValue as IEnumerable)
					?.OfType<object>()
					?.Select((elementValue, index) => new KeyValuePair<object, object>(index, elementValue));
			}

			var NodeValueWrapped = nodeValue.WrapIfValueType();

			return
				MemberView
				?.Select(memberView => new KeyValuePair<object, object>(memberView?.Id, memberView?.Getter?.Invoke(NodeValueWrapped)));
		}
	}

	public class ApiTreeViewNodeView : Bib3.FCL.GBS.Inspektor.IAstSict
	{
		Bib3.FCL.GBS.Inspektor.IAstSict Base;

		readonly SictScatenscpaicerDict<Type, ApiTreeViewNodeTypeView> CacheTypeView = new SictScatenscpaicerDict<Type, ApiTreeViewNodeTypeView>();

		public ApiTreeViewNodeView()
		{
			Base = new Bib3.Terz.GBS.Inspektor.AstSictRefNezDif(Interface.FromInterfaceResponse.SerialisPolicyCache);
		}

		/// <summary>
		/// True if no members should be shown for this Type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		static public bool IsLeaf(Type type)
		{
			if (null == type)
			{
				return true;
			}

			if (type.IsPrimitive || type.IsEnum)
			{
				return true;
			}

			if (typeof(string) == type)
			{
				return true;
			}

			return false;
		}

		ApiTreeViewNodeTypeView TypeView(Type type) =>
			null == type ? null : CacheTypeView?.ValueFürKey(type, TypeViewConstruct);

		static bool MemberVisible(MemberInfo member)
		{
			if (null == member)
			{
				return false;
			}

			var Property = member as PropertyInfo;

			if (null != Property)
			{
				if (!Property.CanRead)
				{
					return false;
				}

				if (!Property.GetMethod?.IsPublic ?? false)
				{
					return false;
				}

			}

			if (member.ReflectedType?.InheritsOrImplements<ITimespanInt64>() ?? false)
			{
				if (new[] {
					nameof(PropertyGenTimespanInt64<int>.Up),
					nameof(PropertyGenTimespanInt64<int>.Low)
				}.Contains(member.Name))
				{
					return false;
				}
			}

			if (member.ReflectedType?.InheritsOrImplementsOrEquals<Script.Impl.HostToScript>() ?? false)
			{
				if (new[] {
					nameof(Script.Impl.HostToScript.MemoryMeasurementFunc),
					nameof(Script.Impl.HostToScript.MotionExecuteFunc),
				}.Contains(member.Name))
				{
					return false;
				}
			}

			switch (member.MemberType)
			{
				case MemberTypes.Property:
					break;
				case MemberTypes.Field:
					break;

				default:
					return false;
			}

			return true;
		}

		MemberGetter MemberGetter(System.Reflection.MemberInfo member) =>
			(member as PropertyInfo)?.DelegateForGetPropertyValue() ??
			(member as FieldInfo)?.DelegateForGetFieldValue();

		ApiTreeViewNodeTypeView TypeViewConstruct(Type type)
		{
			if (IsLeaf(type))
			{
				return null;
			}

			if (type.IsArray)
			{

			}

			if (type.Implements(typeof(IEnumerable)))
			{
				return new ApiTreeViewNodeTypeView()
				{
					AsSequence = true,
				};
			}

			var SetMember =
				type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
				?.Where(MemberVisible)
				?.ToArray();

			return new ApiTreeViewNodeTypeView()
			{
				MemberView =
				SetMember
				?.Select(member => new ApiTreeViewNodeTypeMemberView()
				{
					Id = member?.Name,
					Getter = MemberGetter(member),
				})
				?.ToArray(),
			};
		}

		public bool AstIdentGlaicwertig(object id0, object id1) => Base.AstIdentGlaicwertig(id0, id1);

		public object HeaderContent(object nodeId, object nodeValue, object headerContentPrev) =>
			Base?.HeaderContent(nodeId, nodeValue, headerContentPrev);

		public IEnumerable<KeyValuePair<object, object>> ListeAstEnthalteInAstIdentUndWert(object nodeValue) =>
			TypeView(nodeValue?.GetType())?.ListeContainedNodeIdAndValue(nodeValue);
	}
}
