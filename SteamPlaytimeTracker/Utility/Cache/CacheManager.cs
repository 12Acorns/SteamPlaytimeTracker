using System.Diagnostics.CodeAnalysis;
using System.Runtime.Caching;

namespace SteamPlaytimeTracker.Utility.Cache;

public sealed class CacheManager : ICacheManager
{
	private readonly ObjectCache _cache = MemoryCache.Default;

	public T Get<T>(string key) => (T)_cache[key];
	public bool TryGet<T>(string key, [NotNullWhen(true)] out T value)
	{
		var val = _cache[key];
		if(val != null)
		{
			value = (T)val;
			return true;
		}
		value = default!;
		return false;
	}
	public void Set(string key, object data, int cacheTime = ICacheManager.DefaultCacheTime)
	{
		if(data == null)
		{
			return;
		}

		CacheItemPolicy policy = new()
		{
			AbsoluteExpiration = DateTime.Now + TimeSpan.FromMinutes(cacheTime)
		};

		_cache.Add(new CacheItem(key, data), policy);
	}

	public bool IsSet(string key) => _cache.Contains(key);
	public void Remove(string key) => _cache.Remove(key);

	public void Clear()
	{
		foreach(var item in _cache)
		{
			Remove(item.Key);
		}
	}
}