using SteamPlaytimeTracker.SelfConfig.Data;
using SteamPlaytimeTracker.Utility.Cache;

namespace SteamPlaytimeTracker.SelfConfig;

public sealed class AppConfig
{
	public AppConfig(IAppData appDataConfig)
	{
		AppData = appDataConfig;
	}

	public IAppData AppData { get; }
	public IList<ICacheEntry> CacheEntries { get; } = [];
}