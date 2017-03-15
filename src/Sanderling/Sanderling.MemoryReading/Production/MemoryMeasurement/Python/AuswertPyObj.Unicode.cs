using BotEngine.Interface;
using System;
using System.Text;

namespace Optimat.EveOnline
{
	public class SictAuswertPyObj32UnicodeZuusctand : SictAuswertPyObj32Zuusctand
	{
		readonly public Int32 LengthScranke = 0x10000;

		public SictAuswertPyObj32UnicodeZuusctand(
			Int64 herkunftAdrese,
			Int64 beginZait)
			:
			base(herkunftAdrese, beginZait)
		{
		}

		public Int32 length
		{
			private set;
			get;
		}

		public Int64 Ref_str
		{
			private set;
			get;
		}

		public Int32 hash
		{
			private set;
			get;
		}

		public Int64 Ref_defenc
		{
			private set;
			get;
		}

		public string WertString
		{
			private set;
			get;
		}

		public bool? LengthScrankeAingehalte
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
			var InternZuLeeseListeOktetAnzaal = ObjektBeginListeOktetAnzaal;

			if (zuLeeseListeOktetAnzaal.HasValue)
			{
				InternZuLeeseListeOktetAnzaal = Math.Max(zuLeeseListeOktetAnzaal.Value, InternZuLeeseListeOktetAnzaal);
			}

			base.Aktualisiire(
				ausProzesLeeser,
				out	geändert,
				zait,
				InternZuLeeseListeOktetAnzaal);

			var ObjektBegin = this.ObjektBegin;

			var length = ObjektBegin.BaiPlus8Int32;
			var Ref_str = ObjektBegin.BaiPlus12UInt32;

			this.length = length;
			this.Ref_str = Ref_str;

			hash = ObjektBegin.BaiPlus16Int32;
			Ref_defenc = ObjektBegin.BaiPlus20UInt32;

			string WertString = null;

			try
			{
				if (null == ausProzesLeeser)
				{
					return;
				}

				var LengthBescrankt = Math.Max(0, Math.Min(LengthScranke, length));

				LengthScrankeAingehalte = LengthBescrankt == length;

				var ListeOktetAnzaal = LengthBescrankt * 2;

				var StringListeOktet = ausProzesLeeser.ListeOktetLeeseVonAdrese(Ref_str, ListeOktetAnzaal, false);

				WertString = (null == StringListeOktet) ? null : Encoding.Unicode.GetString(StringListeOktet);
			}
			finally
			{
				this.WertString = WertString;
			}
		}
	}

}
