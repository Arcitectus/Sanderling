using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using BotEngine.Interface;
using Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;
using Sanderling.MemoryReading.Production;

namespace Optimat.EveOnline.AuswertGbs
{
	static public class Extension
	{
		static public Vektor2DInt AlsVektor2DInt(
			this Vektor2DSingle vektor2DSingle) =>
			new Vektor2DInt((Int64)vektor2DSingle.A, (Int64)vektor2DSingle.B);

		static readonly Bib3.RefNezDiferenz.SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer
			KonvertGbsAstInfoRictliinieMitScatescpaicer =
			new Bib3.RefNezDiferenz.SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer(
				Bib3.RefNezDiferenz.NewtonsoftJson.SictMengeTypeBehandlungRictliinieNewtonsoftJson.KonstruktMengeTypeBehandlungRictliinie(
				new KeyValuePair<Type, Type>[]{
					new KeyValuePair<Type, Type>(typeof(GbsAstInfo), typeof(UINodeInfoInTree)),
					new KeyValuePair<Type, Type>(typeof(GbsAstInfo[]), typeof(UINodeInfoInTree[])),
				}));

		static public UINodeInfoInTree SictAuswert(
			this GbsAstInfo gbsBaum)
		{
			if (null == gbsBaum)
				return null;

			var GbsBaumScpez =
				SictRefNezKopii.ObjektKopiiErsctele(
				gbsBaum,
				null,
				new Bib3.RefBaumKopii.Param(null, KonvertGbsAstInfoRictliinieMitScatescpaicer),
				null,
				null)
				as UINodeInfoInTree;

			if (null == GbsBaumScpez)
				return null;

			int InBaumAstIndexZääler = 0;

			GbsBaumScpez.AbgelaiteteAigescafteBerecne(ref InBaumAstIndexZääler);

			return GbsBaumScpez;
		}

		static public IMemoryMeasurement SensorikScnapscusKonstrukt(
			this Optimat.EveOnline.GbsAstInfo gbsBaum,
			int? sessionDurationRemaining)
		{
			var GbsBaumSictAuswert = gbsBaum.SictAuswert();

			var Auswert = new SictAuswertGbsAgr(GbsBaumSictAuswert);

			Auswert.Berecne(sessionDurationRemaining);

			return Auswert.AuswertErgeebnis;
		}


		static public IEnumerable<UINodeInfoInTree> BaumEnumFlacListeKnoote(
			this UINodeInfoInTree suuceWurzel,
			int? tiifeMax = null,
			int? tiifeMin = null)
		{
			return
				suuceWurzel.EnumerateNodeFromTreeBFirst(
				node => node?.GetListChild()?.OfType<UINodeInfoInTree>(),
				tiifeMax,
				tiifeMin);
		}

		static public Vektor2DSingle? LaagePlusVonParentErbeLaage(
			this UINodeInfoInTree node)
		{
			var VonParentErbeLaage = node?.FromParentLocation;

			if (!VonParentErbeLaage.HasValue)
				return node.LaageInParent;

			return node.LaageInParent + VonParentErbeLaage;
		}

		static public string LabelText(
			this UINodeInfoInTree node) => node?.SetText;

		static public void AbgelaiteteAigescafteBerecne(
			this UINodeInfoInTree node,
			ref int inBaumAstIndexZääler,
			int? tiifeMax = null,
			Vektor2DSingle? vonParentErbeLaage = null,
			float? vonParentErbeClippingFläceLinx = null,
			float? vonParentErbeClippingFläceOobn = null,
			float? vonParentErbeClippingFläceRecz = null,
			float? vonParentErbeClippingFläceUntn = null)
		{
			if (null == node)
				return;

			if (tiifeMax < 0)
				return;

			node.InTreeIndex = ++inBaumAstIndexZääler;
			node.FromParentLocation = vonParentErbeLaage;

			var FürChildVonParentErbeLaage = node.LaageInParent;

			var LaagePlusVonParentErbeLaage = node.LaagePlusVonParentErbeLaage();
			var Grööse = node.Grööse;

			var FürChildVonParentErbeClippingFläceLinx = vonParentErbeClippingFläceLinx;
			var FürChildVonParentErbeClippingFläceOobn = vonParentErbeClippingFläceOobn;
			var FürChildVonParentErbeClippingFläceRecz = vonParentErbeClippingFläceRecz;
			var FürChildVonParentErbeClippingFläceUntn = vonParentErbeClippingFläceUntn;

			if (LaagePlusVonParentErbeLaage.HasValue && Grööse.HasValue)
			{
				FürChildVonParentErbeClippingFläceLinx = Bib3.Glob.Max(FürChildVonParentErbeClippingFläceLinx, LaagePlusVonParentErbeLaage.Value.A);
				FürChildVonParentErbeClippingFläceRecz = Bib3.Glob.Min(FürChildVonParentErbeClippingFläceRecz, LaagePlusVonParentErbeLaage.Value.A);
				FürChildVonParentErbeClippingFläceOobn = Bib3.Glob.Max(FürChildVonParentErbeClippingFläceOobn, LaagePlusVonParentErbeLaage.Value.B);
				FürChildVonParentErbeClippingFläceUntn = Bib3.Glob.Min(FürChildVonParentErbeClippingFläceUntn, LaagePlusVonParentErbeLaage.Value.B);
			}

			if (vonParentErbeLaage.HasValue)
			{
				if (FürChildVonParentErbeLaage.HasValue)
				{
					FürChildVonParentErbeLaage = FürChildVonParentErbeLaage.Value + vonParentErbeLaage.Value;
				}
				else
				{
					FürChildVonParentErbeLaage = vonParentErbeLaage;
				}
			}

			var ListeChild = node.ListChild;

			for (int ChildIndex = 0; ChildIndex < ListeChild?.Length; ChildIndex++)
			{
				var Child = ListeChild[ChildIndex];

				if (null == Child)
					continue;

				Child.InParentListChildIndex = ChildIndex;
				Child.AbgelaiteteAigescafteBerecne(
					ref inBaumAstIndexZääler,
					tiifeMax - 1,
					FürChildVonParentErbeLaage,
					FürChildVonParentErbeClippingFläceLinx,
					FürChildVonParentErbeClippingFläceOobn,
					FürChildVonParentErbeClippingFläceRecz,
					FürChildVonParentErbeClippingFläceUntn);
			}

			var MengeChildInBaumAstIndex =
				ListeChild
				?.Select(child => child?.ChildLastInTreeIndex ?? child?.InTreeIndex)
				?.WhereNotDefault()
				?.ToArray();

			if (0 < MengeChildInBaumAstIndex?.Length)
			{
				node.ChildLastInTreeIndex = MengeChildInBaumAstIndex.Max();
			}
		}

		static public UINodeInfoInTree SuuceFlacMengeAstFrüheste(
			this UINodeInfoInTree[] suuceMengeWurzel,
			Func<UINodeInfoInTree, bool> prädikaat,
			int? tiifeMax = null,
			int? tiifeMin = null)
		{
			foreach (var Wurzel in suuceMengeWurzel.EmptyIfNull())
			{
				var Fund = Wurzel.FirstMatchingNodeFromSubtreeBreadthFirst(prädikaat, tiifeMax, tiifeMin);

				if (null != Fund)
					return Fund;
			}

			return null;
		}

		static public T Grööste<T>(
			this IEnumerable<T> source)
			where T : class, IUIElement =>
			source?.OrderByDescending(element => element.Region.Area())?.FirstOrDefault();

		static public T LargestNodeInSubtree<T>(
			this IEnumerable<T> source)
			where T : GbsAstInfo =>
			source?.OrderByDescending(element => (element.GrööseA * element.GrööseB) ?? int.MinValue)?.FirstOrDefault();

		static public T GröösteSpriteAst<T>(
			this IEnumerable<T> source)
			where T : GbsAstInfo =>
			source?.Where(k => k.PyObjTypNameIsSprite())
			?.LargestNodeInSubtree();

		static public UINodeInfoInTree LargestLabelInSubtree(
			this UINodeInfoInTree rootNode,
			int? tiifeMax = null)
		{
			var mengeLabelSictbar =
				rootNode.MatchingNodesFromSubtreeBreadthFirst(kandidaat => kandidaat.GbsAstTypeIstLabel(), null, tiifeMax);

			UINodeInfoInTree bisherBeste = null;

			foreach (var LabelAst in mengeLabelSictbar.EmptyIfNull())
			{
				var labelAstGrööse = LabelAst?.Grööse;

				if (!labelAstGrööse.HasValue)
					continue;

				if ((bisherBeste?.Grööse.Value.BetraagQuadriirt ?? -1) < labelAstGrööse.Value.BetraagQuadriirt)
					bisherBeste = LabelAst;
			}

			return bisherBeste;
		}

		static public UINodeInfoInTree[] MatchingNodesFromSubtreeBreadthFirst(
			this UINodeInfoInTree rootNode,
			Func<UINodeInfoInTree, bool> predicate,
			int? resultCountMax = null,
			int? depthBoundMax = null,
			int? depthBoundMin = null,
			bool omitNodesBelowNodesMatchingPredicate = false)	=>
			rootNode.ListPathToNodeFromSubtreeBreadthFirst(
				predicate,
				resultCountMax,
				depthBoundMax,
				depthBoundMin,
				omitNodesBelowNodesMatchingPredicate)
				?.Select(astMitPfaad => astMitPfaad.LastOrDefault()).ToArray();

		static public UINodeInfoInTree FirstNodeWithPyObjAddressFromSubtreeBreadthFirst(
			this UINodeInfoInTree node,
			Int64? pyObjAddress,
			int? depthBoundMax = null,
			int? depthBoundMin = null) =>
			node.FirstMatchingNodeFromSubtreeBreadthFirst(
				kandidaat => kandidaat.PyObjAddress == pyObjAddress,
				depthBoundMax,
				depthBoundMin);

		static public UINodeInfoInTree FirstMatchingNodeFromSubtreeBreadthFirst(
			this UINodeInfoInTree rootNode,
			Func<UINodeInfoInTree, bool> predicate,
			int? depthBoundMax = null,
			int? depthBoundMin = null) =>
			rootNode?.MatchingNodesFromSubtreeBreadthFirst(predicate, 1, depthBoundMax, depthBoundMin, true)
			?.FirstOrDefault();

		static public UINodeInfoInTree[][] ListPathToNodeFromSubtreeBreadthFirst(
			this UINodeInfoInTree rootNode,
			Func<UINodeInfoInTree, bool> predicate,
			int? ListeFundAnzaalScrankeMax = null,
			int? depthBoundMax = null,
			int? depthBoundMin = null,
			bool omitNodesBelowNodesMatchingPredicate = false)
		{
			if (null == rootNode)
				return null;

			return Bib3.Glob.SuuceFlacMengeAstMitPfaad(
				rootNode,
				predicate,
				node => node.ListChild,
				ListeFundAnzaalScrankeMax,
				depthBoundMax,
				depthBoundMin,
				omitNodesBelowNodesMatchingPredicate);
		}

		static public Vektor2DSingle? GrööseMaxAusListeChild(
			this UINodeInfoInTree Ast)
		{
			if (null == Ast)
			{
				return null;
			}

			Vektor2DSingle? GrööseMax = null;

			var ThisGrööse = Ast.Grööse;

			if (ThisGrööse.HasValue)
			{
				GrööseMax = ThisGrööse;
			}

			var ListeChild = Ast.ListChild;

			if (null != ListeChild)
			{
				foreach (var Child in ListeChild)
				{
					var ChildGrööse = Child.Grööse;

					if (ChildGrööse.HasValue)
					{
						if (GrööseMax.HasValue)
						{
							GrööseMax = new Vektor2DSingle(
								Math.Max(GrööseMax.Value.A, ChildGrööse.Value.A),
								Math.Max(GrööseMax.Value.B, ChildGrööse.Value.B));
						}
						else
						{
							GrööseMax = ChildGrööse;
						}
					}
				}
			}

			return GrööseMax;
		}

		static string[] UIRootVorgaabeGrööseListeName = new string[] { "l_main", "l_viewstate" };

		static public Vektor2DSingle? GrööseAusListeChildFürScpezUIRootBerecne(
			this UINodeInfoInTree Ast)
		{
			if (null == Ast)
			{
				return null;
			}

			var ListeChild = Ast.ListChild;

			if (null != ListeChild)
			{
				foreach (var Child in ListeChild)
				{
					var ChildGrööse = Child.Grööse;

					if (ChildGrööse.HasValue)
					{
						if (UIRootVorgaabeGrööseListeName.Any((AstNaame) => string.Equals(AstNaame, Child.Name)))
						{
							return ChildGrööse;
						}
					}
				}
			}

			return null;
		}

		static public IUIElementText AsUIElementText(this UINodeInfoInTree GbsAst) =>
			(GbsAst?.VisibleIncludingInheritance ?? false) ? new UIElementText(GbsAst.AsUIElementIfVisible(), GbsAst.LabelText() ?? GbsAst.Text) : null;

		static public IUIElementInputText AsUIElementInputText(this UINodeInfoInTree GbsAst)
		{
			var UIElementText = GbsAst?.AsUIElementText();

			return null == UIElementText ? null : new UIElementInputText(UIElementText);
		}

		static public IUIElementText AsUIElementTextIfTextNotEmpty(
			this UINodeInfoInTree GbsAst)
		{
			var UIElementText = GbsAst?.AsUIElementText();

			if ((UIElementText?.Text).IsNullOrEmpty())
				return null;

			return UIElementText;
		}

		static public IEnumerable<IUIElementText> ExtraktMengeLabelString(
			this UINodeInfoInTree GbsAst) =>
			GbsAst?.MatchingNodesFromSubtreeBreadthFirst(kandidaat => kandidaat?.VisibleIncludingInheritance ?? false)
			?.Select(AsUIElementTextIfTextNotEmpty)
			?.WhereNotDefault();

		static public IEnumerable<IUIElementText> ExtraktMengeButtonLabelString(
			this UINodeInfoInTree GbsAst) =>
			GbsAst?.MatchingNodesFromSubtreeBreadthFirst(kandidaat => (kandidaat?.VisibleIncludingInheritance ?? false) &&
			Regex.Match(kandidaat?.PyObjTypName ?? "", "button", RegexOptions.IgnoreCase).Success)
			?.Select(kandidaatButtonAst => new { ButtonAst = kandidaatButtonAst, LabelAst = kandidaatButtonAst.LargestLabelInSubtree() })
			?.GroupBy(buttonAstUndLabelAst => buttonAstUndLabelAst.LabelAst)
			?.Select(GroupLabelAst => new
			{
				ButtonAst = GroupLabelAst.Select(buttonAstUndLabelAst => buttonAstUndLabelAst.ButtonAst).OrderBy(kandidaatButtonAst => kandidaatButtonAst.InTreeIndex).LastOrDefault(),
				LabelAst = GroupLabelAst.Key
			})
			?.Select(buttonAstUndLabelAst => new UIElementText(buttonAstUndLabelAst.ButtonAst.AsUIElementIfVisible(),
				buttonAstUndLabelAst?.LabelAst?.LabelText()))
			?.Where(kandidaat => !(kandidaat?.Text).IsNullOrEmpty());

		static public IEnumerable<T> OrdnungLabel<T>(
			this IEnumerable<T> Menge)
			where T : IUIElement =>
			Menge
			?.OrderBy(element => ((element?.Region)?.Center())?.B ?? int.MaxValue)
			?.ThenBy(element => ((element?.Region)?.Center())?.A ?? int.MaxValue);

		static public Sprite AlsSprite(
			this UINodeInfoInTree GbsAst) =>
			!(GbsAst?.VisibleIncludingInheritance ?? false) ? null :
			new Sprite(GbsAst.AsUIElementIfVisible())
			{
				Name = GbsAst?.Name,
				Color = GbsAst?.Color.AsColorORGBIfAnyHasValue(),
				Texture0Id = GbsAst?.TextureIdent0?.AsObjectIdInMemory(),
				HintText = GbsAst?.Hint,
				TexturePath = GbsAst?.texturePath,
			};

		static public ListViewAndControl<EntryT> AlsListView<EntryT>(
			this UINodeInfoInTree ListViewportAst,
			Func<UINodeInfoInTree, IColumnHeader[], RectInt?, EntryT> CallbackListEntryConstruct = null,
			ListEntryTrenungZeleTypEnum? InEntryTrenungZeleTyp = null)
			where EntryT : class, IListEntry
		{
			var Auswert = new SictAuswertGbsListViewport<EntryT>(
				ListViewportAst,
				CallbackListEntryConstruct,
				InEntryTrenungZeleTyp);

			Auswert.Read();

			return Auswert?.Result;
		}

		static public IUIElement WithRegionConstrainedToIntersection(
			this IUIElement original,
			RectInt constraint) =>
			original?.WithRegion(original.Region.Intersection(constraint));

		static public Container AlsContainer(
			this UINodeInfoInTree containerNode,
			bool treatIconAsSprite = false,
			RectInt? regionConstraint = null)
		{
			if (!(containerNode?.VisibleIncludingInheritance ?? false))
				return null;

			var MengeKandidaatInputTextAst =
				containerNode?.MatchingNodesFromSubtreeBreadthFirst(k =>
					k.PyObjTypNameMatchesRegexPatternIgnoreCase("SinglelineEdit|QuickFilterEdit"));

			var ListeInputText =
				MengeKandidaatInputTextAst
				?.Select(textBoxAst =>
				{
					var LabelAst = textBoxAst.LargestLabelInSubtree();

					if (null == LabelAst)
						return null;

					var LabelText = LabelAst?.LabelText();

					return new UIElementInputText(textBoxAst.AsUIElementIfVisible(), LabelText);
				})
				?.WhereNotDefault()
				?.OrdnungLabel()
				?.ToArrayIfNotEmpty();

			var ListeButton =
				containerNode?.ExtraktMengeButtonLabelString()?.OrdnungLabel()
				?.ToArrayIfNotEmpty();

			var ListeButtonAst = ListeButton?.Select(button => containerNode.FirstNodeWithPyObjAddressFromSubtreeBreadthFirst(button.Id))?.ToArray();

			var ListeTextBoxAst = ListeInputText?.Select(textBox => containerNode.FirstNodeWithPyObjAddressFromSubtreeBreadthFirst(textBox.Id))?.ToArray();

			var LabelContainerAussclus = new[] { ListeButtonAst, ListeTextBoxAst }.ConcatNullable().ToArray();

			var ListeLabelText =
				containerNode?.ExtraktMengeLabelString()
				?.WhereNitEnthalte(LabelContainerAussclus)
				?.OrdnungLabel()
				?.ToArrayIfNotEmpty();

			var setSprite =
				containerNode.SetSpriteFromChildren(treatIconAsSprite)
				?.OrdnungLabel()
				?.ToArrayIfNotEmpty();

			var baseElement = containerNode.AsUIElementIfVisible();

			if (regionConstraint.HasValue)
				baseElement = baseElement.WithRegionConstrainedToIntersection(regionConstraint.Value);

			return new Container(baseElement)
			{
				ButtonText = ListeButton,
				InputText = ListeInputText,
				LabelText = ListeLabelText,
				Sprite = setSprite,
			};
		}

		static public IEnumerable<ISprite> SetSpriteFromChildren(
			this UINodeInfoInTree uiNode,
			bool treatIconAsSprite = false) =>
			uiNode?.MatchingNodesFromSubtreeBreadthFirst(c =>
				(c?.PyObjTypNameIsSprite() ?? false) ||
				(treatIconAsSprite && (c?.PyObjTypNameIsIcon() ?? false)), null, null, null, true)
				?.Select(spriteNode => spriteNode?.AlsSprite())
				?.WhereNotDefault();

		static public IInSpaceBracket AsInSpaceBracket(this UINodeInfoInTree node)
		{
			var container = node?.AlsContainer();

			if (null == container)
				return null;

			return new InSpaceBracket(container)
			{
				Name = node.Name,
			};
		}

		/// <summary>
		/// Diis werd verwand mit LayerUtilmenu, daher prüüfung Sictbarkait nit ausraicend.
		/// </summary>
		/// <param name="GbsAst"></param>
		/// <returns></returns>
		static public IContainer AlsUtilmenu(this UINodeInfoInTree GbsAst)
		{
			//	2015.09.08:	PyObjTypName	= "ExpandedUtilMenu"

			var AstExpanded =
				GbsAst?.FirstMatchingNodeFromSubtreeBreadthFirst(k => k.PyObjTypNameMatchesRegexPatternIgnoreCase("ExpandedUtilMenu"));

			return AstExpanded?.AlsContainer();
		}

		static public Func<Int64, IEnumerable<KeyValuePair<string, SictAuswertPythonObj>>> FunkEnumDictEntry;

		static public IEnumerable<T> WhereNitEnthalte<T, AstT>(
			this IEnumerable<T> MengeKandidaat,
			IEnumerable<AstT> MengeContainerZuMaide)
			where T : IObjectIdInMemory
			where AstT : GbsAstInfo =>
			MengeKandidaat?.Where(Kandidaat => !(MengeContainerZuMaide?.Any(ContainerZuMaide =>
			new AstT[] { ContainerZuMaide }.ConcatNullable(ContainerZuMaide.MengeChildAstTransitiiveHüle()).Any(ContainerZuMaideChild => ContainerZuMaideChild.PyObjAddress == Kandidaat.Id)) ?? false));

		static public ColorORGB AlsColorORGB(this ColorORGBVal? Color) =>
			Color.HasValue ? new ColorORGB(Color) : null;

		static public IEnumerable<NodeT> OrderByRegionSizeDescending<NodeT>(this IEnumerable<NodeT> seq)
			where NodeT : GbsAstInfo => seq?.OrderByDescending(node => node?.Grööse?.Betraag ?? -1);
	}
}
