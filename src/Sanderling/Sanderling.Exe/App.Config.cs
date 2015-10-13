using Bib3;
using BotEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sanderling.Exe
{
	partial	class App
	{
		static public string ConfigFilePath =>
			Bib3.FCL.Glob.ZuProcessSelbsctMainModuleDirectoryPfaadBerecne().ScteleSicerEndung(System.IO.Path.DirectorySeparatorChar.ToString()) + "config";

		BotEngine.UI.WriteToOrReadFromFile ConfigFileControl =>
			Window?.Main?.ConfigFileControl;

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
