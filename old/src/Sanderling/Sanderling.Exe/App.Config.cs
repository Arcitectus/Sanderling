using Bib3;
using BotEngine.Common;
using BotSharp.UI.Wpf;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sanderling.Exe
{
	partial class App
	{
		string LicenseKeyStoreFilePath => AssemblyDirectoryPath.PathToFilesysChild(@"license.key");

		static public string ConfigFilePath =>
			AssemblyDirectoryPath.PathToFilesysChild("config");

		string ScriptDirectoryPath => AssemblyDirectoryPath.PathToFilesysChild(@"script\");

		string DefaultScriptPath => ScriptDirectoryPath.PathToFilesysChild("default.cs");

		static KeyValuePair<string, byte[]>[] IncludedDemoBots =
			SetScriptIncludedConstruct()?.ExceptionCatch(Bib3.FCL.GBS.Extension.MessageBoxException)
			?.OrderBy(scriptNameAndContent => !scriptNameAndContent.Key.RegexMatchSuccessIgnoreCase("travel"))
			?.ToArray();

		static IEnumerable<KeyValuePair<string, byte[]>> SetScriptIncludedConstruct()
		{
			var Assembly = typeof(App).Assembly;

			var SetResourceName = Assembly?.GetManifestResourceNames();

			var ScriptPrefix = Assembly.GetName().Name + ".sample.script.";

			foreach (var ResourceName in SetResourceName.EmptyIfNull())
			{
				var ScriptIdMatch = ResourceName.RegexMatchIfSuccess(Regex.Escape(ScriptPrefix) + @"(.*)");

				if (null == ScriptIdMatch)
					continue;

				yield return new KeyValuePair<string, byte[]>(
					ScriptIdMatch?.Groups?[1]?.Value,
					Assembly.GetManifestResourceStream(ResourceName)?.LeeseGesamt());
			}
		}

		static BotsNavigationConfiguration BotsNavigationConfiguration()
		{
			byte[] includedBotFromNameRegexPattern(string regexPattern) =>
				IncludedDemoBots
				.FirstOrDefault(includedScript => Regex.Match(includedScript.Key, regexPattern, RegexOptions.IgnoreCase).Success)
				.Value;

			var botsOfferedAtRoot = new[]
			{
				("Automate Mining Ore From Asteroids", includedBotFromNameRegexPattern(@"beginners-ore-asteroid-miner")),
				("Automate Travel (Faster Autopilot)", includedBotFromNameRegexPattern(@"beginners-autopilot")),
			}
			.Where(descriptionAndBot => 0 < descriptionAndBot.Item2?.Length)
			.ToList();

			return new BotsNavigationConfiguration
			{
				RootContentFromDefaultBot = defaultBot => UI.BotsNavigation.NavigationRoot(botsOfferedAtRoot, defaultBot),
				OfferedDemoBots = IncludedDemoBots,
			};
		}

		static public ExeConfig ConfigDefaultConstruct() =>
			new ExeConfig
			{
				LicenseClient = ExeConfig.LicenseClientDefault,
			};
	}
}
