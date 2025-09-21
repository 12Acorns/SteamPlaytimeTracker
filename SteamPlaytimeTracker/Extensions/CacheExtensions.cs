using SteamPlaytimeTracker.Utility.Cache;

namespace SteamPlaytimeTracker.Extensions;

// https://stackoverflow.com/a/17653980
internal static class CacheExtensions
{
	public static T Get<T>(this ICacheManager cacheManager, string key, Func<T> acquire) => 
		Get(cacheManager, key, ICacheManager.DefaultCacheTime, acquire);
	public static T Get<T>(this ICacheManager cacheManager, string key, TimeSpan cacheTime, Func<T> acquire) =>
		Get(cacheManager, key, (int)cacheTime.TotalMinutes, acquire);
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
	public static async ValueTask<T> GetAsync<T>(this ICacheManager cacheManager, string key, Func<Task<T>> acquire) =>
		await GetAsync(cacheManager, key, ICacheManager.DefaultCacheTime, acquire).ConfigureAwait(false);
	public static async ValueTask<T> GetAsync<T>(this ICacheManager cacheManager, string key, TimeSpan cacheTime, Func<Task<T>> acquire) => 
		await GetAsync(cacheManager, key, (int)cacheTime.TotalMinutes, acquire).ConfigureAwait(false);
	public static async ValueTask<T> GetAsync<T>(this ICacheManager cacheManager, string key, int cacheTime, Func<Task<T>> acquire)
	{
		if(cacheManager.TryGet(key, out T cached))
		{
			return cached;
		}
		var result = (await acquire().ConfigureAwait(false)) ?? throw new NullReferenceException("Acquire function returned null");
		cacheManager.Set(key, result, cacheTime);
		return result;
	}
}
