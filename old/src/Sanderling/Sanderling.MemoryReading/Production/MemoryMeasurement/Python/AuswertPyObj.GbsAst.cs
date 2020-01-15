using System;
using System.Collections.Generic;
using System.Linq;

namespace Optimat.EveOnline
{
	public class SictAuswertPyObj32GbsAstZuusctand : SictAuswertPyObj32MitBaiPlus8RefDictZuusctand
	{
		public const int AusDictRenderObjectRefVersazNaacRenderObjectBlok = -8;

		readonly public List<SictAuswertPyObj32GbsAstZuusctand> ListeChild = new List<SictAuswertPyObj32GbsAstZuusctand>();

		readonly public GbsAstInfo AstInfo = new GbsAstInfo();

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstSclüselParentRefString)]
		public SictAuswertPyObj32Zuusctand DictEntryParentRef;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstSclüselChildrenString)]
		public SictAuswertPyObj32Zuusctand DictEntryChildren;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstSclüselRenderObjectString)]
		public SictAuswertPyObj32Zuusctand DictEntryRenderObject;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstSclüselNameString)]
		public SictAuswertPyObj32Zuusctand DictEntryName;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstSclüselLastStateString)]
		public SictAuswertPyObj32Zuusctand DictEntryLastState;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstSclüselLastValueString)]
		public SictAuswertPyObj32Zuusctand DictEntryLastValue;

		/// <summary>
		/// 2015.08.26
		/// Beobactung in Singularity in Type "CapacitorContainer" in ShipUi.
		/// </summary>
		[SictInPyDictEntryKeyAttribut("lastSetCapacitor")]
		public SictAuswertPyObj32Zuusctand DictEntryLastSetCapacitor;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstSclüselRotationString)]
		public SictAuswertPyObj32Zuusctand DictEntryRotation;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstSclüselTextureString)]
		public SictAuswertPyObj32Zuusctand DictEntryTexture;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstSclüselColorString)]
		public SictAuswertPyObj32Zuusctand DictEntryColor;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstSclüselHintString)]
		public SictAuswertPyObj32Zuusctand DictEntryHint;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstSclüsel_SrString)]
		public SictAuswertPyObj32Zuusctand DictEntry_Sr;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstLabelSclüselTextString)]
		public SictAuswertPyObj32Zuusctand DictEntryText;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstLabelSclüselLinkTextString)]
		public SictAuswertPyObj32Zuusctand DictEntryLinkText;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstLabelSclüselSetTextString)]
		public SictAuswertPyObj32Zuusctand DictEntrySetText;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstWindowSclüselIsModalString)]
		public SictAuswertPyObj32Zuusctand DictEntryIsModal;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.OverviewScrollEntrySclüselIsSelectedString)]
		public SictAuswertPyObj32Zuusctand DictEntryOverviewEntryIsSelected;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.TreeViewEntrySclüselIsSelectedString)]
		public SictAuswertPyObj32Zuusctand DictEntryTreeViewEntryIsSelected;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstWindowSclüselTexturePathString)]
		public SictAuswertPyObj32Zuusctand DictEntryTexturePath;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstWindowSclüselCaptionString)]
		public SictAuswertPyObj32Zuusctand DictEntryCaption;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstWindowSclüselMinimizedString)]
		public SictAuswertPyObj32Zuusctand DictEntryMinimized;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstWindowSclüselIsDialogString)]
		public SictAuswertPyObj32Zuusctand DictEntryIsDialog;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstWindowSclüselWindowIdString)]
		public SictAuswertPyObj32Zuusctand DictEntryWindowId;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstWindowSclüselPinnedString)]
		public SictAuswertPyObj32Zuusctand DictEntryPinned;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstShipUISclüselSpeedString)]
		public SictAuswertPyObj32Zuusctand DictEntrySpeed;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstShipUISclüselCapacitorFülsctandString)]
		public SictAuswertPyObj32Zuusctand DictEntryCapacitorLevel;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstShipUISclüselShieldFülsctandString)]
		public SictAuswertPyObj32Zuusctand DictEntryShieldLevel;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstShipUISclüselArmorFülsctandString)]
		public SictAuswertPyObj32Zuusctand DictEntryArmorLevel;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstShipUISclüselStructureFülsctandString)]
		public SictAuswertPyObj32Zuusctand DictEntryStructureLevel;

		[SictInPyDictEntryKeyAttribut(Optimat.EveOnline.SictProzesAuswertZuusctand.GbsAstWindowSclüselBackgroundListString)]
		public SictAuswertPyObj32Zuusctand DictEntryBackgroundList;

		public SictAuswertPyObj32GbsAstZuusctand(
			Int64 HerkunftAdrese,
			Int64 BeginZait)
			:
			base(HerkunftAdrese, BeginZait)
		{
			DictListeEntryAnzaalScrankeMax = 0x1000;

			AstInfo.PyObjAddress = HerkunftAdrese;
		}

		public void ListeChildPropagiireNaacInfoObjekt()
		{
			AstInfo.ListChild =
				this.ListeChild
				?.Select((ChildObj) => ChildObj?.AstInfo)
				?.Reverse()
				?.ToArray();
		}
	}

}
