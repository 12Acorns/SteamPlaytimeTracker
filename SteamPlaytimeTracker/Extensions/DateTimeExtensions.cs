using System.Runtime.CompilerServices;

namespace SteamPlaytimeTracker.Extensions;

internal static class DateTimeExtensions
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DateTime FirstDayOfNextMonth(this DateTime date) =>
		new DateTime(date.Year, date.Month, 1).AddMonths(1);
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DateTime LastDayOfMonth(this DateTime date) =>
		new DateTime(date.Year, date.Month, 1).AddMonths(1).AddDays(-1);
}
