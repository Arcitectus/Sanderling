namespace Sanderling
{
	static public class ConfigExtension
	{
		static public BotEngine.Client.LicenseClientConfig CompletedWithDefault(
			this BotEngine.Client.LicenseClientConfig config)
		{
			config = config ?? ExeConfig.LicenseClientDefault;

			if (!(0 < config.ApiVersionAddress?.Length))
				config.ApiVersionAddress = ExeConfig.ConfigApiVersionAddressDefault;

			config.Request = config?.Request ?? ExeConfig.InterfaceLicenseClientRequestDefault;

			//	force use default ServiceId to prevent problems with old config file when user exchanges executable.
			config.Request.ServiceId = ExeConfig.ConfigServiceId;

			if (!(0 < config.Request.LicenseKey?.Length))
				config.Request.LicenseKey = ExeConfig.ConfigLicenseKeyFree;

			config.Request.Consume = true;

			return config;
		}
	}
}
