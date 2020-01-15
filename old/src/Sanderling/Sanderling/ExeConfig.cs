namespace Sanderling
{
	public class ExeConfig
	{
		public const string ConfigLicenseKeyDefault = "Sanderling.Free";
		public const string ConfigServiceId = "Sanderling.17-01-18";
		public const string ConfigApiVersionAddressDefault = @"http://service.botengine.de:4074/api";

		public const string EveOnlineProcessMainModuleFileName = "ExeFile.exe";

		public const int StayExposedSessionDurationRemainingMin = 60 * 15;

		public BotEngine.Client.LicenseClientConfig LicenseClient;

		static public BotEngine.Client.AuthRequest InterfaceLicenseClientRequestDefault => new BotEngine.Client.AuthRequest
		{
			LicenseKey = ConfigLicenseKeyDefault,
			ServiceId = ConfigServiceId,
			Consume = true,
		};

		static public BotEngine.Client.LicenseClientConfig LicenseClientDefault => new BotEngine.Client.LicenseClientConfig
		{
			ApiVersionAddress = ConfigApiVersionAddressDefault,
			Request = InterfaceLicenseClientRequestDefault,
		};
	}
}
