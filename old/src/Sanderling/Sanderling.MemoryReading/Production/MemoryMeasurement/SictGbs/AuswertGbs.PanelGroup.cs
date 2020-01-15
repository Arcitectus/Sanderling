using System;
using Sanderling.Interface.MemoryStruct;

namespace Optimat.EveOnline.AuswertGbs
{
	/// <summary>
	/// Wii 2014.00.27 in Layer "l_abovemain" gesictet.
	/// </summary>
	public	class SictAuswertGbsPanelGroup
	{
		readonly public UINodeInfoInTree PanelGroupAst;

		/// <summary>
		/// Naame des Ast welcer Menge der Entry enthalt.
		/// 2014.00.27.10	Beobactung:
		/// für PanelGroup isc Name des Ast "Container"
		/// für PanelEveMenu isc Name des Ast "main"
		/// </summary>
		readonly public string ContainerMengeEntryName;

		public UINodeInfoInTree ContainerMengeEntryAst
		{
			private set;
			get;
		}

		public	PanelGroup	Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertGbsPanelGroup(
			UINodeInfoInTree PanelGroupAst,
			string ContainerMengeEntryName = "Container")
		{
			this.PanelGroupAst = PanelGroupAst;
			this.ContainerMengeEntryName = ContainerMengeEntryName;
		}

		virtual	public	void Berecne()
		{
			if (null == PanelGroupAst)
			{
				return;
			}

			if (!(true == PanelGroupAst.VisibleIncludingInheritance))
			{
				return;
			}

			var ContainerMengeEntryName = this.ContainerMengeEntryName;

			ContainerMengeEntryAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				PanelGroupAst, (Kandidaat) =>
					Kandidaat.PyObjTypNameIsContainer() &&
					string.Equals(ContainerMengeEntryName, Kandidaat.Name, StringComparison.InvariantCultureIgnoreCase),
				2, 1);

			var Ergeebnis = new PanelGroup(PanelGroupAst.AlsContainer());

			this.Ergeebnis = Ergeebnis;
		}
	}
}
