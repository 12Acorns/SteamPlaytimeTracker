namespace SteamPlaytimeTracker.SelfConfig.Data;

public interface IAppData
{
	public ILocalizationData LocalizationData { get; set; }
	public ISteamInstallData SteamInstallData { get; set; }
	public IStyleData StyleData { get; set; }
	public IDiskCacheData DiskCacheBehaviour { get; set; }
	public ILogData LoggingData { get; set; }
	public IUpdaterData UpdateBehaviour { get; set; }
}