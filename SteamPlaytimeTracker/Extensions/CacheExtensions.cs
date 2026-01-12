using SteamPlaytimeTracker.Utility.Cache;
using ValueTaskSupplement;

namespace SteamPlaytimeTracker.Extensions;

// https://stackoverflow.com/a/17653980
internal static class CacheExtensions
{
	private static readonly SemaphoreSlim _writeSemaphore = new(1, 1);

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
	public static async ValueTask<T> GetAsync<T>(this ICacheManager cacheManager, string key, Func<CancellationToken, Task<T>> acquire, CancellationToken token = default) =>
		await GetAsync(cacheManager, key, ICacheManager.DefaultCacheTime, acquire, token).ConfigureAwait(false);
	public static async ValueTask<T> GetAsync<T>(this ICacheManager cacheManager, string key, TimeSpan cacheTime, Func<CancellationToken, Task<T>> acquire, 
		CancellationToken token = default) => 
		await GetAsync(cacheManager, key, (int)cacheTime.TotalMinutes, acquire, token).ConfigureAwait(false);
	public static async ValueTask<T> GetAsync<T>(this ICacheManager cacheManager, string key, int cacheTime, Func<CancellationToken, Task<T>> acquire,
		CancellationToken token = default)
	{
		if(cacheManager.TryGet(key, out T cached))
		{
			return cached;
		}
		await _writeSemaphore.WaitAsync(token).ConfigureAwait(false);
		try
		{
			if(cacheManager.TryGet(key, out cached))
			{
				return cached;
			}
			var result = (await acquire(token).ConfigureAwait(false)) ?? throw new NullReferenceException("Acquire function returned null");
			cacheManager.Set(key, result, cacheTime);
			return result;
		}
		finally
		{
			_writeSemaphore.Release();
		}
	}
}
