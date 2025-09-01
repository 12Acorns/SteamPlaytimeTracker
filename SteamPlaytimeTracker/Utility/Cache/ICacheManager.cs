namespace SteamPlaytimeTracker.Utility.Cache;

public interface ICacheManager
{
	public const int DefaultCacheTime = 2; // in minutes

	T Get<T>(string key);
	bool TryGet<T>(string key, out T value);
	void Set(string key, object data, int cacheTime);
	bool IsSet(string key);
	void Remove(string key);
	void Clear();
}
