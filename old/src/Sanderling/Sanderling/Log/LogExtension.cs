using BotEngine;
using System.IO;
using System.Linq;

namespace Sanderling.Log
{
	static public class LogExtension
	{
		static public void Write(this Stream destination, LogEntry entry)
		{
			var entrySerial = entry?.SerializeToUtf8();

			if (null == entrySerial)
				return;

			var entrySerialAndDelimiter = entrySerial.Concat(new byte[] { 13, 10 }).ToArray();

			destination?.Write(entrySerialAndDelimiter, 0, entrySerialAndDelimiter.Length);
		}
	}
}
