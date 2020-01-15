using System;
using System.Linq;
using Sanderling.Interface.MemoryStruct;
using Bib3;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsShipUiSlots
	{
		readonly public UINodeInfoInTree shipUiSlotsNode;

		public UINodeInfoInTree[] MengeKandidaatSlotFenster
		{
			private set;
			get;
		}

		public SictAuswertGbsShipUiSlotsSlot[] MengeKandidaatSlotAuswert
		{
			private set;
			get;
		}

		public ShipUiModule[] ListModuleButton
		{
			private set;
			get;
		}

		public SictAuswertGbsShipUiSlots(UINodeInfoInTree shipUiSlotsNode)
		{
			this.shipUiSlotsNode = shipUiSlotsNode;
		}

		public void Berecne()
		{
			if (!(shipUiSlotsNode?.VisibleIncludingInheritance ?? false))
				return;

			MengeKandidaatSlotFenster =
				shipUiSlotsNode?.MatchingNodesFromSubtreeBreadthFirst(node => string.Equals("ShipSlot", node.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 2, 1, true);

			MengeKandidaatSlotAuswert =
				MengeKandidaatSlotFenster
				?.Select((kandidaatSlotFenster) =>
					{
						var Auswert = new SictAuswertGbsShipUiSlotsSlot(kandidaatSlotFenster);

						Auswert.Berecne();

						return Auswert;
					}).ToArray();

			ListModuleButton =
				MengeKandidaatSlotAuswert
				?.Select(slotAuswert => slotAuswert.ModuleRepr)
				.WhereNotDefault()
				.OrderBy(slot => slot?.RegionCenter()?.A)
				.ToArray();
		}
	}
}
