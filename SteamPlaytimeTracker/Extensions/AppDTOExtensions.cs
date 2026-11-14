using SteamPlaytimeTracker.DbObject;

namespace SteamPlaytimeTracker.Extensions;

internal static class AppDTOExtensions
{
	extension(AppDTO app)
	{
		public TimeSpan GetTotalPlaytime() => app.Intervals
			.Aggregate(TimeSpan.Zero, (a, b) => a + b.SessionLength);
	}
}
