using SteamPlaytimeTracker.Localization.Data;
using SteamPlaytimeTracker.Utility.Cache;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.Extensions;
using System.Diagnostics.CodeAnalysis;
using SteamPlaytimeTracker.IO;
using System.Reflection;
using System.Text.Json;
using System.IO;
using Serilog;
using System.Runtime.Caching;

namespace SteamPlaytimeTracker.Localization;

internal sealed class LocalizationManager
{
	private static readonly JsonSerializerOptions _options = new()
	{
		AllowTrailingCommas = true
	};

	public static LocalizationManager? Instance { get; private set; }

	private readonly ICacheManager _cacheManager;
	private readonly AppConfig _config;
	private readonly ILogger _logger;
	private LocaleData? _locale;

	public LocalizationManager(ILogger logger, AppConfig config, ICacheManager cacheManager)
	{
		_logger = logger;
		_config = config;
		_cacheManager = cacheManager;
		Instance = this;
	}


	public LocaleData? Current => _locale;

	public bool TryGetTranslatedString(string key, [NotNullWhen(true)] out string? translation)
	{
		translation = null;
		return _locale?.LocaleTextMap?.TryGetValue(key, out translation) ?? false;
	}
	public bool TryLoadLocale(LocaleData data)
	{
		var filePath = Path.Combine(
			ApplicationPath.GetPath(GlobalData.LocalizationLookupName), 
			GlobalData.LocalesFolderName,
			$"{data.Code}.json");
		if(!File.Exists(filePath))
		{
			_logger.Warning("Locale file not found: {LocaleFilePath}", filePath);
			if(!LoadEmbededLocaleAndDumpToDisk(data, filePath))
			{
				_logger.Warning("Falling back to en-gb locale.");
				return LoadEnGbLocaleEmbededAndDumpToDisk(Path.Combine(
					ApplicationPath.GetPath(GlobalData.LocalizationLookupName),
					GlobalData.LocalesFolderName,
					$"en-GB.json"));
			}
			_logger.Information("Loaded from embeded resource, contents dumped to disk: {LocaleFilePath}", filePath);
			return true;
		}
		using var stream = File.OpenRead(filePath);
		_locale = new LocaleData
		{
			Code = data.Code,
			DisplayName = data.DisplayName,
			LocaleTextMap = JsonSerializer.Deserialize<TranslationMap>(stream, _options)?.Map ?? []
		};
		_config.AppData.LocalizationData.LanguageCode = _locale.Code;
		return true;
	}
	/// <exception cref="FileNotFoundException"></exception>
	public void LoadLocale(string code)
	{
		var match = GetAvailableLocales().FirstOrDefault(x => x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));
		TryLoadLocale(match ?? throw new FileNotFoundException("Locale code not found in available locales.", code));
	}
	public IEnumerable<LocaleData> GetAvailableLocales()
	{
		var localeDir = ApplicationPath.GetPath(GlobalData.LocalizationLookupName);
		var localesMapFilePath = Path.Combine(localeDir, GlobalData.LocaleMapFileName);
		if(!File.Exists(localesMapFilePath))
		{
			_logger.Warning("Locales map file not found: {LocalesMapFilePath}", localesMapFilePath);
			return [];
		}
		return _cacheManager.GetOrAdd(GlobalData.AvailableLocalesCacheKey, TimeSpan.FromHours(1), () =>
		{
			using var stream = File.OpenRead(Path.Combine(localeDir, GlobalData.LocaleMapFileName));
			var container = JsonSerializer.Deserialize<LocaleContainer>(stream);
			return container?.Locales ?? [];
		}, new HostFileChangeMonitor([localesMapFilePath]));
	}
	private bool LoadEnGbLocaleEmbededAndDumpToDisk(string filePath) => LoadEmbededLocaleAndDumpToDisk(new LocaleData
	{
		Code = "en-gb",
		DisplayName = "English"
	}, filePath);
	private bool LoadEmbededLocaleAndDumpToDisk(LocaleData locale, string filePath)
	{
		var asm = Assembly.GetExecutingAssembly();
		var resourceName = asm.GetManifestResourceNames()
			.FirstOrDefault(n => n.EndsWith($"{locale.Code}.json", StringComparison.OrdinalIgnoreCase));

		if(resourceName == null)
		{
			LoadNoLocale();
			return false;
		}
		using var stream = asm.GetManifestResourceStream(resourceName);
		if(stream == null)
		{
			LoadNoLocale();
			return false;
		}

		_locale = new LocaleData
		{
			Code = locale.Code,
			DisplayName = $"{locale.DisplayName} (Embeded)",
			LocaleTextMap = JsonSerializer.Deserialize<TranslationMap>(stream, _options)?.Map ?? []
		};
		_config.AppData.LocalizationData.LanguageCode = _locale.Code;

		using var file = File.Create(filePath);
		stream.Position = 0;
		stream.CopyTo(file);
		return true;
	}
	private void LoadNoLocale() => _locale = GlobalData.NoLocaleFound;
}