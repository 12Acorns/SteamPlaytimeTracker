using SteamPlaytimeTracker.Utility.Cache;

namespace SteamPlaytimeTracker.Extensions;

// https://stackoverflow.com/a/17653980
internal static class CacheExtensions
{
	public static T Get<T>(this ICacheManager cacheManager, string key, Func<T> acquire) => 
		Get(cacheManager, key, ICacheManager.DefaultCacheTime, acquire);
	public static T Get<T>(this ICacheManager cacheManager, string key, int cacheTime, Func<T> acquire)
	{
		if(cacheManager.TryGet(key, out T cached))
		{
			return cached;
		}
		var result = acquire() ?? throw new NullReferenceException("Acquire function returned null");
		cacheManager.Set(key, result, cacheTime);
		return result;
	}
}
