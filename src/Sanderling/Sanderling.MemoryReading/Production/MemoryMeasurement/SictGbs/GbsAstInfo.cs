using System;
using System.Collections.Generic;
using System.Linq;
using Bib3;
using Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;
using Sanderling.MemoryReading.Production;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictGbsAstInfoSictAuswert : GbsAstInfo
	{
		/// <summary>
		/// Index welcen diises Objekt in der ListeChild des Parent ainnimt. Gibt bai Eve Online hinwais auf dii Z-Ordnung von GBS Elementen.
		/// </summary>
		public int? InParentListeChildIndex;

		/// <summary>
		/// Index welcer di Ordnung aler Äste im Baum per Tiifensuuce angiibt.
		/// Gibt bai Eve Online hinwais auf dii Z-Ordnung von GBS Elementen.
		/// </summary>
		public int? InBaumAstIndex;

		public int? ChildLezteInBaumAstIndex;

		new public SictGbsAstInfoSictAuswert[] ListeChild;

		public Vektor2DSingle? VonParentErbeLaage;

		public float VonParentErbeClippingFläceLinx
		{
			private set;
			get;
		}

		public float VonParentErbeClippingFläceOobn
		{
			private set;
			get;
		}

		public float VonParentErbeClippingFläceRecz
		{
			private set;
			get;
		}

		public float VonParentErbeClippingFläceUntn
		{
			private set;
			get;
		}

		public void VonParentErbeClippingFläceSezeAufUnbescrankt()
		{
			VonParentErbeClippingFläceLinx = float.MinValue;
			VonParentErbeClippingFläceOobn = float.MinValue;
			VonParentErbeClippingFläceRecz = float.MaxValue;
			VonParentErbeClippingFläceUntn = float.MaxValue;
		}

		override public IEnumerable<GbsAstInfo> ListeChildBerecne()
		{
			return ListeChild;
		}
	}


	static public class Erwaiterung
	{
		static public IUIElement AusGbsAstClippedFalsUnglaicNullUndSictbar(
			SictGbsAstInfoSictAuswert uiNode,
			SictGbsAstInfoSictAuswert clipperNode)
		{
			if (!(uiNode?.SictbarMitErbe ?? false))
				return null;

			var clipperNodeFläce = clipperNode?.AlsUIElementFalsUnglaicNullUndSictbar();

			var GbsFläceVorClipping = uiNode.AlsUIElementFalsUnglaicNullUndSictbar();

			var GbsAstFläceClipped =
				(null == clipperNodeFläce) ? RectInt.Empty : clipperNodeFläce.Region.Intersection(GbsFläceVorClipping.Region);

			return new UIElement(GbsFläceVorClipping)
			{
				Region = GbsAstFläceClipped,
			};
		}

		static public IUIElement AusGbsAstFalsUnglaicNull(
			this SictGbsAstInfoSictAuswert uiNode)
		{
			if (null == uiNode)
				return null;

			var InGbsFläce = AuswertGbs.Glob.FläceAusGbsAstInfoMitVonParentErbe(uiNode);
			var InGbsParentChildIndex = uiNode.InParentListeChildIndex;
			var InGbsBaumAstIndex = uiNode.InBaumAstIndex;
			var GbsAstName = uiNode.Name;

			return new UIElement(new ObjectIdInMemory(uiNode.HerkunftAdrese ?? 0))
			{
				Region = InGbsFläce ?? RectInt.Empty,

				InTreeIndex = InGbsBaumAstIndex,
				ChildLastInTreeIndex = uiNode.ChildLezteInBaumAstIndex,
			};
		}

		static public IUIElement AlsUIElementFalsUnglaicNullUndSictbar(
			this SictGbsAstInfoSictAuswert uiNode)
		{
			if (!(uiNode?.SictbarMitErbe ?? false))
				return null;

			return AusGbsAstFalsUnglaicNull(uiNode);
		}

		static public bool AstMitHerkunftAdreseEnthaltAstMitHerkunftAdrese(
			this SictGbsAstInfoSictAuswert suuceWurzel,
			Int64 enthaltendeAstHerkunftAdrese,
			Int64 enthalteneAstHerkunftAdrese)
		{
			if (enthaltendeAstHerkunftAdrese == enthalteneAstHerkunftAdrese)
				return true;

			if (null == suuceWurzel)
				return false;

			var enthaltendeAst = suuceWurzel.SuuceFlacMengeAstFrühesteMitHerkunftAdrese(enthaltendeAstHerkunftAdrese);

			if (null == enthaltendeAst)
				return false;

			return enthaltendeAst.EnthaltAstMitHerkunftAdrese(enthalteneAstHerkunftAdrese);
		}

		static public bool EnthaltAstMitHerkunftAdrese(
			this SictGbsAstInfoSictAuswert suuceWurzel,
			Int64 astHerkunftAdrese) =>
			suuceWurzel?.AstEnthalteInBaum((kandidaatAst) => kandidaatAst?.HerkunftAdrese == astHerkunftAdrese, zuZerleegende => zuZerleegende.ListeChild) ?? false;

		static public bool EnthaltAst(
			this SictGbsAstInfoSictAuswert suuceWurzel,
			SictGbsAstInfoSictAuswert node)
		{
			if (null == node)
				return false;

			return suuceWurzel?.AstEnthalteInBaum(node, zuZerleegende => zuZerleegende.ListeChild) ?? false;
		}

		/// <summary>
		/// Tailmenge der Ast welce kaine andere Ast aus der Menge enthalt.
		/// </summary>
		/// <param name="mengeAstRepr"></param>
		/// <returns></returns>
		static public IEnumerable<T> TailmengeUnterste<T>(
			this IEnumerable<T> mengeAstRepr,
			SictGbsAstInfoSictAuswert uiTree)
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
						var Ast = uiTree.SuuceFlacMengeAstFrühesteMitHerkunftAdrese(astRepr.Id);

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

								var AndereAst = uiTree.SuuceFlacMengeAstFrühesteMitHerkunftAdrese(andereAstRepr.Id);

								if (AndereAst == Ast)
								{
									return false;
								}

								return Ast.EnthaltAst(AndereAst);
							});
					});
		}

		static public float? LaagePlusVonParentErbeLaageA(this SictGbsAstInfoSictAuswert uiNode) =>
			uiNode?.LaagePlusVonParentErbeLaage()?.A;

		static public float? LaagePlusVonParentErbeLaageB(this SictGbsAstInfoSictAuswert uiNode) =>
			uiNode?.LaagePlusVonParentErbeLaage()?.B;

		static public MenuEntry MenuEntry(
			this SictGbsAstInfoSictAuswert menuEntryAst,
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
			this SictGbsAstInfoSictAuswert uiNode,
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

			return new Window(uiNode.AlsUIElementFalsUnglaicNullUndSictbar())
			{
				isModal = isModal,
				Caption = caption,
				HeaderButtonsVisible = headerButtonsVisible,
				HeaderButton = headerButton,
			};
		}
	}
}
