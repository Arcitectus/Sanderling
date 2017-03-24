using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bib3;
using Sanderling.Interface.MemoryStruct;
using Bib3.Geometrik;
using BotEngine.Common;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsListColumnHeader
	{
		static public IColumnHeader Read(UINodeInfoInTree ColumnHeaderAst)
		{
			if (!(ColumnHeaderAst?.VisibleIncludingInheritance ?? false))
				return null;

			var container = ColumnHeaderAst?.AlsContainer(
				treatIconAsSprite: true);

			if (null == container)
				return null;

			return new ColumnHeader(container);
		}
	}

	public enum ListEntryTrenungZeleTypEnum
	{
		Kain = 0,
		InLabelTab = 10,
		Ast = 30,
	}

	public class SictAuswertGbsListEntry
	{
		readonly public IColumnHeader[] ListeColumnHeader;

		readonly public UINodeInfoInTree EntryAst;

		readonly public RectInt? RegionConstraint;

		readonly public ListEntryTrenungZeleTypEnum? TrenungZeleTyp;

		virtual public ListEntryTrenungZeleTypEnum ListEntryInZeleTrenung()
		{
			return TrenungZeleTyp ?? ListEntryTrenungZeleTypEnum.InLabelTab;
		}

		public UINodeInfoInTree LabelAst
		{
			private set;
			get;
		}

		public int? LabelGrenzeLinx
		{
			private set;
			get;
		}

		public UINodeInfoInTree ExpanderAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree FläceFürMenuAst
		{
			private set;
			get;
		}

		public bool? IstGroup
		{
			private set;
			get;
		}

		public bool? IstItem
		{
			private set;
			get;
		}

		public int? InhaltGrenzeLinx
		{
			private set;
			get;
		}

		/// <summary>
		/// 2013.08.29 Bsp:
		/// "Scourge Heavy Missile<t><right>4.679<t>Heavy Missile<t><t><t><right>140,37 m3"
		/// </summary>
		IUIElementText Label;

		public string BescriftungTailTitel
		{
			private set;
			get;
		}

		public int? BescriftungQuantitäät
		{
			private set;
			get;
		}

		public string[] ItemListeZeleTextMitFormat
		{
			private set;
			get;
		}

		public KeyValuePair<IColumnHeader, string>[] ListeZuHeaderZeleString
		{
			private set;
			get;
		}

		public IListEntry ErgeebnisListEntry
		{
			private set;
			get;
		}

		public string ZeleStringZuHeaderMitBescriftungRegexPattern(
			string headerBescriftungRegexPattern,
			RegexOptions regexOptions = RegexOptions.None)
		{
			return ZeleStringZuHeaderMitBescriftungRegex(new Regex(headerBescriftungRegexPattern, regexOptions));
		}

		public string ZeleStringZuHeaderMitBescriftungRegex(
			Regex headerBescriftungRegex)
		{
			if (null == headerBescriftungRegex)
				return null;

			var ListeZuHeaderZeleString = this.ListeZuHeaderZeleString;

			if (null == ListeZuHeaderZeleString)
				return null;

			foreach (var ZuHeaderZeleString in ListeZuHeaderZeleString)
			{
				if (null == ZuHeaderZeleString.Key.Text)
					continue;

				var Match = headerBescriftungRegex.Match(ZuHeaderZeleString.Key.Text);

				if (Match.Success)
					return ZuHeaderZeleString.Value;
			}

			return null;
		}

		static readonly KeyValuePair<string, bool>[] MengeZuTexturePathBedoitungIsExpanded =
			new KeyValuePair<string, bool>[]{

				/*
				 * Aus 2015.02.26	Beobactung mit PyObjBeobact in SurveyScanListEntry.Expander Dict entry "texturePath"
				 * */

				//	collapsed:
				new KeyValuePair<string,bool>("res:/UI/Texture/Icons/38_16_228.png", false),

				//	expanded:
				new KeyValuePair<string,bool>("res:/UI/Texture/Icons/38_16_229.png", true),
		};

		static bool? ExpanderSpriteIsExpanded(UINodeInfoInTree expanderSprite)
		{
			if (null == expanderSprite)
				return null;

			var texturePath = expanderSprite.texturePath;

			if (texturePath.IsNullOrEmpty())
				return null;

			var FunkPasend = new Func<KeyValuePair<string, bool>, bool>((kandidaat) =>
				{
					return string.Equals(kandidaat.Key, texturePath, StringComparison.InvariantCultureIgnoreCase);
				});

			var ZuTexturePathBedoitungIsExpanded =
				MengeZuTexturePathBedoitungIsExpanded
				.FirstOrDefault(FunkPasend);

			if (FunkPasend(ZuTexturePathBedoitungIsExpanded))
				return ZuTexturePathBedoitungIsExpanded.Value;

			return null;
		}

		const string RegexGrupeInKlamerNaame = "inklamer";

		/// <summary>
		/// 2014.09.08	Bsp:
		/// "Personal Locations [7]"
		/// "Drones in Bay (4)"
		/// </summary>
		const string GroupBescriftungZerleegungRegexPattern = "([^\\(\\[]+)((\\(|\\[)(?<" + RegexGrupeInKlamerNaame + ">[^\\)\\]]+)(\\)|\\])|)";

		static public string ZeleTextAusZeleTextMitFormat(string zeleTextMitFormat)
		{
			if (null == zeleTextMitFormat)
				return null;

			var ZeleText = zeleTextMitFormat.RemoveXmlTag();

			return ZeleText;
		}

		public SictAuswertGbsListEntry(
			UINodeInfoInTree entryAst,
			IColumnHeader[] listeScrollHeader,
			RectInt? regionConstraint,
			ListEntryTrenungZeleTypEnum? trenungZeleTyp = null)
		{
			EntryAst = entryAst;
			ListeColumnHeader = listeScrollHeader;
			RegionConstraint = regionConstraint;
			TrenungZeleTyp = trenungZeleTyp;
		}

		static public Int64 ÜberlapungBetraag(IUIElement uIElement, IUIElement header) =>
			Math.Min(uIElement?.Region.Max0 ?? int.MinValue, header?.Region.Max0 ?? int.MinValue) -
			Math.Max(uIElement?.Region.Min0 ?? int.MaxValue, header?.Region.Min0 ?? int.MaxValue);

		static public T HeaderBestFit<T>(
			IUIElement zeleUIElement,
			IEnumerable<T> mengeKandidaatHeader)
			where T : class, IUIElement =>
			mengeKandidaatHeader
			?.Select(kandidaat => new { Kandidaat = kandidaat, ÜberlapungBetraag = ÜberlapungBetraag(zeleUIElement, kandidaat) })
			?.Where(kandidaatUndÜberlapungBetraag => 3 < kandidaatUndÜberlapungBetraag.ÜberlapungBetraag)
			?.OrderByDescending(kandidaatUndÜberlapungBetraag => kandidaatUndÜberlapungBetraag.ÜberlapungBetraag)
			?.FirstOrDefault()?.Kandidaat;

		virtual public void Berecne()
		{
			var Container = EntryAst.AlsContainer(regionConstraint: RegionConstraint);

			if (null == Container)
				return;

			LabelAst = EntryAst.LargestLabelInSubtree();

			FläceFürMenuAst = LabelAst;

			LabelGrenzeLinx = (int?)LabelAst.LaagePlusVonParentErbeLaageA();

			/*
			 * 2014.11.08
			 * 
			 * Änderung für Release "Rhea":
			 * "PyObjTypName": "GlowSprite"
			 * 
			ExpanderAst =
				EntryAst.SuuceFlacMengeAstFrüheste(
				(Kandidaat) =>
					AuswertGbs.Glob.GbsAstTypeIstEveIcon(Kandidaat) &&
					Regex.Match(Kandidaat.Name	?? "", "expander", RegexOptions.IgnoreCase).Success);
			 * */
			ExpanderAst =
					EntryAst.FirstMatchingNodeFromSubtreeBreadthFirst(
					(kandidaat) =>
						(AuswertGbs.Glob.GbsAstTypeIstEveIcon(kandidaat) ||
						AuswertGbs.Glob.GbsAstTypeIstSprite(kandidaat)) &&
						Regex.Match(kandidaat.Name ?? "", "expander", RegexOptions.IgnoreCase).Success);

			IstGroup = ((null == ExpanderAst) ? null : ExpanderAst.VisibleIncludingInheritance) ?? false;

			var MengeIconOderSpriteAst =
				EntryAst.MatchingNodesFromSubtreeBreadthFirst(
				(kandidaat) =>
					(AuswertGbs.Glob.GbsAstTypeIstEveIcon(kandidaat) ||
					Regex.Match(kandidaat.PyObjTypName ?? "", "sprite", RegexOptions.IgnoreCase).Success) &&
					(kandidaat.VisibleIncludingInheritance ?? false));

			IstItem =
				(!MengeIconOderSpriteAst.IsNullOrEmpty() || null != LabelAst) &&
				!(IstGroup ?? false);

			var MengeIconOderSpriteGrenzeLinx =
				MengeIconOderSpriteAst
				?.Select((iconAst) => iconAst.LaagePlusVonParentErbeLaageA())
				?.Where((kandidaat) => kandidaat.HasValue)
				?.Select((iconAstGrenzeLinx) => iconAstGrenzeLinx.Value)
				?.ToArray();

			if (!MengeIconOderSpriteGrenzeLinx.IsNullOrEmpty())
				InhaltGrenzeLinx = (int)Math.Min(InhaltGrenzeLinx ?? int.MaxValue, MengeIconOderSpriteGrenzeLinx.Min());

			if (LabelGrenzeLinx.HasValue)
				InhaltGrenzeLinx = (int)Math.Min(InhaltGrenzeLinx ?? int.MaxValue, LabelGrenzeLinx.Value);

			var ListEntryInZeleTrenung = this.ListEntryInZeleTrenung();

			if (ListEntryTrenungZeleTypEnum.Ast == ListEntryInZeleTrenung)
			{
				//	genuzt z.B. für "OverviewScrollEntry".
				var EntryListeZeleLabel =
					EntryAst.ExtraktMengeLabelString()?.ToArray();

				ListeZuHeaderZeleString =
					EntryListeZeleLabel
					?.Select(zeleLabel => new KeyValuePair<IColumnHeader, string>(
						HeaderBestFit(zeleLabel, ListeColumnHeader),
						zeleLabel?.Text))
					?.ToArray();
			}
			else
			{
				Label = LabelAst.AsUIElementTextIfTextNotEmpty();

				var Bescriftung = Label?.Text;

				if (null != Bescriftung)
				{
					if (!(IstGroup ?? false))
					{
						ItemListeZeleTextMitFormat = Bescriftung.Split(new string[] { "<t>" }, StringSplitOptions.None);

						var ItemListeZeleText =
							ItemListeZeleTextMitFormat
							?.Select((zeleTextMitFormat) => ZeleTextAusZeleTextMitFormat(zeleTextMitFormat))
							?.ToArray();

						ListeZuHeaderZeleString =
							ItemListeZeleText
							?.Select((zeleText, index) => new KeyValuePair<IColumnHeader, string>(
								ListeColumnHeader?.FirstOrDefault(kandidaat => kandidaat.ColumnIndex == index),
								zeleText))
							?.ToArray();
					}
				}
			}

			var backgroundColorNode =
				EntryAst?.FirstMatchingNodeFromSubtreeBreadthFirst(node =>
					(node?.PyObjTypNameMatchesRegexPatternIgnoreCase("fill") ?? false) &&
					(node?.Name?.RegexMatchSuccessIgnoreCase("bgColor") ?? false));

			var ListBackgroundColor =
				EntryAst?.BackgroundList
				?.Select(background => background.Color.AlsColorORGB())
				.ConcatNullable(new[] { backgroundColorNode?.Color.AlsColorORGB() })
				?.WhereNotDefault()
				?.ToArrayIfNotEmpty();

			var SetSprite =
				EntryAst.SetSpriteFromChildren()
				?.ToArrayIfNotEmpty();

			ErgeebnisListEntry = new ListEntry(Container)
			{
				ContentBoundLeft = InhaltGrenzeLinx,

				ListColumnCellLabel = ListeZuHeaderZeleString,

				GroupExpander = ExpanderAst?.AsUIElementIfVisible(),

				IsGroup = IstGroup,
				IsExpanded = ExpanderSpriteIsExpanded(ExpanderAst),
				IsSelected = EntryAst.isSelected,

				ListBackgroundColor = ListBackgroundColor,
				SetSprite = SetSprite,
			};
		}
	}

	public class SictAuswertGbsListViewport<EntryT>
		where EntryT : class, IListEntry
	{
		readonly public IAusGbsAstExtraktor EntryExtraktor;

		readonly public UINodeInfoInTree ListViewNode;

		readonly public Func<UINodeInfoInTree, IColumnHeader[], RectInt?, EntryT> CallbackListEntryConstruct;

		readonly public ListEntryTrenungZeleTypEnum? InEntryTrenungZeleTyp;

		public UINodeInfoInTree ScrollHeadersAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree ScrollClipperAst
		{
			private set;
			get;
		}

		public UINodeInfoInTree ScrollClipperContentNode
		{
			private set;
			get;
		}

		public SictAuswertGbsListEntry[] ScrollClipperContentMengeKandidaatEntryAuswert
		{
			private set;
			get;
		}

		public UINodeInfoInTree[] ListeColumnHeaderAst
		{
			private set;
			get;
		}

		public SictAuswertGbsListColumnHeader[] ListeColumnHeaderAuswert
		{
			private set;
			get;
		}

		public IColumnHeader[] ListeHeader
		{
			private set;
			get;
		}

		public ListViewAndControl<EntryT> Result
		{
			private set;
			get;
		}

		static public ListViewAndControl<EntryT> ReadListView(
			UINodeInfoInTree listViewNode,
			Func<UINodeInfoInTree, IColumnHeader[], RectInt?, EntryT> callbackListEntryConstruct,
			ListEntryTrenungZeleTypEnum? inEntryTrenungZeleTyp = null)
		{
			var auswert = new SictAuswertGbsListViewport<EntryT>(listViewNode, callbackListEntryConstruct, inEntryTrenungZeleTyp);

			auswert.Read();

			return auswert.Result;
		}

		static public IListEntry ListEntryKonstruktSctandard(
			UINodeInfoInTree uiNode,
			IColumnHeader[] header,
			RectInt? regionConstraint,
			ListEntryTrenungZeleTypEnum? trenungZeleTyp)
		{
			if (!(uiNode?.VisibleIncludingInheritance ?? false))
				return null;

			var Auswert = new SictAuswertGbsListEntry(uiNode, header, regionConstraint, trenungZeleTyp);

			Auswert.Berecne();

			return Auswert.ErgeebnisListEntry;
		}

		static public IListEntry ListEntryKonstruktSctandard(
			UINodeInfoInTree gbsAst,
			IColumnHeader[] header,
			RectInt? regionConstraint) =>
			ListEntryKonstruktSctandard(gbsAst, header, regionConstraint, null);

		public SictAuswertGbsListViewport(
			UINodeInfoInTree listViewNode,
			Func<UINodeInfoInTree, IColumnHeader[], RectInt?, EntryT> callbackListEntryConstruct,
			ListEntryTrenungZeleTypEnum? inEntryTrenungZeleTyp = null)
		{
			this.ListViewNode = listViewNode;

			this.CallbackListEntryConstruct = callbackListEntryConstruct;

			this.InEntryTrenungZeleTyp = inEntryTrenungZeleTyp;
		}

		public void Read()
		{
			if (!(ListViewNode?.VisibleIncludingInheritance ?? false))
				return;

			var scrollReader = new ScrollReader(ListViewNode);
			scrollReader.Read();

			var scroll = scrollReader.Result;

			if (null == scroll)
				return;

			ListeHeader = scroll.ColumnHeader;

			var ListeHeaderInfo =
				ListeHeader
				?.Select((header, index) => new ColumnHeader(header) { ColumnIndex = index })
				?.ToArray();

			ScrollClipperContentNode = scrollReader.ClipperContentNode;

			var clipperRegion = ScrollClipperContentNode?.AsUIElementIfVisible()?.Region;

			var listEntry =
				ScrollClipperContentNode?.ListChild
				?.Select(kandidaatEntryAst => CallbackListEntryConstruct(kandidaatEntryAst, ListeHeaderInfo, clipperRegion))
				?.WhereNotDefault()
				?.OrderBy(entry => entry.Region.Center().B)
				?.ToArray();

			Result = new ListViewAndControl<EntryT>(ListViewNode.AsUIElementIfVisible())
			{
				ColumnHeader = ListeHeader,
				Entry = listEntry,
				Scroll = scroll,
			};
		}
	}
}
