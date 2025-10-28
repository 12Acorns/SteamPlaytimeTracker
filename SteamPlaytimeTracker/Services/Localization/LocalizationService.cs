using SteamPlaytimeTracker.Localization.Data;
using SteamPlaytimeTracker.Localization;
using SteamPlaytimeTracker.SelfConfig;
using System.ComponentModel;
using Serilog;

namespace SteamPlaytimeTracker.Services.Localization;

internal class LocalizationService : ILocalizationService
{
	private readonly ILogger _logger;
	private readonly AppConfig _config;
	private readonly LocalizationManager _manager;

	public event PropertyChangedEventHandler? PropertyChanged;

	public string CurrentLocaleCode => _manager.Current?.Code ?? "en-gb";
	public LocaleData? CurrentLocale => _manager.Current;

	public LocalizationService(ILogger logger, AppConfig config, LocalizationManager manager)
	{
		_logger = logger;
		_config = config;
		_manager = manager;
	}

	public string this[string key, (string Key, object Value)[] paramaters]
	{
		get
		{
			var template = this[key];
			if(template == key)
				return template;
			if(string.IsNullOrEmpty(template))
				return template;
			foreach(var (keyI, value) in paramaters)
			{
				template = template.Replace($"{{{keyI}}}", value?.ToString() ?? string.Empty);
			}
			if(template.Contains('{'))
			{
				_logger.Warning("Translation for key: {Key} may be missing parameters. Resulting template: {Template}", key, template);
			}
			return template;
		}
	}
	public string this[string key]
	{
		get
		{
			if(_manager.TryGetTranslatedString(key, out var value))
				return value;
			_logger.Warning("Missing translation for key: {Key}", key);
			return $"[{key}]"; // obvious placeholder for missing keys
		}
	}

	public void ChangeLocale(LocaleData locale)
	{
		_manager.LoadLocale(locale);
		_config.AppData.LocalizationData.LanguageCode = locale.Code;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
	}
	public void ChangeLocale(string code)
	{
		_manager.LoadLocale(code);
		_config.AppData.LocalizationData.LanguageCode = code;
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
	}
}