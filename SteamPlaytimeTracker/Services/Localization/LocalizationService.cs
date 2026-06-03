using SteamPlaytimeTracker.Localization.Data;
using SteamPlaytimeTracker.Localization;
using SteamPlaytimeTracker.SelfConfig;
using System.ComponentModel;
using Serilog;
using SteamPlaytimeTracker.Utility.Text;

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

	public string this[string key, params (string Key, object Value)[] paramaters]
	{
		get
		{
			var template = this[key];
			if(template.Key == key || string.IsNullOrEmpty(template.Text))
			{
				return template.Text;
			}
			var formatted = StringUtility.FormatNamed(template.Text, paramaters);
			if(formatted.Contains('{'))
			{
				_logger.Warning("Translation for key: {Key} may be missing parameters. Resulting template: {Template}", key, formatted);
			}
			return formatted;
		}
	}
	public LocalizedText this[string key]
	{
		get
		{
			if(_manager.TryGetTranslatedString(key, out var value))
			{
				return LocalizedText.Create(value, key, 0);
			}
			_logger.Warning("Missing translation for key: {Key}", key);
			// placeholder for missing keys
			return LocalizedText.Create($"[{key}]", key, 0);
		}
	}

	public void ChangeLocale(LocaleData locale)
	{
		_manager.TryLoadLocale(locale);
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