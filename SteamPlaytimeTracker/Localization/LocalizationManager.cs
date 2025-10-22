using Serilog;
using SteamPlaytimeTracker.IO;
using SteamPlaytimeTracker.Localization.Data;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.Utility;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace SteamPlaytimeTracker.Localization;

internal sealed class LocalizationManager
{
	public LocalizationManager(ILogger logger, AppConfig config)
	{
		_logger = logger;
		_config = config;
	}

	private readonly ILogger _logger;
	private readonly AppConfig _config;
	private LocaleData _locale;

	public static IEnumerable<LocaleData> GetAvailableLocales()
	{
		var localeDir = ApplicationPath.GetPath(GlobalData.LocalizationLookupName);
		using var stream = File.OpenRead(Path.Combine(localeDir, GlobalData.LocaleMapFileName));
		var container = JsonSerializer.Deserialize<LocaleContainer>(stream);
		return container?.Locales ?? [];
	}
	public string GetTranslatedString(string key)
	{
		// Placeholder implementation
		return key;
	}
	public void LoadLocale(string localeCode)
	{
	}
}

internal sealed class LocaleContainer
{
	[JsonPropertyName("locales")]
	public List<LocaleData> Locales { get; set; } = [];
}