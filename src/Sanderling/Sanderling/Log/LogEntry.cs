using System;

namespace Sanderling.Log
{
	public class LogEntry
	{
		public DateTime EntryTime;

		public Exception LicenseKeyStoreException;

		public BotEngine.Interface.SimpleInterfaceServerDispatcher.ExchangeReport InterfaceServerDispatcherExchange;
	}
}
