using System;

namespace Sanderling.MemoryReading.Production
{
	public struct Vektor2DSingle
	{
		public float A;

		public float B;

		public Vektor2DSingle(float a, float b)
		{
			this.A = a;
			this.B = b;
		}

		public Vektor2DSingle(Vektor2DSingle zuKopiirende)
			:
			this(zuKopiirende.A, zuKopiirende.B)
		{
		}

		static public Vektor2DSingle operator -(Vektor2DSingle minuend, Vektor2DSingle subtrahend) =>
			new Vektor2DSingle(minuend.A - subtrahend.A, minuend.B - subtrahend.B);

		static public Vektor2DSingle operator -(Vektor2DSingle subtrahend) =>
			new Vektor2DSingle(0, 0) - subtrahend;

		static public Vektor2DSingle operator +(Vektor2DSingle vektor0, Vektor2DSingle vektor1) =>
			new Vektor2DSingle(vektor0.A + vektor1.A, vektor0.B + vektor1.B);

		static public Vektor2DSingle operator /(Vektor2DSingle dividend, double divisor) =>
			new Vektor2DSingle((float)(dividend.A / divisor), (float)(dividend.B / divisor));

		static public Vektor2DSingle operator *(Vektor2DSingle vektor0, double faktor) =>
			new Vektor2DSingle((float)(vektor0.A * faktor), (float)(vektor0.B * faktor));

		static public Vektor2DSingle operator *(double faktor, Vektor2DSingle vektor0) =>
			new Vektor2DSingle((float)(vektor0.A * faktor), (float)(vektor0.B * faktor));

		static public bool operator ==(Vektor2DSingle vektor0, Vektor2DSingle vektor1) =>
			vektor0.A == vektor1.A && vektor0.B == vektor1.B;

		static public bool operator !=(Vektor2DSingle vektor0, Vektor2DSingle vektor1) =>
			!(vektor0 == vektor1);

		override public bool Equals(object obj)
		{
			if (!(obj is Vektor2DSingle))
				return false;

			var AlsVektor = (Vektor2DSingle)obj;

			if (null == AlsVektor)
				return false;

			return this == AlsVektor;
		}

		override public int GetHashCode() =>
			A.GetHashCode() ^ B.GetHashCode();

		public double BetraagQuadriirt => A * A + B * B;

		public double Betraag => Math.Sqrt(BetraagQuadriirt);

		public Vektor2DSingle Normalisiirt()
		{
			var Betraag = this.Betraag;

			return new Vektor2DSingle((float)(this.A / Betraag), (float)(this.B / Betraag));
		}

		public void Normalisiire()
		{
			var Length = this.Betraag;

			this.A = (float)(this.A / Length);
			this.B = (float)(this.B / Length);
		}

		static public double Skalarprodukt(Vektor2DSingle vektor0, Vektor2DSingle vektor1) =>
			vektor0.A * vektor1.A + vektor0.B * vektor1.B;

		override public string ToString() =>
			"{" + GetType().Name + "}(" + A.ToString() + " | " + B.ToString() + ")";
	}
}
