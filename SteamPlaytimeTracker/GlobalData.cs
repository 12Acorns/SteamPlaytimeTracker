global using static SteamPlaytimeTracker.Utility.VisualTreeUtility;
using OpenTK.Graphics.ES20;
using ScottPlot;
using SteamPlaytimeTracker.Localization;
using SteamPlaytimeTracker.Localization.Data;
using System.IO;

namespace SteamPlaytimeTracker;

// Path related constants
internal static partial class GlobalData
{
	public const string MainTimeSliceCheckLookupName = "MainTimeSliceCheck";
	public const string AppDataStoreLookupName = "AppDataStorePath";
	public const string LocalizationLookupName = "LocaleEntries";
	public const string ConfigPathLookupName = "ConfigPath";
	public const string DbLookupName = "Db";
	public const string TmpFolderName = "SteamPlaytimeTracker";
	public const string LocalesFolderName = "locales";
	public const string LocaleMapFileName = "locales.json";

	public static readonly string MainSliceCheckLocalPath = Path.Combine("logs", "gameprocess_log.txt");
	public static readonly LocaleData NoLocaleFound = new()
	{
		Code = "no-locale",
		DisplayName = "No Locale Found",
		LocaleTextMap = []
	};
}

internal static partial class GlobalData
{
	public const string MemoryCacheKey = "MemCache";
	public const string HybridCacheKey = "HybridCache";
}

internal static partial class GlobalData
{
	public const string NameOrderImagePathLastFirst = "/resources/Sorting/Name-Order-Icon_Last-First.png";
	public const string NameOrderImagePathFirstLast = "/resources/Sorting/Name-Order-Icon_First-Last.png";

	public const string PlaytimeOrderImagePathLastFirst = "/resources/Sorting/Playtime-Order-Icon_Last-First.png";
	public const string PlaytimeOrderImagePathFirstLast = "/resources/Sorting/Playtime-Order-Icon_First-Last.png";
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

internal static partial class GlobalData
{
	public const string LocPlaytimeHoursGraphY = "app-view.graph.playtime-hours-y";
	public const string LocMonthGraphY = "app-view.graph.month-y";
	public const string LocDayGraphY = "app-view.graph.day-y";

	public const string LocPlaytimeHoursText = "app-view.hours-playtime-indicator";
	public const string LocAppViewYearPlaytimeViewKey = "app-view.year-playtime";
	public const string LocAppViewMonthPlaytimeViewKey = "app-view.month-playtime";
	public const string LocAppViewDayPlaytimeViewKey = "app-view.day-playtime";

	public const string NoAppsFoundKey = "main.no-apps-found";

	public const string FolderSelectOpenKey = "settings.option.open-folder";

	public const string SteamInstallFolderPlaceholderKey = "settings.steam-install-placeholder-indicator";
	public const string SteamInstallFolderNotSelectedWarnignKey = "settings.steam-install-invalid";
	public const string SteamInstallFolderTitleKey = "settings.steam-install-title";
	public const string SteamInstallLocationKey = "settings.steam-install";
	public const string SelectLocalizationKey = "settings.select-Localization";
	public const string OpenLogsKey = "settings.open-logs";

	public const string ConfirmKey = "global.confirm";
	public const string SettingsLocKey = "global.settings";
}