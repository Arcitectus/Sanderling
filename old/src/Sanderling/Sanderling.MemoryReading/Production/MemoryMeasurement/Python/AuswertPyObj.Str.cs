using BotEngine.Interface;
using System;
using System.Linq;
using System.Text;

namespace Optimat.EveOnline
{
	public class SictAuswertPyObj32StrZuusctand : SictAuswertPyObj32VarZuusctand
	{
		public SictAuswertPyObj32StrZuusctand(
			Int64 herkunftAdrese,
			Int64 beginZait)
			:
			base(herkunftAdrese, beginZait)
		{
		}

		public const int StringBeginOktetIndex = 20;

		public Int32 ob_shash
		{
			private set;
			get;
		}

		public Int32 ob_sstate
		{
			private set;
			get;
		}

		public string WertString
		{
			private set;
			get;
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

			ob_shash = ObjektBegin.BaiPlus12Int32;
			ob_sstate = ObjektBegin.BaiPlus16Int32;

			var ob_size = this.ob_size;

			ObjektListeOktetAnzaal = StringBeginOktetIndex + ob_size;

			if (this.AusScpaicerLeeseLezteListeOktetUndAnzaal.Value < ObjektListeOktetAnzaal)
			{
				base.Aktualisiire(
					ausProzesLeeser,
					out	geändert,
					zait,
					Math.Max(ObjektListeOktetAnzaal, zuLeeseListeOktetAnzaal ?? 0));
			}

			var VerarbaitetLezteListeOktetUndAnzaal = this.AusScpaicerLeeseLezteListeOktetUndAnzaal;

			var WertStringListeOktet =
				(null == VerarbaitetLezteListeOktetUndAnzaal.Key) ? null :
				VerarbaitetLezteListeOktetUndAnzaal.Key.Skip(StringBeginOktetIndex)
				.Take(VerarbaitetLezteListeOktetUndAnzaal.Value)
				.TakeWhile((zaicenOktet) => 0 != zaicenOktet)
				.ToArray();

			WertString = (null == WertStringListeOktet) ? null : Encoding.ASCII.GetString(WertStringListeOktet);
		}
	}
}
