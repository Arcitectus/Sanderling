using Sanderling.Interface.MemoryStruct;
using BotEngine.Interface;
using System;
using System.Text;
using BotEngine;
using System.Linq;

namespace Sanderling.Interface
{
	public class MemoryMeasurementInitParam
	{
		public int ProcessId;
	}

	public class ToInterfaceRequest
	{
		public MemoryMeasurementInitParam MemoryMeasurementInitTake;

		public bool MemoryMeasurementInitGetLast;

		public bool MemoryMeasurementTake;

		public bool MemoryMeasurementGetLast;
	}

	public class FromInterfaceResponse
	{
		public bool MemoryMeasurementInProgress;

		public FromProcessMeasurement<MemoryMeasurementInitParam> MemoryMeasurementInit;

		public FromProcessMeasurement<IMemoryMeasurement> MemoryMeasurement;

		/// <summary>
		/// Area is Window Client Area.
		/// </summary>
		public FromProcessMeasurement<WindowMeasurement> WindowMeasurement;

		static public Bib3.RefNezDiferenz.SictMengeTypeBehandlungRictliinie SerialisPolicyConstruct() =>
			Bib3.SictRefNezKopii.SctandardRictlinieMitScatescpaicer.Rictliinie;

		static Bib3.RefNezDiferenz.SictMengeTypeBehandlungRictliinie SerialisPolicy = SerialisPolicyConstruct();

		static readonly public Bib3.RefNezDiferenz.SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer SerialisPolicyCache =
			new Bib3.RefNezDiferenz.SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer(SerialisPolicy);

		static readonly public Bib3.RefNezDiferenz.SictTypeBehandlungRictliinieMitTransportIdentScatescpaicer
			UITreeComponentTypeHandlePolicyCache = SerialisPolicyCache;

		static public string SerializeToString<T>(T Snapshot) =>
			Bib3.RefNezDiferenz.Extension.WurzelSerialisiire(Snapshot, SerialisPolicyCache).SerializeToString();

		static public byte[] SerializeToUTF8<T>(T Snapshot)
		{
			return Encoding.UTF8.GetBytes(SerializeToString(Snapshot));
		}

		static public T DeserializeFromString<T>(string Json)
		{
			var ListRoot = Bib3.RefNezDiferenz.Extension.ListeWurzelDeserialisiire(Json.DeserializeFromString<Bib3.RefNezDiferenz.SictZuNezSictDiferenzScritAbbild>(), SerialisPolicyCache);

			return (T)(ListRoot?.FirstOrDefault());
		}

		static public T DeserializeFromUTF8<T>(byte[] Utf8)
		{
			if (null == Utf8)
			{
				return default(T);
			}

			return DeserializeFromString<T>(Encoding.UTF8.GetString(Utf8));
		}
	}
}
