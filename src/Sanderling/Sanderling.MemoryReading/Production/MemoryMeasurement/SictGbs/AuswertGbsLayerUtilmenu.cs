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
		readonly public SictGbsAstInfoSictAuswert AstLayerUtilmenu;

		public SictGbsAstInfoSictAuswert AstHeader
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstHeaderLabel
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

		public SictGbsAstInfoSictAuswert AstExpandedUtilMenu
		{
			private set;
			get;
		}

		public SictAuswertGbsLayerUtilmenu(SictGbsAstInfoSictAuswert AstLayerUtilmenu)
		{
			this.AstLayerUtilmenu = AstLayerUtilmenu;
		}

		static public bool KandidaatUtilmenuLaagePasendZuExpandedUtilmenu(
			SictGbsAstInfoSictAuswert ExpandedUtilMenuAst,
			SictGbsAstInfoSictAuswert KandidaatUtilmenuAst)
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
			SictGbsAstInfoSictAuswert[]	MengeKandidaatUtilmenuAst)
		{
			if (null == AstLayerUtilmenu)
			{
				return;
			}

			if (!(true == AstLayerUtilmenu.SictbarMitErbe))
			{
				return;
			}

			AstHeader =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstLayerUtilmenu,
				(Kandidaat) => Kandidaat.PyObjTypNameIsContainer(), 2, 1);

			AstExpandedUtilMenu =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
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

			SictGbsAstInfoSictAuswert	UtilmenuGbsAst	= null;

			if (null != MengeKandidaatUtilmenuAst)
			{
				UtilmenuGbsAst = MengeKandidaatUtilmenuAst.FirstOrDefault((Kandidaat) => KandidaatUtilmenuLaagePasendZuExpandedUtilmenu(AstExpandedUtilMenu, Kandidaat));
			}

			AstHeaderLabel =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				UtilmenuGbsAst,
				(Kandidaat) => string.Equals("EveLabelMedium", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2, 1);

			if (null != AstHeaderLabel)
			{
				Header = new UIElementText(AstHeaderLabel.AlsUIElementFalsUnglaicNullUndSictbar(), AstHeaderLabel.LabelText());
			}

			if (null != Header)
			{
				MenuTitel = Header.Text;
			}
		}
	}
}
