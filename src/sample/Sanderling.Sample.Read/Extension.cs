using Mono.Terminal;
using System;
using System.Linq;

namespace Sanderling.Sample.Read
{
	static public class Extension
	{
		static public int? TryParseInt(this string @string)
		{
			int Int;

			if (!int.TryParse(@string, out Int))
			{
				return null;
			}

			return Int;
		}

		static public string ConsoleEditString(this string @default, string prefix = null)
		{
			var Editor = new LineEditor(null);

			Editor.TabAtStartCompletes = true;

			return Editor.Edit(prefix ?? "", @default);
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

		static public config ConfigReadFromConsole()
		{
			var EveOnlineClientProcessIdDefault = GetEveOnlineClientProcessId();

			Console.WriteLine("id of process assumed to be eve online client: " + (EveOnlineClientProcessIdDefault?.ToString() ?? "null"));

			var EveOnlineClientProcessIdString = (EveOnlineClientProcessIdDefault?.ToString() ?? "")?.ConsoleEditString("enter id of eve online client process >");

			var EveOnlineClientProcessId = EveOnlineClientProcessIdString?.TryParseInt();

			if (!EveOnlineClientProcessId.HasValue)
			{
				Console.WriteLine("expected an integer.");
				return null;
			}

			Console.WriteLine();

			var LicenseServerAddress = ExeConfig.ConfigApiVersionAddressDefault.ConsoleEditString("enter license server address >");

			Console.WriteLine();

			var LicenseKey = ExeConfig.ConfigLicenseKeyDefault.ConsoleEditString("enter license key >");
			var ServiceId = ExeConfig.ConfigServiceId.ConsoleEditString("enter service id >");

			return new config()
			{
				EveOnlineClientProcessId = EveOnlineClientProcessId.Value,
				LicenseServerAddress = LicenseServerAddress,
				LicenseKey = LicenseKey,
				ServiceId = ServiceId,
			};
		}
	}
}
