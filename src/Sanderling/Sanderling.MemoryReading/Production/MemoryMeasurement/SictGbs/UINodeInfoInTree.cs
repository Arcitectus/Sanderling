using System;
using System.Collections.Generic;
using System.Linq;
using Bib3;
using Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;
using Sanderling.MemoryReading.Production;

namespace Optimat.EveOnline.AuswertGbs
{
	public class UINodeInfoInTree : GbsAstInfo
	{
		/// <summary>
		/// Index of this node in the list of children of its parent.
		/// </summary>
		public int? InParentListChildIndex;

		/// <summary>
		/// Index of this node in the tree when serialized depth-first.
		/// Can be used to determine the Z-Order of UI Elements.
		/// </summary>
		public int? InTreeIndex;

		public int? ChildLastInTreeIndex;

		new public UINodeInfoInTree[] ListChild;

		public Vektor2DSingle? FromParentLocation;

		override public IEnumerable<GbsAstInfo> GetListChild() => ListChild;
	}


	static public class UINodeExtension
	{
		static public IUIElement AsUIElementIfVisible(
			this UINodeInfoInTree uiNode)
		{
			if (!(uiNode?.VisibleIncludingInheritance ?? false))
				return null;

			return new UIElement(new ObjectIdInMemory(uiNode.PyObjAddress ?? 0))
			{
				Region = AuswertGbs.Glob.FläceAusGbsAstInfoMitVonParentErbe(uiNode) ?? RectInt.Empty,

				InTreeIndex = uiNode.InTreeIndex,
				ChildLastInTreeIndex = uiNode.ChildLastInTreeIndex,
			};
		}

		static public bool AstMitHerkunftAdreseEnthaltAstMitHerkunftAdrese(
			this UINodeInfoInTree suuceWurzel,
			Int64 enthaltendeAstHerkunftAdrese,
			Int64 enthalteneAstHerkunftAdrese)
		{
			if (enthaltendeAstHerkunftAdrese == enthalteneAstHerkunftAdrese)
				return true;

			if (null == suuceWurzel)
				return false;

			var enthaltendeAst = suuceWurzel.FirstNodeWithPyObjAddressFromSubtreeBreadthFirst(enthaltendeAstHerkunftAdrese);

			if (null == enthaltendeAst)
				return false;

			return enthaltendeAst.EnthaltAstMitHerkunftAdrese(enthalteneAstHerkunftAdrese);
		}

		static public bool EnthaltAstMitHerkunftAdrese(
			this UINodeInfoInTree suuceWurzel,
			Int64 astHerkunftAdrese) =>
			suuceWurzel?.AstEnthalteInBaum((kandidaatAst) => kandidaatAst?.PyObjAddress == astHerkunftAdrese, zuZerleegende => zuZerleegende.ListChild) ?? false;

		static public bool EnthaltAst(
			this UINodeInfoInTree suuceWurzel,
			UINodeInfoInTree node)
		{
			if (null == node)
				return false;

			return suuceWurzel?.AstEnthalteInBaum(node, zuZerleegende => zuZerleegende.ListChild) ?? false;
		}

		/// <summary>
		/// Tailmenge der Ast welce kaine andere Ast aus der Menge enthalt.
		/// </summary>
		/// <param name="mengeAstRepr"></param>
		/// <returns></returns>
		static public IEnumerable<T> TailmengeUnterste<T>(
			this IEnumerable<T> mengeAstRepr,
			UINodeInfoInTree uiTree)
			where T : class, IUIElement
		{
			if (null == mengeAstRepr)
				return null;

			if (null == uiTree)
				return null;

			return
				mengeAstRepr
				?.Where(astRepr =>
					{
						var Ast = uiTree.FirstNodeWithPyObjAddressFromSubtreeBreadthFirst(astRepr.Id);

						if (null == Ast)
						{
							return false;
						}

						return !mengeAstRepr.Any(andereAstRepr =>
							{
								if (null == andereAstRepr)
									return false;

								if (andereAstRepr == astRepr)
									return false;

								var AndereAst = uiTree.FirstNodeWithPyObjAddressFromSubtreeBreadthFirst(andereAstRepr.Id);

								if (AndereAst == Ast)
								{
									return false;
								}

								return Ast.EnthaltAst(AndereAst);
							});
					});
		}

		static public float? LaagePlusVonParentErbeLaageA(this UINodeInfoInTree uiNode) =>
			uiNode?.LaagePlusVonParentErbeLaage()?.A;

		static public float? LaagePlusVonParentErbeLaageB(this UINodeInfoInTree uiNode) =>
			uiNode?.LaagePlusVonParentErbeLaage()?.B;

		static public MenuEntry MenuEntry(
			this UINodeInfoInTree menuEntryAst,
			RectInt regionConstraint,
			bool? highlight = null)
		{
			var container = menuEntryAst.AlsContainer(regionConstraint: regionConstraint);

			if (null == container)
				return null;

			return new MenuEntry(container)
			{
				HighlightVisible = highlight,
			};
		}

		static public Window Window(
			this UINodeInfoInTree uiNode,
			bool? isModal,
			string caption,
			bool? headerButtonsVisible = null,
			Sprite[] headerButton = null)
		{
			string GbsAstType = null;

			if (null != uiNode)
			{
				GbsAstType = uiNode.PyObjTypName;
			}

			return new Window(uiNode.AsUIElementIfVisible())
			{
				isModal = isModal,
				Caption = caption,
				HeaderButtonsVisible = headerButtonsVisible,
				HeaderButton = headerButton,
			};
		}
	}
}
