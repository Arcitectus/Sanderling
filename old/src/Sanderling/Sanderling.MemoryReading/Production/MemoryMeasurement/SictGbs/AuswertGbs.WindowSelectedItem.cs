using System;
using System.Linq;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsWindowSelectedItem : SictAuswertGbsWindow
	{
		new static public WindowSelectedItemView BerecneFürWindowAst(
			UINodeInfoInTree windowAst)
		{
			if (null == windowAst)
				return null;

			var WindowAuswert = new SictAuswertGbsWindowSelectedItem(windowAst);

			WindowAuswert.Berecne();

			return WindowAuswert.ErgeebnisScpez;
		}

		public UINodeInfoInTree AstMainContainerMainToparea
		{
			private set;
			get;
		}

		public UINodeInfoInTree AstMainContainerMainTopareaLabelNameUndDistance
		{
			private set;
			get;
		}

		public string SelectedItemTextNameUndDistance
		{
			private set;
			get;
		}

		public string[] SelectedItemTextNameUndDistanceListeZaile
		{
			private set;
			get;
		}

		public string SelectedItemName
		{
			private set;
			get;
		}

		public string SelectedItemDistanceSictString
		{
			private set;
			get;
		}

		public Int64? SelectedItemDistanceScrankeMin
		{
			private set;
			get;
		}

		public Int64? SelectedItemDistanceScrankeMax
		{
			private set;
			get;
		}

		public WindowSelectedItemView ErgeebnisScpez
		{
			private set;
			get;
		}

		public SictAuswertGbsWindowSelectedItem(UINodeInfoInTree windowSelectedItem)
			:
			base(windowSelectedItem)
		{
		}

		override public void Berecne()
		{
			base.Berecne();

			var BaseErgeebnis = base.Ergeebnis;

			if (null == BaseErgeebnis)
			{
				return;
			}

			AstMainContainerMainToparea =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				AstMainContainerMain, (kandidaat) => string.Equals("toparea", kandidaat.Name, StringComparison.InvariantCultureIgnoreCase), 2);

			AstMainContainerMainTopareaLabelNameUndDistance =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				AstMainContainerMainToparea, (kandidaat) => string.Equals("EveLabelSmall", kandidaat.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), 2);

			if (null != AstMainContainerMainTopareaLabelNameUndDistance)
			{
				SelectedItemTextNameUndDistance = AstMainContainerMainTopareaLabelNameUndDistance.SetText;
			}

			var ActionContainer =
				//	2015.09.07:	Name = "actions"
				AstMainContainerMain?.FirstMatchingNodeFromSubtreeBreadthFirst(k => k.PyObjTypNameIsContainer() && k.NameMatchesRegexPatternIgnoreCase("action"));


			var tInspektSpritePfaad =
				AstMainContainerMain
				?.ListPathToNodeFromSubtreeBreadthFirst(k => k.PyObjTypNameIsSprite())
				?.ToArray();

            if (null != SelectedItemTextNameUndDistance)
			{
				/*
				 * 2013.07.17
				 * Bsp: "Ruvas II - Sukuuvestaa Corporation Production Plant<br>Distance: 0 m"
				 * */

				SelectedItemTextNameUndDistanceListeZaile = SelectedItemTextNameUndDistance.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);

				SelectedItemName = SelectedItemTextNameUndDistanceListeZaile.ElementAtOrDefault(0);
				SelectedItemDistanceSictString = SelectedItemTextNameUndDistanceListeZaile.ElementAtOrDefault(1);
			}

			var ErgeebnisScpez = new WindowSelectedItemView(
				BaseErgeebnis);

			this.ErgeebnisScpez = ErgeebnisScpez;
		}
	}
}
