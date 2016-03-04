using Bib3;
using BotEngine;
using BotEngine.Client;
using BotEngine.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sanderling.Exe
{
	partial class App
	{
		static public string ConfigFilePath =>
			AssemblyDirectoryPath.PathToFilesysChild("config");

		BotEngine.UI.WriteToOrReadFromFile ConfigFileControl =>
			Window?.Main?.ConfigFileControl;

		string ScriptDirectoryPath => AssemblyDirectoryPath.PathToFilesysChild(@"script\");

		string DefaultScriptPath => ScriptDirectoryPath.PathToFilesysChild("default.cs");

		KeyValuePair<string, string>[] ListScriptIncluded =
			SetScriptIncludedConstruct()?.ExceptionCatch(Bib3.FCL.GBS.Extension.MessageBoxException)
			?.OrderBy(ScriptNameAndContent => !ScriptNameAndContent.Key.RegexMatchSuccessIgnoreCase("travel"))
			?.ToArray();

		static IEnumerable<KeyValuePair<string, string>> SetScriptIncludedConstruct()
		{
			var Assembly = typeof(App).Assembly;

			var SetResourceName = Assembly?.GetManifestResourceNames();

			var ScriptPrefix = Assembly.GetName().Name + ".sample.script.";

			foreach (var ResourceName in SetResourceName.EmptyIfNull())
			{
				var ScriptIdMatch = ResourceName.RegexMatchIfSuccess(Regex.Escape(ScriptPrefix) + @"(.*)");

				if (null == ScriptIdMatch)
					continue;

				var ScriptUTF8 = Assembly.GetManifestResourceStream(ResourceName)?.LeeseGesamt();

				if (null == ScriptUTF8)
					continue;

				yield return new KeyValuePair<string, string>(ScriptIdMatch?.Groups?[1]?.Value, Encoding.UTF8.GetString(ScriptUTF8));
			}
		}

		public ExeConfig ConfigReadFromUI()
		{
			return Window?.Main?.ConfigFromViewToModel();
		}

		public void ConfigWriteToUI(ExeConfig Config)
		{
			Window?.Main?.ConfigFromModelToView(Config);
		}

		public byte[] ConfigReadFromUISerialized() => ConfigReadFromUI().SerializeToUtf8();

		public void ConfigWriteToUIDeSerialized(byte[] Config) => ConfigWriteToUI(Config.DeserializeFromUtf8<ExeConfig>());

		static public ExeConfig ConfigDefaultConstruct() =>
			new ExeConfig
			{
				LicenseClient = new LicenseClientConfig
				{
					ApiVersionAddress = ExeConfig.ConfigApiVersionAddressDefault,
					Request = new AuthRequest
					{
						LicenseKey = ExeConfig.ConfigLicenseKeyFree,
						ServiceId = ExeConfig.ConfigServiceId,
						Consume = true,
					},
				},
			};

	}
}
