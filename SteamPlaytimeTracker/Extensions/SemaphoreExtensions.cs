namespace SteamPlaytimeTracker.Extensions;

internal static class SemaphoreExtensions
{
	private static int GetMaxSemCount(SemaphoreSlim sem)
	{
		var field = typeof(SemaphoreSlim).GetField("m_maxCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
		if(field != null && field.GetValue(sem) is int maxCount)
		{
			return maxCount;
		}
		throw new InvalidOperationException("Unable to retrieve max count of SemaphoreSlim.");
	}

	extension(SemaphoreSlim sem)
	{
		public bool TryRelease(int count)
		{
			if(sem.CurrentCount < GetMaxSemCount(sem))
			{
				sem.Release(count);
				return true;
			}
			return false;
		}
		public bool TryRelease() => sem.TryRelease(1);
	}
}
