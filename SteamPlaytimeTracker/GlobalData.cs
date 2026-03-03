global using static SteamPlaytimeTracker.Utility.VisualTreeUtility;
global using static SteamPlaytimeTracker.Utility.ResourceUtility;
using SteamPlaytimeTracker.Localization.Data;
using System.Globalization;
using System.IO;
using ScottPlot;

namespace SteamPlaytimeTracker;

internal static partial class GlobalData
{
	internal static class SVGData
	{
		public const string DataAlertIcon = "M120-160v-80h480v80H120Zm378.5-338.5Q440-557 440-640t58.5-141.5Q557-840 640-840t141.5 58.5Q840-723 840-640t-58.5 141.5Q723-440 640-440t-141.5-58.5ZM120-480v-80h252q7 22 16 42t22 38H120Zm0 160v-80h376q23 14 49 23.5t55 13.5v43H120Zm500-280h40v-160h-40v160Zm34 74q6-6 6-14t-6-14q-6-6-14-6t-14 6q-6 6-6 14t6 14q6 6 14 6t14-6Z";
		public const string ErrorIcon = "M508.5-291.5Q520-303 520-320t-11.5-28.5Q497-360 480-360t-28.5 11.5Q440-337 440-320t11.5 28.5Q463-280 480-280t28.5-11.5ZM440-440h80v-240h-80v240Zm40 360q-83 0-156-31.5T197-197q-54-54-85.5-127T80-480q0-83 31.5-156T197-763q54-54 127-85.5T480-880q83 0 156 31.5T763-763q54 54 85.5 127T880-480q0 83-31.5 156T763-197q-54 54-127 85.5T480-80Zm0-80q134 0 227-93t93-227q0-134-93-227t-227-93q-134 0-227 93t-93 227q0 134 93 227t227 93Zm0-320Z";
		public const string InformationIcon = "M440-280h80v-240h-80v240Zm68.5-331.5Q520-623 520-640t-11.5-28.5Q497-680 480-680t-28.5 11.5Q440-657 440-640t11.5 28.5Q463-600 480-600t28.5-11.5ZM480-80q-83 0-156-31.5T197-197q-54-54-85.5-127T80-480q0-83 31.5-156T197-763q54-54 127-85.5T480-880q83 0 156 31.5T763-763q54 54 85.5 127T880-480q0 83-31.5 156T763-197q-54 54-127 85.5T480-80Zm0-80q134 0 227-93t93-227q0-134-93-227t-227-93q-134 0-227 93t-93 227q0 134 93 227t227 93Zm0-320Z";
		public const string NotificationAlertIcon = "M480-80q-33 0-56.5-23.5T400-160h160q0 33-23.5 56.5T480-80Zm0-420ZM160-200v-80h80v-280q0-83 50-147.5T420-792v-28q0-25 17.5-42.5T480-880q25 0 42.5 17.5T540-820v13q-11 22-16 45t-4 47q-10-2-19.5-3.5T480-720q-66 0-113 47t-47 113v280h320v-257q18 8 38.5 12.5T720-520v240h80v80H160Zm475-435q-35-35-35-85t35-85q35-35 85-35t85 35q35 35 35 85t-35 85q-35 35-85 35t-85-35Z";
		public const string NotificationIcon = "M160-200v-80h80v-280q0-83 50-147.5T420-792v-28q0-25 17.5-42.5T480-880q25 0 42.5 17.5T540-820v28q80 20 130 84.5T720-560v280h80v80H160Zm320-300Zm0 420q-33 0-56.5-23.5T400-160h160q0 33-23.5 56.5T480-80ZM320-280h320v-280q0-66-47-113t-113-47q-66 0-113 47t-47 113v280Z";
		public const string WarningIcon = "m40-120 440-760 440 760H40Zm138-80h604L480-720 178-200Zm330.5-51.5Q520-263 520-280t-11.5-28.5Q497-320 480-320t-28.5 11.5Q440-297 440-280t11.5 28.5Q463-240 480-240t28.5-11.5ZM440-360h80v-200h-80v200Zm40-100Z";
		public const string SettingsCogIcon = "M19.14,12.94c0.04-0.3,0.06-0.61,0.06-0.94c0-0.32-0.02-0.64-0.07-0.94l2.03-1.58c0.18-0.14,0.23-0.41,0.12-0.61 l-1.92-3.32c-0.12-0.22-0.37-0.29-0.59-0.22l-2.39,0.96c-0.5-0.38-1.03-0.7-1.62-0.94L14.4,2.81c-0.04-0.24-0.24-0.41-0.48-0.41 h-3.84c-0.24,0-0.43,0.17-0.47,0.41L9.25,5.35C8.66,5.59,8.12,5.92,7.63,6.29L5.24,5.33c-0.22-0.08-0.47,0-0.59,0.22L2.74,8.87 C2.62,9.08,2.66,9.34,2.86,9.48l2.03,1.58C4.84,11.36,4.8,11.69,4.8,12s0.02,0.64,0.07,0.94l-2.03,1.58 c-0.18,0.14-0.23,0.41-0.12,0.61l1.92,3.32c0.12,0.22,0.37,0.29,0.59,0.22l2.39-0.96c0.5,0.38,1.03,0.7,1.62,0.94l0.36,2.54 c0.05,0.24,0.24,0.41,0.48,0.41h3.84c0.24,0,0.44-0.17,0.47-0.41l0.36-2.54c0.59-0.24,1.13-0.56,1.62-0.94l2.39,0.96 c0.22,0.08,0.47,0,0.59-0.22l1.92-3.32c0.12-0.22,0.07-0.47-0.12-0.61L19.14,12.94z M12,15.6c-1.98,0-3.6-1.62-3.6-3.6 s1.62-3.6,3.6-3.6s3.6,1.62,3.6,3.6S13.98,15.6,12,15.6z";
	}
}

internal static partial class GlobalData
{
	private const string SteamStoreDomainUrl = "https://store.steampowered.com";

	public const string SteamHttpClientKey = "steam";
	public const string SingleAppDetailsUrl = $"{SteamStoreDomainUrl}/api/";
	public const string SteamHttpPipelineKey = SteamHttpClientKey;
}

internal static partial class GlobalData
{
	public const string AppVersion = "0.1.0";
}

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
	public const string DateTimeFormatString = "yyyy-MM-ddTHH:mm:sszzz";

	public static readonly CultureInfo GbCulture = CultureInfo.GetCultureInfo("en-GB");

	public static string DateToString(DateTimeOffset offset) => offset.ToString(DateTimeFormatString, GbCulture);
	public static bool TryParseDateTimeOffset(string dto, out DateTimeOffset offset) => DateTimeOffset.TryParseExact(dto, DateTimeFormatString, GbCulture, DateTimeStyles.None, out offset);
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

	public const string LoadingAppsKey = "main.loading-apps";
	public const string NoAppsFoundKey = "main.no-apps-found";

	public const string FolderSelectOpenKey = "settings.option.open-folder";

	public const string SteamInstallFolderPlaceholderKey = "settings.steam-install-placeholder-indicator";
	public const string SteamInstallFolderNotSelectedWarnignKey = "settings.steam-install-invalid";
	public const string SteamInstallFolderTitleKey = "settings.steam-install-title";
	public const string SteamInstallLocationKey = "settings.steam-install";
	public const string SelectLocalizationKey = "settings.select-Localization";
	public const string SelectLogLevelKey = "settings.select-log-level";
	public const string OpenLogsKey = "settings.open-logs";

	public const string ConfirmKey = "global.confirm";
	public const string SettingsLocKey = "global.settings";
}