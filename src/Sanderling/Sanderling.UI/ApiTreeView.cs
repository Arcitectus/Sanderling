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

		public IEnumerable<KeyValuePair<object, object>> ListeContainedNodeIdAndValue(object NodeValue)
		{
			if (null == NodeValue)
			{
				return null;
			}

			if (AsSequence)
			{
				return
					(NodeValue as IEnumerable)
					?.OfType<object>()
					?.Select((elementValue, Index) => new KeyValuePair<object, object>(Index, elementValue));
			}

			var NodeValueWrapped = NodeValue.WrapIfValueType();

			return
				MemberView
				?.Select(MemberView => new KeyValuePair<object, object>(MemberView?.Id, MemberView?.Getter?.Invoke(NodeValueWrapped)));
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
		/// <param name="Type"></param>
		/// <returns></returns>
		static public bool IsLeaf(Type Type)
		{
			if (null == Type)
			{
				return true;
			}

			if (Type.IsPrimitive || Type.IsEnum)
			{
				return true;
			}

			if (typeof(string) == Type)
			{
				return true;
			}

			return false;
		}

		ApiTreeViewNodeTypeView TypeView(Type Type) =>
			null == Type ? null : CacheTypeView?.ValueFürKey(Type, TypeViewConstruct);

		static bool MemberVisible(MemberInfo Member)
		{
			if (null == Member)
			{
				return false;
			}

			var Property = Member as PropertyInfo;

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

			if (Member.ReflectedType?.InheritsOrImplements<ITimespanInt64>() ?? false)
			{
				if (new[] {
					nameof(PropertyGenTimespanInt64<int>.Up),
					nameof(PropertyGenTimespanInt64<int>.Low)
				}.Contains(Member.Name))
				{
					return false;
				}
			}

			if (Member.ReflectedType?.InheritsOrImplementsOrEquals<Script.HostToScript>() ?? false)
			{
				if (new[] {
					nameof(Script.HostToScript.MemoryMeasurementFunc),
					nameof(Script.HostToScript.MotionExecuteFunc),
				}.Contains(Member.Name))
				{
					return false;
				}
			}

			switch (Member.MemberType)
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

		MemberGetter MemberGetter(System.Reflection.MemberInfo Member) =>
			(Member as PropertyInfo)?.DelegateForGetPropertyValue() ??
			(Member as FieldInfo)?.DelegateForGetFieldValue();

		ApiTreeViewNodeTypeView TypeViewConstruct(Type Type)
		{
			if (IsLeaf(Type))
			{
				return null;
			}

			if (Type.IsArray)
			{

			}

			if (Type.Implements(typeof(IEnumerable)))
			{
				return new ApiTreeViewNodeTypeView()
				{
					AsSequence = true,
				};
			}

			var SetMember =
				Type.GetMembers(BindingFlags.Public | BindingFlags.Instance)
				?.Where(MemberVisible)
				?.ToArray();

			return new ApiTreeViewNodeTypeView()
			{
				MemberView =
				SetMember
				?.Select(Member => new ApiTreeViewNodeTypeMemberView()
				{
					Id = Member?.Name,
					Getter = MemberGetter(Member),
				})
				?.ToArray(),
			};
		}

		public bool AstIdentGlaicwertig(object Id0, object Id1) => Base.AstIdentGlaicwertig(Id0, Id1);

		public object HeaderContent(object NodeId, object NodeValue, object HeaderContentPrev) =>
			Base?.HeaderContent(NodeId, NodeValue, HeaderContentPrev);

		public IEnumerable<KeyValuePair<object, object>> ListeAstEnthalteInAstIdentUndWert(object NodeValue) =>
			TypeView(NodeValue?.GetType())?.ListeContainedNodeIdAndValue(NodeValue);
	}
}
