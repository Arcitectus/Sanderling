using Mono.Terminal;
using System;
using System.Linq;

namespace Sanderling.Sample.Read
{
	static public class Extension
	{
		static public int? TryParseInt(this string String)
		{
			int Int;

			if (!int.TryParse(String, out Int))
			{
				return null;
			}

			return Int;
		}

		static public string ConsoleEditString(this string Default, string Prefix = null)
		{
			var Editor = new LineEditor(null);

			Editor.TabAtStartCompletes = true;

			return Editor.Edit(Prefix ?? "", Default);
		}

		static public int? GetEveOnlineClientProcessId() =>
			System.Diagnostics.Process.GetProcesses()
			?.FirstOrDefault(Process =>
			{
				try
				{
					return string.Equals("ExeFile.exe", Process?.MainModule?.ModuleName, StringComparison.InvariantCultureIgnoreCase);
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

			var LicenseKey = ExeConfig.ConfigLicenseKeyFree.ConsoleEditString("enter license key >");
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
