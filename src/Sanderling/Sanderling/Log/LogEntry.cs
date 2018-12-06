using System;

namespace Sanderling.Log
{
	public class LogEntry
	{
		public DateTimeOffset EntryTime;

		public string Text;

		public StartupLogEntry Startup;

		public ExecuteCommandsFromArgumentsEntry ExecuteCommandsFromArguments;
	}

	public class StartupLogEntry
	{
		public string[] Args;
	}

	public class ExecuteCommandsFromArgumentsEntry
	{
		public Exception Exception;
	}
}
