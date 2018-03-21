using Bib3;
using BotEngine.Common;
using BotSharp.UI.Wpf;
using Sanderling.Log;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sanderling.Exe
{
	partial class App
	{
		string LicenseKeyStoreFilePath => AssemblyDirectoryPath.PathToFilesysChild(@"license.key");

		ISingleValueStore<string> LicenseKeyStore;

		static public string ConfigFilePath =>
			AssemblyDirectoryPath.PathToFilesysChild("config");

		string ScriptDirectoryPath => AssemblyDirectoryPath.PathToFilesysChild(@"script\");

		string DefaultScriptPath => ScriptDirectoryPath.PathToFilesysChild("default.cs");

		static KeyValuePair<string, string>[] ListScriptIncluded =
			SetScriptIncludedConstruct()?.ExceptionCatch(Bib3.FCL.GBS.Extension.MessageBoxException)
			?.OrderBy(scriptNameAndContent => !scriptNameAndContent.Key.RegexMatchSuccessIgnoreCase("travel"))
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

		static BotsNavigationConfiguration BotsNavigationConfiguration()
		{
			string includedScriptFromNameRegexPattern(string regexPattern) =>
				ListScriptIncluded
				.FirstOrDefault(includedScript => Regex.Match(includedScript.Key, regexPattern, RegexOptions.IgnoreCase).Success)
				.Value;

			var botsOfferedAtRoot = new[]
			{
				("Automate Mining Ore From Asteroids", includedScriptFromNameRegexPattern(@"beginners-ore-asteroid-miner")),
				("Automate Travel (Faster Autopilot)", includedScriptFromNameRegexPattern(@"Travel\.simple")),
			}
			.Where(descriptionAndBot => 0 < descriptionAndBot.Item2?.Length)
			.ToList();

			return new BotsNavigationConfiguration
			{
				RootContentFromDefaultBot = defaultBot => UI.BotsNavigation.NavigationRoot(botsOfferedAtRoot, defaultBot),
				OfferedDemoScripts = ListScriptIncluded,
			};
		}

		static public ExeConfig ConfigDefaultConstruct() =>
			new ExeConfig
			{
				LicenseClient = ExeConfig.LicenseClientDefault,
			};

		void ConfigSetup()
		{
			LicenseKeyStore =
				new SingleValueStoreCached<string>
				{
					BaseStore =
						new SingleValueStoreRelayWithExceptionToDelegate<string>
						{
							BaseStore = new StringStoreToFilePath
							{
								FilePath = LicenseKeyStoreFilePath,
							},

							ExceptionDelegate = e => LogEntryWriteNow(new LogEntry { LicenseKeyStoreException = e }),
						}
				};

			UI.Main.LicenseKeyStore = LicenseKeyStore;
		}
	}
}
