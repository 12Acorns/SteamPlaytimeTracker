using SteamPlaytimeTracker.Utility.Cache;

namespace Tests;

public class CacheManagerTest
{
	private const string DefaultValueKey = "key";
	private const string BadKey = "BADKEY";

	[Fact]
	public void TestTryGetValue_AssertTrue_And_AssertValueExistsAndEqual()
	{
		int value = 5;
		var cache = new CacheManager();
		cache.Set(DefaultValueKey, value);

		Assert.True(cache.TryGet(DefaultValueKey, out int cachedValue));
		Assert.Equal(cachedValue, value);
	}
	[Fact]
	public void TestTryGetValue_AssertFalse_And_AssertValueNull()
	{
		int? value = 5;
		var cache = new CacheManager();
		cache.Set(DefaultValueKey, value);

		Assert.False(cache.TryGet(BadKey, out int? cachedValue));
		Assert.Null(cachedValue);
	}
}
