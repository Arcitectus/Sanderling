using Bib3;
using BotEngine;
using BotEngine.Common;
using System.Linq;
using System.Text;

namespace Sanderling.Exe
{
	partial class App
	{
		static public string ConfigFilePath =>
			Bib3.FCL.Glob.ZuProcessSelbsctMainModuleDirectoryPfaadBerecne().PathToFilesysChild("config");

		BotEngine.UI.WriteToOrReadFromFile ConfigFileControl =>
			Window?.Main?.ConfigFileControl;

		string ScriptDirectoryPath => AssemblyDirectoryPath.PathToFilesysChild(@"script\");

		string DefaultScriptPath => ScriptDirectoryPath.PathToFilesysChild("default.cs");

		string DefaultScript
		{
			get
			{
				try
				{
					return
						Encoding.UTF8.GetString(
							System.Reflection.Assembly.GetCallingAssembly()?.GetManifestResourceStream(
							System.Reflection.Assembly.GetCallingAssembly()?.GetManifestResourceNames()?.FirstOrDefault(k => k.RegexMatchSuccessIgnoreCase(@"default\.cs"))).LeeseGesamt());
				}
				catch
				{
					return null;
				}
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
			new ExeConfig() { LicenseClient = new LicenseClientConfig() { ApiVersionAddress = ConfigApiVersionDefaultAddress } };

	}
}
