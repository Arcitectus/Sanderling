using BotEngine.Interface;
using System;
using System.Linq;

namespace Optimat.EveOnline
{
	public class SictAuswertPyObj32LongZuusctand : SictAuswertPyObj32VarZuusctand
	{
		public SictAuswertPyObj32LongZuusctand(
			Int64 herkunftAdrese,
			Int64 beginZait)
			:
			base(herkunftAdrese, beginZait)
		{
		}

		public const int IntBeginOktetIndex = 12;

		public byte[] WertSictListeOktet
		{
			private set;
			get;
		}

		public Int64 WertSictIntModulo64Abbild
		{
			private set;
			get;
		}

		override public void Aktualisiire(
			IMemoryReader ausProzesLeeser,
			out bool geändert,
			Int64 zait,
			int? zuLeeseListeOktetAnzaal = null)
		{
			base.Aktualisiire(
				ausProzesLeeser,
				out geändert,
				zait,
				zuLeeseListeOktetAnzaal);

			var ob_size = this.ob_size;

			/*
			2015.07.27

			ObjektListeOktetAnzaal = IntBeginOktetIndex + 2 * ob_size;
			*/
			ObjektListeOktetAnzaal = Math.Min(0x100, IntBeginOktetIndex + 2 * ob_size);

			if (this.AusScpaicerLeeseLezteListeOktetUndAnzaal.Value < ObjektListeOktetAnzaal)
			{
				base.Aktualisiire(
					ausProzesLeeser,
					out geändert,
					zait,
					Math.Max(ObjektListeOktetAnzaal, zuLeeseListeOktetAnzaal ?? 0));
			}

			var VerarbaitetLezteListeOktetUndAnzaal = this.AusScpaicerLeeseLezteListeOktetUndAnzaal;

			var WertSictListeOktet =
				(null == VerarbaitetLezteListeOktetUndAnzaal.Key) ? null :
				VerarbaitetLezteListeOktetUndAnzaal.Key.Skip(IntBeginOktetIndex)
				.Take(VerarbaitetLezteListeOktetUndAnzaal.Value)
				.ToArray();

			this.WertSictIntModulo64Abbild = Optimat.EveOnline.SictAuswertPythonObjLong.WertSictIntModulo64(WertSictListeOktet);
		}
	}
}
