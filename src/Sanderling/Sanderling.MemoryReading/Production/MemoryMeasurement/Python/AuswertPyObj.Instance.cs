using BotEngine.Interface;
using System;

namespace Optimat.EveOnline
{
	public class SictAuswertPyObj32InstanceZuusctand : SictAuswertPyObj32Zuusctand
	{
		public SictAuswertPyObj32InstanceZuusctand(
			Int64 herkunftAdrese,
			Int64 beginZait)
			:
			base(herkunftAdrese, beginZait)
		{
		}

		override public void Aktualisiire(
			IMemoryReader ausProzesLeeser,
			out	bool geändert,
			Int64 zait,
			int? zuLeeseListeOktetAnzaal = null)
		{
			base.Aktualisiire(
				ausProzesLeeser,
				out	geändert,
				zait,
				zuLeeseListeOktetAnzaal);

			if (this.AusScpaicerLeeseLezteListeOktetUndAnzaal.Value < ObjektListeOktetAnzaal)
			{
				base.Aktualisiire(
					ausProzesLeeser,
					out	geändert,
					zait,
					Math.Max(ObjektListeOktetAnzaal, zuLeeseListeOktetAnzaal ?? 0));
			}
		}
	}
}
