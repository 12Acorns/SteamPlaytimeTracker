namespace SteamPlaytimeTracker.Extensions;

internal static class DateTimeExtensions
{
	public static DateTime LastDayOfMonth(this DateTime date) =>
		new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
}
