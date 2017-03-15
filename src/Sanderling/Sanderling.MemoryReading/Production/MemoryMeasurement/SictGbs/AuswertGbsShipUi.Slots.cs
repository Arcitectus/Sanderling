using System;
using System.Linq;
using Sanderling.Interface.MemoryStruct;
using Bib3;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsShipUiSlots
	{
		readonly public SictGbsAstInfoSictAuswert shipUiSlotsNode;

		public SictGbsAstInfoSictAuswert[] MengeKandidaatSlotFenster
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

		public SictAuswertGbsShipUiSlots(SictGbsAstInfoSictAuswert shipUiSlotsNode)
		{
			this.shipUiSlotsNode = shipUiSlotsNode;
		}

		public void Berecne()
		{
			if (!(shipUiSlotsNode?.SictbarMitErbe ?? false))
				return;

			MengeKandidaatSlotFenster =
				shipUiSlotsNode?.SuuceFlacMengeAst(node => string.Equals("ShipSlot", node.PyObjTypName, StringComparison.InvariantCultureIgnoreCase), null, 2, 1, true);

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
