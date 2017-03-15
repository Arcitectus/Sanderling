using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bib3;
using BotEngine.EveOnline.Sensor;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsInfoPanelGen
	{
		readonly public SictGbsAstInfoSictAuswert InfoPanelAst;

		public SictGbsAstInfoSictAuswert TopContAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert HeaderBtnContAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert HeaderBtnContExpandButtonAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert HeaderContAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert MainContAst
		{
			private set;
			get;
		}

		public InfoPanel Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertGbsInfoPanelGen(SictGbsAstInfoSictAuswert InfoPanelAst)
		{
			this.InfoPanelAst = InfoPanelAst;
		}

		virtual public void Berecne()
		{
			if (null == InfoPanelAst)
			{
				return;
			}

			if (!(true == InfoPanelAst.SictbarMitErbe))
			{
				return;
			}

			TopContAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				InfoPanelAst, (Kandidaat) =>
					Kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("topCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				3, 1);

			HeaderBtnContAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				TopContAst, (Kandidaat) =>
					Kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("headerBtnCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				3, 1);

			HeaderBtnContExpandButtonAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				HeaderBtnContAst, (Kandidaat) =>
					string.Equals("Sprite", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
					3, 1);

			HeaderContAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				TopContAst, (Kandidaat) =>
					Kandidaat.PyObjTypNameIsContainer() &&
					string.Equals("headerCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				3, 1);

			MainContAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				InfoPanelAst, (Kandidaat) =>
					string.Equals("ContainerAutoSize", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase) &&
					string.Equals("mainCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				3, 1);

			var ExpandedContent = MainContAst.AlsContainer();

			var ExpandedListLabelString =
				MainContAst?.ExtraktMengeLabelString()?.OrdnungLabel()?.ToArray();

			bool? MainContSictbar = null;
			IUIElement ExpandToggleButton = null;

			if (null != MainContAst)
			{
				MainContSictbar = MainContAst.SictbarMitErbe;
			}

			if (null != HeaderBtnContExpandButtonAst)
			{
				ExpandToggleButton = HeaderBtnContExpandButtonAst.AlsSprite();
			}

			//	var HeaderLabel = TopContAst.GröösteLabel().AlsUIElementLabelStringFalsLabelString();
			var HeaderLabel = TopContAst?.ExtraktMengeLabelString()?.Grööste();

			var HeaderContent = TopContAst?.AlsContainer();

			var Ergeebnis = new InfoPanel(InfoPanelAst.AlsContainer())
			{
				IsExpanded = MainContSictbar,
				ExpandToggleButton = ExpandToggleButton,
				ExpandedContent = ExpandedContent,
				HeaderContent = HeaderContent,
			};

			this.Ergeebnis = Ergeebnis;
		}
	}

	public class SictAuswertGbsSidePanels
	{
		readonly public SictGbsAstInfoSictAuswert AstSidePanels;

		public SictGbsAstInfoSictAuswert NeocomAst
		{
			private set;
			get;
		}

		SictAuswertNeocom NeocomAuswert;

		public Neocom Neocom => NeocomAuswert?.Ergeebnis;

		public SictGbsAstInfoSictAuswert InfoPanelContainerAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert InfoPanelContainerTopContAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert[] InfoPanelContainerTopContMengeButtonAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert InfoPanelButtonIncursionsAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert InfoPanelButtonLocationInfoAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert InfoPanelButtonRouteAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert InfoPanelButtonMissionAst
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstInfoPanelLocationInfo
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstInfoPanelRoute
		{
			private set;
			get;
		}

		public SictGbsAstInfoSictAuswert AstInfoPanelMissions
		{
			private set;
			get;
		}

		public SictAuswertGbsInfoPanelCurrentSystem AuswertPanelCurrentSystem
		{
			private set;
			get;
		}

		public SictAuswertGbsInfoPanelRoute AuswertPanelRoute
		{
			private set;
			get;
		}

		public SictAuswertGbsInfoPanelMissions AuswertPanelMissions
		{
			private set;
			get;
		}

		public SictAuswertGbsSidePanels(SictGbsAstInfoSictAuswert AstSidePanels)
		{
			this.AstSidePanels = AstSidePanels;
		}

		public void Berecne()
		{
			if (null == AstSidePanels)
			{
				return;
			}

			if (!(true == AstSidePanels.SictbarMitErbe))
			{
				return;
			}

			NeocomAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstSidePanels, (Kandidaat) =>
					string.Equals("Neocom", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase),
					2, 1);

			NeocomAuswert = new SictAuswertNeocom(NeocomAst);
			NeocomAuswert.Berecne();

			InfoPanelContainerAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				AstSidePanels, (Kandidaat) => string.Equals("InfoPanelContainer", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 4);

			InfoPanelContainerTopContAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				InfoPanelContainerAst, (Kandidaat) => string.Equals("topCont", Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 1, 1);

			InfoPanelContainerTopContMengeButtonAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAst(
				InfoPanelContainerTopContAst, (Kandidaat) =>
					true == Kandidaat.SictbarMitErbe &&
					string.Equals("ButtonIconInfoPanel", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 3, 1);

			if (null != InfoPanelContainerTopContMengeButtonAst)
			{
				InfoPanelButtonIncursionsAst =
					InfoPanelContainerTopContMengeButtonAst
					.FirstOrDefault((Kandidaat) =>
						Regex.Match(Kandidaat.Name ?? "", "Incursion", RegexOptions.IgnoreCase).Success);

				InfoPanelButtonLocationInfoAst =
					InfoPanelContainerTopContMengeButtonAst
					.FirstOrDefault((Kandidaat) =>
						Regex.Match(Kandidaat.Name ?? "", "Location", RegexOptions.IgnoreCase).Success);

				InfoPanelButtonRouteAst =
					InfoPanelContainerTopContMengeButtonAst
					.FirstOrDefault((Kandidaat) =>
						Regex.Match(Kandidaat.Name ?? "", "Route", RegexOptions.IgnoreCase).Success);

				InfoPanelButtonMissionAst =
					InfoPanelContainerTopContMengeButtonAst
					.FirstOrDefault((Kandidaat) =>
						Regex.Match(Kandidaat.Name ?? "", "Mission", RegexOptions.IgnoreCase).Success);
			}

			AstInfoPanelLocationInfo =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				InfoPanelContainerAst, (Kandidaat) => string.Equals("InfoPanelLocationInfo", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 4);

			if (null == AstInfoPanelLocationInfo)
			{
				var MengeKandidaatAst =
					Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAst(InfoPanelContainerAst, (Kandidaat) => true, null, 2);

				if (null != MengeKandidaatAst)
				{
					foreach (var KandidaatAst in MengeKandidaatAst)
					{
						var KandidaatAuswertPanelLocationInfo = new SictAuswertGbsInfoPanelCurrentSystem(KandidaatAst);
						KandidaatAuswertPanelLocationInfo.Berecne();

						if (null == KandidaatAuswertPanelLocationInfo?.ErgeebnisScpez?.ListSurroundingsButton)
						{
							continue;
						}

						AstInfoPanelLocationInfo = KandidaatAst;
					}
				}
			}

			AstInfoPanelRoute =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				InfoPanelContainerAst, (Kandidaat) => string.Equals("InfoPanelRoute", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 4);

			AstInfoPanelMissions =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				InfoPanelContainerAst, (Kandidaat) => string.Equals("InfoPanelMissions", Kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 4);

			AuswertPanelCurrentSystem = new SictAuswertGbsInfoPanelCurrentSystem(AstInfoPanelLocationInfo);
			AuswertPanelCurrentSystem.Berecne();

			AuswertPanelRoute = new SictAuswertGbsInfoPanelRoute(AstInfoPanelRoute);
			AuswertPanelRoute.Berecne();

			AuswertPanelMissions = new SictAuswertGbsInfoPanelMissions(AstInfoPanelMissions);
			AuswertPanelMissions.Berecne();
		}
	}
}
