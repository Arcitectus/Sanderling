using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BotEngine.EveOnline.Sensor;
using BotEngine.EveOnline.Sensor.Option;
using Sanderling.Interface.MemoryStruct;
using Sanderling.MemoryReading.Production;

namespace Optimat.EveOnline.AuswertGbs
{
	public	class SictAuswertGbsLayerUtilmenu
	{
		readonly public UINodeInfoInTree AstLayerUtilmenu;

		public UINodeInfoInTree AstHeader
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstHeaderLabel
		{
			private set;
			get;
		}

		public IUIElementText Header
		{
			private set;
			get;
		}

		public string MenuTitel
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstExpandedUtilMenu
		{
			private set;
			get;
		}

		public SictAuswertGbsLayerUtilmenu(UINodeInfoInTree AstLayerUtilmenu)
		{
			this.AstLayerUtilmenu = AstLayerUtilmenu;
		}

		static public bool KandidaatUtilmenuLaagePasendZuExpandedUtilmenu(
			UINodeInfoInTree ExpandedUtilMenuAst,
			UINodeInfoInTree KandidaatUtilmenuAst)
		{
			if (null == ExpandedUtilMenuAst || null == KandidaatUtilmenuAst)
			{
				return false;
			}

			var ExpandedUtilMenuAstLaagePlusVonParentErbeLaage = ExpandedUtilMenuAst.LaagePlusVonParentErbeLaage();

			if (!ExpandedUtilMenuAstLaagePlusVonParentErbeLaage.HasValue)
			{
				return	false;
			}

			var KandidaatUtilmenuLaagePlusVonParentErbeLaageNulbar = KandidaatUtilmenuAst.LaagePlusVonParentErbeLaage();
			var KandidaatUtilmenuGrööseNulbar = KandidaatUtilmenuAst.Grööse;

			if (!KandidaatUtilmenuGrööseNulbar.HasValue)
			{
				return false;
			}

			if (!KandidaatUtilmenuLaagePlusVonParentErbeLaageNulbar.HasValue)
			{
				return false;
			}

			//	Linke obere Eke des ExpandedUtilmenu mus in der Nähe von recte untere Eke des Utilmenu liige damit zusamehang vermuutet werd.

			var KandidaatUtilmenuEkeLinksUnteLaage =
				KandidaatUtilmenuLaagePlusVonParentErbeLaageNulbar.Value +
				new Vektor2DSingle(0, KandidaatUtilmenuGrööseNulbar.Value.B);

			var VonUtilmenuNaacExpandedUtilmenuSctreke =
				ExpandedUtilMenuAstLaagePlusVonParentErbeLaage.Value +
				new Vektor2DSingle(0, 1) -
				KandidaatUtilmenuEkeLinksUnteLaage;

			if (4 < VonUtilmenuNaacExpandedUtilmenuSctreke.Betraag)
			{
				return false;
			}

			return true;
		}

		virtual	public	void Berecne(
			UINodeInfoInTree[]	MengeKandidaatUtilmenuAst)
		{
			if (null == AstLayerUtilmenu)
			{
				return;
			}

			if (!(true == AstLayerUtilmenu.VisibleIncludingInheritance))
			{
				return;
			}

			AstHeader =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				AstLayerUtilmenu,
				(Kandidaat) => Kandidaat.PyObjTypNameIsContainer(), 2, 1);

			AstExpandedUtilMenu =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				AstLayerUtilmenu,
				(Kandidaat) => string.Equals("ExpandedUtilMenu", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			if (null == AstExpandedUtilMenu)
			{
				return;
			}

			var AstExpandedUtilMenuLaagePlusVonParentErbeLaage = AstExpandedUtilMenu.LaagePlusVonParentErbeLaage();

			if (!AstExpandedUtilMenuLaagePlusVonParentErbeLaage.HasValue)
			{
				return;
			}

			UINodeInfoInTree	UtilmenuGbsAst	= null;

			if (null != MengeKandidaatUtilmenuAst)
			{
				UtilmenuGbsAst = MengeKandidaatUtilmenuAst.FirstOrDefault((Kandidaat) => KandidaatUtilmenuLaagePasendZuExpandedUtilmenu(AstExpandedUtilMenu, Kandidaat));
			}

			AstHeaderLabel =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				UtilmenuGbsAst,
				(Kandidaat) => string.Equals("EveLabelMedium", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			if (null != AstHeaderLabel)
			{
				Header = new UIElementText(AstHeaderLabel.AsUIElementIfVisible(), AstHeaderLabel.LabelText());
			}

			if (null != Header)
			{
				MenuTitel = Header.Text;
			}
		}
	}
}
