using System;

namespace Sanderling.Log
{
	public class LogEntry
	{
		public DateTimeOffset EntryTime;

		public string Text;

		public StartupLogEntry Startup;

		public ParseCommandsFromArgumentsEntry ParseCommandsFromArguments;

		public ExecuteCommandsFromArgumentsEntry ExecuteCommandsFromArguments;

		public ContinueOrStartBotOperationLogEntry ContinueOrStartBotOperation;
	}

	public class ContinueOrStartBotOperationLogEntry
	{
		public string Trigger;
	}

	public class StartupLogEntry
	{
		public string[] Args;
	}

	public class ParseCommandsFromArgumentsEntry
	{
		public string Arguments;

		public Exception Exception;
	}

	public class ExecuteCommandsFromArgumentsEntry
	{
		public string Arguments;

		public Exception Exception;
	}
}
