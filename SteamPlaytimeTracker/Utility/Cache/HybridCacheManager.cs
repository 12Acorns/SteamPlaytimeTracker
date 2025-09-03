using Microsoft.Extensions.DependencyInjection;
using SteamPlaytimeTracker.SelfConfig;
using OneOf.Monads;
using System.IO;

namespace SteamPlaytimeTracker.Utility.Cache;

internal sealed class HybridCacheManager : IAsyncCacheManager
{
	private readonly ICacheManager _memoryCache;
	private readonly AppConfig _config;

	public HybridCacheManager(AppConfig config, [FromKeyedServices(GlobalData.MemoryCacheKey)] ICacheManager memoryCache)
	{
		_memoryCache = memoryCache;
		_config = config;
	}

	public ValueTask ClearAsync()
	{
		throw new NotImplementedException();
	}

	public ValueTask<T> GetAsync<T>(string key, Func<FileStream, ValueTask<T>>? converter = null) where T : unmanaged
	{
		throw new NotImplementedException();
	}

	public ValueTask IsSetAsync(string key)
	{
		throw new NotImplementedException();
	}

	public ValueTask RemoveAsync(string key)
	{
		throw new NotImplementedException();
	}

	public ValueTask SetAsync(string key, ReadOnlySpan<byte> data, TimeSpan cacheTime)
	{
		throw new NotImplementedException();
	}

	public ValueTask SetAsync(string key, ReadOnlySpan<byte> data, int cacheTimeMinutes = 2)
	{
		throw new NotImplementedException();
	}

	public ValueTask<Option<T>> TryGetAsync<T>(string key, Func<FileStream, ValueTask<T>>? converter = null) where T : unmanaged
	{
		throw new NotImplementedException();
	}
}