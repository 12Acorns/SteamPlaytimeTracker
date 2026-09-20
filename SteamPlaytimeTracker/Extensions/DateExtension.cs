using System.Runtime.CompilerServices;

namespace SteamPlaytimeTracker.Extensions;

internal static class DateExtension
{
	public delegate bool DateComparison(DateTime self, DateTime other);

	extension(DateTime dt)
	{
		public DateOnly DateOnly => DateOnly.FromDateTime(dt);
		public bool CompareTo(DateTime other, DateComparison comp) => comp(dt, other);
		public DateTime FirstDayOfNextMonth() => dt.FirstDayOfMonth().AddMonths(1);
		public DateTime LastDayOfMonth() => dt.FirstDayOfMonth().AddMonths(1).AddDays(-1);
		public DateTime FirstDayOfMonth() => new(dt.Year, dt.Month, 1);
	}
	extension(DateOnly d)
	{
		public DateOnly FirstDayOfNextMonth() => d.FirstDayOfMonth().AddMonths(1);
		public DateOnly LastDayOfMonth() => d.FirstDayOfMonth().AddMonths(1).AddDays(-1);
		public DateOnly FirstDayOfMonth() => new(d.Year, d.Month, 1);
	}
	extension(DateOnly)
	{
		public static DateOnly Now => DateOnly.FromDateTime(DateTime.Now);
	}
}
internal sealed class DateComparisonOptions
{
	public static DateExtension.DateComparison YearComparison { get; } = (self, other) => self.Year == other.Year;
	public static DateExtension.DateComparison MonthComparison { get; } = (self, other) => self.Month == other.Month;
	public static DateExtension.DateComparison DayComparison { get; } = (self, other) => self.Day == other.Day;
	public static DateExtension.DateComparison YearMonthComparison { get; } = YearComparison + MonthComparison;
	public static DateExtension.DateComparison YearMonthDayComparison { get; } = YearComparison + MonthComparison + DayComparison;

}