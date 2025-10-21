global using static SteamPlaytimeTracker.Utility.VisualTreeUtility;
using ScottPlot;
using System.IO;

namespace SteamPlaytimeTracker;

// Path related constants
internal static partial class GlobalData
{
	public const string MainTimeSliceCheckLookupName = "MainTimeSliceCheck";
	public const string AppDataStoreLookupName = "AppDataStorePath";
	public const string ConfigPathLookupName = "ConfigPath";
	public const string DbLookupName = "Db";
	public const string TmpFolderName = "SteamPlaytimeTracker";

	public static readonly string MainSliceCheckLocalPath = Path.Combine("logs", "gameprocess_log.txt");
}

internal static partial class GlobalData
{
	public const string MemoryCacheKey = "MemCache";
	public const string HybridCacheKey = "HybridCache";
}

internal static partial class GlobalData
{
	private static readonly Color[] _yearPlotColours = [
		Colors.Red,
		Colors.Orange,
		Colors.LightBlue,
		Colors.Aquamarine,
		Colors.DarkBlue,
		Colors.Yellow,
		Colors.Green,
		Colors.Purple,
		Colors.Pink
	];
	private static readonly Color[] _monthPlotColours = [
		Colors.Pink,
		Colors.Purple,
		Colors.BlueViolet,
		Colors.Blue,
		Colors.LightBlue,
		Colors.Green,
		Colors.YellowGreen,
		Colors.Yellow,
		Colors.Orange,
		Colors.DarkOrange,
		Colors.OrangeRed,
		Colors.Red
	];

	public static Color GetYearPlotColour(int year) => _yearPlotColours[year % _yearPlotColours.Length];
	public static Color GetMonthPlotColour(int month) => _monthPlotColours[month % _monthPlotColours.Length];
	public static Color GetDayPlotColour(DateTime dt) => dt.DayOfWeek switch
	{
		DayOfWeek.Monday => Colors.Yellow,
		DayOfWeek.Tuesday => Colors.Pink,
		DayOfWeek.Wednesday => Colors.Green,
		DayOfWeek.Thursday => Colors.Orange,
		DayOfWeek.Friday => Colors.LightBlue,
		DayOfWeek.Saturday => Colors.Purple,
		DayOfWeek.Sunday => Colors.Red,
		var dow => throw new NotImplementedException($"Inputted date produced an invalid day of the week. D/M/Y -> {dt.Day}/{dt.Month}/{dt.Year}. DOW: {dow}")
	};
	public static Color GetDayPlotColour(int day, int month, int year) => GetDayPlotColour(new DateTime(year, month, day));
}