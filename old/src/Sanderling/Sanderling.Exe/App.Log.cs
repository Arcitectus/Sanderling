using Sanderling.Log;
using System;
using System.IO;

namespace Sanderling.Exe
{
	partial class App
	{
		Stream logStream;

		Exception createLogException;

		Exception writeLogEntryException;

		void WriteLogEntry(LogEntry entry)
		{
			try
			{
				lock (logStream)
				{
					logStream.Write(entry);
					logStream.Flush();
				}
			}
			catch (Exception e)
			{
				writeLogEntryException = e;
			}
		}

		void WriteLogEntryWithTimeNow(LogEntry entry)
		{
			entry.EntryTime = DateTime.Now;

			WriteLogEntry(entry);
		}

		void CreateLogFile()
		{
			try
			{
				var logFileName = DateTimeOffset.Now.ToString("yyyy-MM-ddThh-mm-ss") + ".Sanderling.log.jsonl";

				var logFilePath =
					Path.Combine(Bib3.FCL.Glob.ZuProcessSelbsctMainModuleDirectoryPfaadBerecne(), "log", logFileName);

				var directory = new FileInfo(logFilePath).Directory;

				if (!directory.Exists)
					directory.Create();

				logStream = new FileStream(logFilePath, FileMode.CreateNew, FileAccess.Write);

				WriteLogEntryWithTimeNow(new LogEntry { Text = "Sanderling App Started." });
			}
			catch (Exception e)
			{
				createLogException = e;
			}
		}
	}
}
