using SteamPlaytimeTracker.Localization.Data;
using System.ComponentModel;

namespace SteamPlaytimeTracker.Services.Localization;

internal interface ILocalizationService : INotifyPropertyChanged
{
	string CurrentLocaleCode { get; }
	LocaleData? CurrentLocale { get; }
	string this[string key, (string Key, object Value)[] paramaters] { get; }
	string this[string key] { get; }
	void ChangeLocale(LocaleData locale);
	void ChangeLocale(string code);
}