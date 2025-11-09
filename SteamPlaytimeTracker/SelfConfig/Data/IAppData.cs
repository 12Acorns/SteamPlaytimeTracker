using Config.Net;

namespace SteamPlaytimeTracker.SelfConfig.Data;

public interface IAppData
{
	[Option(DefaultValue = null)]
	public string AppVersion { get; set; }

	public ILocalizationData LocalizationData { get; set; }
	public ISteamInstallData SteamInstallData { get; set; }
	public IDiskCacheData DiskCacheBehaviour { get; set; }
	public ILogData LoggingData { get; set; }
}