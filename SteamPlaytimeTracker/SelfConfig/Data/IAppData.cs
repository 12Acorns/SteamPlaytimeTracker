using Config.Net;

namespace SteamPlaytimeTracker.SelfConfig.Data;

public interface IAppData
{
	public string SteamInstallationFolder { get; set; }
	public long LastCheckedSteamApps { get; set; }
	[Option(DefaultValue = "true")]
	public bool CheckForSteamAppsPeriodically { get; set; }

	public string CacheFolder { get; set; }
	// 128MB
	[Option(DefaultValue = 1024ul * 1024ul * 128ul)]
	public ulong MaximumCacheSizeBytes { get; set; }
	[Option(DefaultValue = true)]
	public bool UseDiskCache { get; set; }
}