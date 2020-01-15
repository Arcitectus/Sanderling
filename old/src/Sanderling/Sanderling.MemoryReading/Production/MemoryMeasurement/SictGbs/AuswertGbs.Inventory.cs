using System;
using Sanderling.Interface.MemoryStruct;
using BotEngine.Common;

namespace Optimat.EveOnline.AuswertGbs
{
	public class SictAuswertGbsInventory
	{
		readonly public UINodeInfoInTree InventoryAst;

		public UINodeInfoInTree ListAst
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

		public SictAuswertGbsInventory(UINodeInfoInTree InventoryAst)
		{
			this.InventoryAst = InventoryAst;
		}

		public void Berecne()
		{
			ListAst =
				Optimat.EveOnline.AuswertGbs.Extension.FirstMatchingNodeFromSubtreeBreadthFirst(
				InventoryAst, (Kandidaat) => "Scroll".EqualsIgnoreCase(Kandidaat.PyObjTypName),
				1, 1);

			var MengeInvItemAst =
				Optimat.EveOnline.AuswertGbs.Extension.MatchingNodesFromSubtreeBreadthFirst(
				InventoryAst,
				(Kandidaat) => true == Kandidaat.VisibleIncludingInheritance && "InvItem".EqualsIgnoreCase(Kandidaat.PyObjTypName), null, null, null);

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
			ListAuswert.Read();

			var ListAuswertErgeebnis = ListAuswert.Result;

			if (null == ListAuswertErgeebnis)
			{
				return;
			}

			var Ergeebnis = new Inventory(InventoryAst.AsUIElementIfVisible())
			{
				ListView = ListAuswertErgeebnis,
			};

			this.Ergeebnis = Ergeebnis;
		}
	}
}
