using System.IO;

namespace SteamPlaytimeTracker;

// Path related constants
internal static partial class GlobalData
{
	public const string MainTimeSliceCheckLookupName = "MainTimeSliceCheck";
	public const string AppDataStoreLookupName = "AppDataStorePath";
	public const string ConfigPathLookupName = "ConfigPath";
	public const string DbLookupName = "Db";

	public static readonly string MainSliceCheckLocalPath = Path.Combine("logs", "gameprocess_log.txt");
}

internal static partial class GlobalData
{
	public const string MemoryCacheKey = "MemCache";
	public const string HybridCacheKey = "HybridCache";
}