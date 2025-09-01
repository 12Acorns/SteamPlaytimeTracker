namespace SteamPlaytimeTracker.Extensions;

internal static class TaskExtensions
{
	public static T Result<T>(this Task<T> task, TimeSpan timeout)
	{
		task.Wait(timeout);
		return task.Result;
	}
	public static T Result<T>(this Task<T> task)
	{
		task.Wait();
		return task.Result;
	}
}
