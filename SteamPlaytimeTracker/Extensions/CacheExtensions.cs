using SteamPlaytimeTracker.Utility.Cache;
using System.Runtime.Caching;
using ValueTaskSupplement;

namespace SteamPlaytimeTracker.Extensions;

// https://stackoverflow.com/a/17653980
internal static class CacheExtensions
{
	private static readonly SemaphoreSlim _writeSemaphore = new(1, 1);

	public static T GetOrAdd<T>(this ICacheManager cacheManager, string key, Func<T> acquire, params ChangeMonitor[] monitors) => 
		GetOrAdd(cacheManager, key, ICacheManager.DefaultCacheTime, acquire, monitors);
	public static T GetOrAdd<T>(this ICacheManager cacheManager, string key, int cacheTimeMinutes, Func<T> acquire, params ChangeMonitor[] monitors) =>
		GetOrAdd(cacheManager, key, TimeSpan.FromMinutes(cacheTimeMinutes), acquire, monitors);
	public static T GetOrAdd<T>(this ICacheManager cacheManager, string key, TimeSpan cacheTime, Func<T> acquire, params ChangeMonitor[] monitors)
	{
		if(cacheManager.TryGet(key, out T cached))
		{
			return cached;
		}
		var result = acquire() ?? throw new NullReferenceException("Acquire function returned null");
		cacheManager.Set(key, result, cacheTime, monitors);
		return result;
	}
	public static ValueTask<T> GetAsync<T>(this ICacheManager cacheManager, string key, Func<CancellationToken, Task<T>> acquire, 
		CancellationToken token = default, params ChangeMonitor[] monitors) =>
		GetAsync(cacheManager, key, ICacheManager.DefaultCacheTime, acquire, token, monitors);
	public static ValueTask<T> GetAsync<T>(this ICacheManager cacheManager, string key, int cacheTimeMinutes, Func<CancellationToken, Task<T>> acquire, 
		CancellationToken token = default, params ChangeMonitor[] monitors) => 
		GetAsync(cacheManager, key, TimeSpan.FromMinutes(cacheTimeMinutes), acquire, token, monitors);
	public static async ValueTask<T> GetAsync<T>(this ICacheManager cacheManager, string key, TimeSpan cacheTime, Func<CancellationToken, Task<T>> acquire,
		 CancellationToken token = default, params ChangeMonitor[] monitors)
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
			cacheManager.Set(key, result, cacheTime, monitors);
			return result;
		}
		finally
		{
			_writeSemaphore.Release();
		}
	}
}
