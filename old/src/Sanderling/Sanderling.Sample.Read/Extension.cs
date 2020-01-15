using Mono.Terminal;
using System;
using System.Linq;

namespace Sanderling.Sample.Read
{
	static public class Extension
	{
		static public int? TryParseInt(this string @string)
		{
			int.TryParse(@string, out var @int);

			return @int;
		}

		static public string ConsoleEditString(this string @default, string prefix = null)
		{
			var editor = new LineEditor(null);

			editor.TabAtStartCompletes = true;

			return editor.Edit(prefix ?? "", @default);
		}

		static public int? GetEveOnlineClientProcessId() =>
			System.Diagnostics.Process.GetProcesses()
			?.FirstOrDefault(process =>
			{
				try
				{
					return string.Equals("ExeFile.exe", process?.MainModule?.ModuleName, StringComparison.InvariantCultureIgnoreCase);
				}
				catch { }

				return false;
			})
			?.Id;

		static public int? ReadEveOnlineClientProcessIdFromConsole()
		{
			var eveOnlineClientProcessIdDefault = GetEveOnlineClientProcessId();

			Console.WriteLine("id of process assumed to be eve online client: " + (eveOnlineClientProcessIdDefault?.ToString() ?? "no eve online client process found"));

			var eveOnlineClientProcessIdString = (eveOnlineClientProcessIdDefault?.ToString() ?? "")?.ConsoleEditString("enter id of eve online client process >");

			var eveOnlineClientProcessId = eveOnlineClientProcessIdString?.TryParseInt();

			if (!eveOnlineClientProcessId.HasValue)
			{
				Console.WriteLine("expected an integer.");
				return null;
			}

			return eveOnlineClientProcessId;
		}
	}
}
