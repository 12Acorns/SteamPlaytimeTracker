using SteamPlaytimeTracker.Localization.Data;
using System.ComponentModel;

namespace SteamPlaytimeTracker.Services.Localization;

internal interface ILocalizationService : INotifyPropertyChanged
{
	string CurrentLocaleCode { get; }
	LocaleData? CurrentLocale { get; }
	LocalizedText this[string key] { get; }
	string GetFormattedTranslation(string key, params (string Key, object Value)[] paramaters);
	void ChangeLocale(LocaleData locale);
	void ChangeLocale(string code);
}