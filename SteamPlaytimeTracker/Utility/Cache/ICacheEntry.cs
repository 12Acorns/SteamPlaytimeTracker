namespace SteamPlaytimeTracker.Utility.Cache;

public interface ICacheEntry
{
	public int KeyHash { get; set; }
	public TimeSpan ExpirationTime { get; set; }
	public DateTimeOffset LastAccessed { get; set; }
}