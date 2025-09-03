
namespace SteamPlaytimeTracker.Utility.Cache.Disk;

internal class DiskCacheEntry : ICacheEntry
{
	public required int KeyHash { get; set; }
	public TimeSpan ExpirationTime { get; set; }
	public DateTimeOffset LastAccessed { get; set; }

	public override int GetHashCode() => KeyHash;
	public override bool Equals(object? obj)
	{
		if(obj == null)
		{
			return false;
		}
		return obj.GetHashCode() == KeyHash;
	}

	public static bool operator ==(DiskCacheEntry? lhs, DiskCacheEntry? rhs) => lhs?.Equals(rhs) ?? false;
	public static bool operator !=(DiskCacheEntry? lhs, DiskCacheEntry? rhs) => !lhs?.Equals(rhs) ?? false;
}
