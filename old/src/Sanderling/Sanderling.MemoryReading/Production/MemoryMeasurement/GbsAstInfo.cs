using System;
using System.Collections.Generic;
using BotEngine.Interface;
using Sanderling.MemoryReading.Production;

namespace Optimat.EveOnline
{
	public class InGbsPfaad
	{
		public Int64? WurzelAstAdrese;

		public Int64[] ListeAstAdrese;

		public InGbsPfaad()
		{
		}

		public InGbsPfaad(
			Int64? wurzelAstAdrese,
			Int64[] listeAstAdrese = null)
		{
			this.WurzelAstAdrese = wurzelAstAdrese;
			this.ListeAstAdrese = listeAstAdrese;
		}
	}

	public class GbsAstInfo
	{
		/// <summary>
		/// Adrese von welcer der Ast aus Ziil Proces geleese wurde.
		/// </summary>
		public Int64? PyObjAddress;

		public string PyObjTypName;

		public bool? VisibleIncludingInheritance;

		public string Name;

		public string Text;

		public string SetText;

		public string LinkText;

		public string Hint;

		public float? LaageInParentA;

		public float? LaageInParentB;

		public float? GrööseA;

		public float? GrööseB;

		public string Caption;

		public bool? Minimized;

		public bool? isModal;

		public bool? isSelected;

		public string WindowID;

		public double? LastStateFloat;

		public double? LastSetCapacitorFloat;

		/// <summary>
		/// 2015.08.26
		/// Beobactung in Type ShipHudSpriteGauge mit sclüsl "_lastValue" und type "float" (verwandt jewails für Shield, Armor, Struct)
		/// </summary>
		public double? LastValueFloat;

		public double? RotationFloat;

		public string SrHtmlstr;

		public string EditTextlineCoreText;

		public int? ColorAMili;

		public int? ColorRMili;

		public int? ColorGMili;

		public int? ColorBMili;

		/// <summary>
		/// 2013.09.13 Pfaad: [(Dict["_texture"] as trinity.Tr2Sprite2dTexture).[8] + 80]
		/// </summary>
		public Int64? TextureIdent0;

		public string texturePath;

		public float? Speed;

		public float? CapacitorLevel;

		public float? ShieldLevel;

		public float? ArmorLevel;

		public float? StructureLevel;

		public GbsAstInfo[] ListChild;

		public GbsAstInfo[] BackgroundList;

		public string[] DictListKeyStringValueNotEmpty;

		public int? SquadronSize;

		public int? SquadronMaxSize;

		public bool? RampActive;

		public ColorORGBVal? Color
		{
			set
			{
				ZuuwaisungNaacKomponente(Color, ref ColorAMili, ref ColorRMili, ref ColorGMili, ref ColorBMili);
			}

			get
			{
				return KomponenteZuColorARGBVal(ColorAMili, ColorRMili, ColorGMili, ColorBMili);
			}
		}

		public Vektor2DSingle? LaageInParent
		{
			set
			{
				ZuuwaisungNaacKomponente(value, ref LaageInParentA, ref LaageInParentB);
			}

			get
			{
				return KomponenteZuVektorSingle(LaageInParentA, LaageInParentB);
			}
		}

		public Vektor2DSingle? Grööse
		{
			set
			{
				ZuuwaisungNaacKomponente(value, ref GrööseA, ref GrööseB);
			}

			get
			{
				return KomponenteZuVektorSingle(GrööseA, GrööseB);
			}
		}

		public GbsAstInfo()
		{
		}

		public GbsAstInfo(
			Int64? inProzesHerkunftAdrese)
		{
			this.PyObjAddress = inProzesHerkunftAdrese;
		}

		virtual public IEnumerable<GbsAstInfo> GetListChild()
		{
			return ListChild;
		}

		static public void ZuuwaisungNaacKomponente(
			Vektor2DSingle? vektor,
			ref float? komponenteA,
			ref float? komponenteB)
		{
			komponenteA = vektor?.A;
			komponenteB = vektor?.B;
		}

		static public Vektor2DSingle? KomponenteZuVektorSingle(
			float? a,
			float? b)
		{
			if (!a.HasValue || !b.HasValue)
				return null;

			return new Vektor2DSingle(a.Value, b.Value);
		}

		static public void ZuuwaisungNaacKomponente(
			ColorORGBVal? farbe,
			ref int? komponenteAMili,
			ref int? komponenteRMili,
			ref int? komponenteGMili,
			ref int? komponenteBMili)
		{
			komponenteAMili = farbe?.OMilli;
			komponenteRMili = farbe?.RMilli;
			komponenteGMili = farbe?.GMilli;
			komponenteBMili = farbe?.BMilli;
		}

		static public ColorORGBVal? KomponenteZuColorARGBVal(
			int? aMilli,
			int? rMilli,
			int? gMilli,
			int? bMilli)
		{
			return new ColorORGBVal(aMilli, rMilli, gMilli, bMilli);
		}

		public IEnumerable<GbsAstInfo> MengeChildAstTransitiiveHüle(
			int? tiifeScrankeMax = null)
		{
			var ListeChild = this.GetListChild();

			if (tiifeScrankeMax <= 0)
				return null;

			if (null == ListeChild)
				return null;

			var listeChildMengeChildAstTransitiiv = new List<GbsAstInfo>();

			foreach (var Child in ListeChild)
			{
				if (null == Child)
					continue;

				listeChildMengeChildAstTransitiiv.Add(Child);

				var ChildMengeChild = Child.MengeChildAstTransitiiveHüle(tiifeScrankeMax - 1);

				if (null != ChildMengeChild)
					listeChildMengeChildAstTransitiiv.AddRange(ChildMengeChild);
			}

			return listeChildMengeChildAstTransitiiv;
		}

		public Int64[] MengeSelbsctUndChildAstHerkunftAdreseTransitiiveHüleBerecne(
			int? tiifeScrankeMax = null)
		{
			var MengeAdrese = new List<Int64>();

			MengeSelbsctUndChildAstHerkunftAdreseTransitiiveHüleFüügeAinNaacListe(MengeAdrese, tiifeScrankeMax);

			return MengeAdrese.ToArray();
		}

		public void MengeSelbsctUndChildAstHerkunftAdreseTransitiiveHüleFüügeAinNaacListe(
			IList<Int64> ziilListe,
			int? tiifeScrankeMax = null)
		{
			if (null == ziilListe)
				return;

			var herkunftAdrese = this.PyObjAddress;

			if (herkunftAdrese.HasValue)
				ziilListe.Add(herkunftAdrese.Value);

			var listeChild = this.GetListChild();

			if (tiifeScrankeMax <= 0)
				return;

			if (null == listeChild)
				return;

			var listeChildMengeChildAstHerkunftAdreseTransitiiv = new List<Int64>();

			foreach (var Child in listeChild)
			{
				if (null == Child)
					continue;

				Child.MengeSelbsctUndChildAstHerkunftAdreseTransitiiveHüleFüügeAinNaacListe(
					ziilListe,
					tiifeScrankeMax - 1);
			}
		}
	}
}
