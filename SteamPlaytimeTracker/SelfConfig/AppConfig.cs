using SteamPlaytimeTracker.SelfConfig.Data;

namespace SteamPlaytimeTracker.SelfConfig;

public sealed class AppConfig
{
	public AppConfig(IAppData appDataConfig)
	{
		AppData = appDataConfig;
	}

	public IAppData AppData { get; }
}