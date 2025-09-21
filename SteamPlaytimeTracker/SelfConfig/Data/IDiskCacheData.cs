using Config.Net;

namespace SteamPlaytimeTracker.SelfConfig.Data;

public interface IDiskCacheData
{
	[Option(DefaultValue = null)]
	public string? CacheFolder { get; set; }
	// 128MB
	[Option(DefaultValue = 1024ul * 1024ul * 128ul)]
	public ulong MaximumCacheSizeBytes { get; set; }
	[Option(DefaultValue = false)]
	public bool UseDiskCache { get; set; }
}