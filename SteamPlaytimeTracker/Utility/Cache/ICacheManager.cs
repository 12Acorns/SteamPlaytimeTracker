namespace SteamPlaytimeTracker.Utility.Cache;

public interface ICacheManager
{
	public const int DefaultCacheTime = 2; // in minutes

	T Get<T>(string key);
	bool TryGet<T>(string key, out T value);
	void Set(string key, object data, TimeSpan cacheTime);
	void Set(string key, object data, int cacheTimeMinutes = DefaultCacheTime) => Set(key, data, TimeSpan.FromMinutes(cacheTimeMinutes));
	bool IsSet(string key);
	void Remove(string key);
	void Clear();

	public abstract object this[string key]
	{
		get;
		set;
	}
	public object this[string key, TimeSpan cacheTime]
	{
		get => this[key]; 
		set => this[key] = value;
	}
	public object this[string key, int cacheTimeMinutes]
	{
		get => this[key];
		set => this[key, TimeSpan.FromMinutes(cacheTimeMinutes)] = value;
	}
}
