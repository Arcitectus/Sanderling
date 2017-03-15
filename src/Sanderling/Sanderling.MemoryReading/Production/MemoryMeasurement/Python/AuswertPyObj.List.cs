using BotEngine.Interface;
using System;

namespace Optimat.EveOnline
{
	public class SictAuswertPyObj32ListZuusctand : SictAuswertPyObj32VarZuusctand
	{
		public SictAuswertPyObj32ListZuusctand(
			Int64 herkunftAdrese,
			Int64 beginZait)
			:
			base(herkunftAdrese, beginZait)
		{
		}

		public Int64 Ref_ob_item
		{
			private set;
			get;
		}

		/* ob_item contains space for 'allocated' elements.  The number
		 * currently in use is ob_size.
		 * Invariants:
		 *     0 <= ob_size <= allocated
		 *     len(list) == ob_size
		 *     ob_item == NULL implies ob_size == allocated == 0
		 * list.sort() temporarily sets allocated to -1 to detect mutations.
		 *
		 * Items must normally not be NULL, except during construction when
		 * the list is not yet visible outside the function that builds it.
		 */
		public Int32 allocated
		{
			private set;
			get;
		}

		public UInt32[] ListeItemRef
		{
			private set;
			get;
		}

		public int? ListeItemAnzaalScrankeMax;

		override public void Aktualisiire(
			IMemoryReader ausProzesLeeser,
			out	bool geändert,
			Int64 zait,
			int? zuLeeseListeOktetAnzaal = null)
		{
			UInt32[] ListeItemRef = null;

			try
			{
				base.Aktualisiire(
					ausProzesLeeser,
					out	geändert,
					zait,
					zuLeeseListeOktetAnzaal);

				var Ref_ob_item = ObjektBegin.BaiPlus12UInt32;

				this.Ref_ob_item = Ref_ob_item;
				allocated = ObjektBegin.BaiPlus16Int32;

				if (null == ausProzesLeeser)
				{
					return;
				}

				if (0 != Ref_ob_item && 0 <= ob_size)
				{
					var ListeItemAnzaal = ob_size;

					var ListeItemAnzaalScrankeMax = this.ListeItemAnzaalScrankeMax;

					if (ListeItemAnzaalScrankeMax.HasValue)
					{
						ListeItemAnzaal = Math.Min(ListeItemAnzaal, ListeItemAnzaalScrankeMax.Value);
					}

					if (0x1000 < ListeItemAnzaal)
					{
						return;
					}

					var ItemListeOktetAnzaal = 4;

					var ListeItemListeOktet = ausProzesLeeser.ListeOktetLeeseVonAdrese(Ref_ob_item, ListeItemAnzaal * ItemListeOktetAnzaal, false);

					if (null != ListeItemListeOktet)
					{
						var GeleeseListeItemAnzaal = ListeItemListeOktet.Length / ItemListeOktetAnzaal;

						var InternListeItemRef = new UInt32[GeleeseListeItemAnzaal];

						Buffer.BlockCopy(ListeItemListeOktet, 0, InternListeItemRef, 0, GeleeseListeItemAnzaal * ItemListeOktetAnzaal);

						ListeItemRef = InternListeItemRef;
					}
				}
			}
			finally
			{
				this.ListeItemRef = ListeItemRef;
			}
		}
	}

}
