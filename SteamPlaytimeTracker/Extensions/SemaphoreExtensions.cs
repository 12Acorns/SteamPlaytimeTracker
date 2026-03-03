using System.Linq.Expressions;
using System.Reflection;

namespace SteamPlaytimeTracker.Extensions;

internal static class SemaphoreExtensions
{
	private static readonly Func<SemaphoreSlim, int> _getMaxCount = CreateGetMaxCountDelegate();

	extension(SemaphoreSlim sem)
	{
		public bool TryRelease(int count)
		{
			if(sem.CurrentCount < _getMaxCount(sem))
			{
				sem.Release(count);
				return true;
			}
			return false;
		}
		public bool TryRelease() => sem.TryRelease(1);
	}

	private static Func<SemaphoreSlim, int> CreateGetMaxCountDelegate()
	{
		var param = Expression.Parameter(typeof(SemaphoreSlim), "semSlim");
		var field = Expression.Field(param, typeof(SemaphoreSlim).GetField("m_maxCount", BindingFlags.NonPublic | BindingFlags.Instance)!);
		return Expression.Lambda<Func<SemaphoreSlim, int>>(field, name: "GetSemMaxCount", parameters: [param]).Compile();
	}
}
