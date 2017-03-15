using System;
using Sanderling.Interface.MemoryStruct;
using BotEngine.Common;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsInventory
	{
		readonly public SictGbsAstInfoSictAuswert InventoryAst;

		public SictGbsAstInfoSictAuswert ListAst
		{
			private set;
			get;
		}

		public SictAuswertGbsListViewport<IListEntry> ListAuswert
		{
			private set;
			get;
		}

		public bool? SictwaiseScaintGeseztAufListNict
		{
			private set;
			get;
		}

		public Inventory Ergeebnis
		{
			private set;
			get;
		}

		public SictAuswertGbsInventory(SictGbsAstInfoSictAuswert InventoryAst)
		{
			this.InventoryAst = InventoryAst;
		}

		public void Berecne()
		{
			ListAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAstFrüheste(
				InventoryAst, (Kandidaat) => "Scroll".EqualsIgnoreCase(Kandidaat.PyObjTypName),
				1, 1);

			var MengeInvItemAst =
				Optimat.EveOnline.AuswertGbs.Extension.SuuceFlacMengeAst(
				InventoryAst,
				(Kandidaat) => true == Kandidaat.SictbarMitErbe && "InvItem".EqualsIgnoreCase(Kandidaat.PyObjTypName), null, null, null);

			if (null != MengeInvItemAst)
			{
				foreach (var InvItemAst in MengeInvItemAst)
				{
					if (null == InvItemAst)
					{
						continue;
					}

					var InvItemAstGrööse = InvItemAst.Grööse;

					if (!InvItemAstGrööse.HasValue)
					{
						continue;
					}

					if (44 < InvItemAstGrööse.Value.B)
					{
						SictwaiseScaintGeseztAufListNict = true;
						break;
					}
				}
			}

			if (null == ListAst)
			{
				return;
			}

			ListAuswert = new SictAuswertGbsListViewport<IListEntry>(ListAst, SictAuswertGbsListViewport<IListEntry>.ListEntryKonstruktSctandard);
			ListAuswert.Berecne();

			var ListAuswertErgeebnis = ListAuswert.Ergeebnis;

			if (null == ListAuswertErgeebnis)
			{
				return;
			}

			var Ergeebnis = new Inventory(InventoryAst.AlsUIElementFalsUnglaicNullUndSictbar())
			{
				ListView = ListAuswertErgeebnis,
			};

			this.Ergeebnis = Ergeebnis;
		}
	}
}
