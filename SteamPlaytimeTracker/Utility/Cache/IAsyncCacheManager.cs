using OneOf.Monads;
using System.IO;

namespace SteamPlaytimeTracker.Utility.Cache;

internal interface IAsyncCacheManager
{
	ValueTask<T> GetAsync<T>(string key, Func<FileStream, ValueTask<T>>? converter = null) where T : unmanaged;
	ValueTask<Option<T>> TryGetAsync<T>(string key, Func<FileStream, ValueTask<T>>? converter = null) where T : unmanaged;
	ValueTask SetAsync(string key, ReadOnlySpan<byte> data, TimeSpan cacheTime);
	ValueTask SetAsync(string key, ReadOnlySpan<byte> data, int cacheTimeMinutes = ICacheManager.DefaultCacheTime);
	ValueTask IsSetAsync(string key);
	ValueTask RemoveAsync(string key);
	ValueTask ClearAsync();
}