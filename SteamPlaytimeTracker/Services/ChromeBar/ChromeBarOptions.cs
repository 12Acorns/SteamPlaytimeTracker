using SteamPlaytimeTracker.Extensions;
using System.ComponentModel;
using System.Windows;

namespace SteamPlaytimeTracker.Services.ChromeBar;

public sealed class ChromeBarOptions : IChromeBarOptions
{
	public Visibility SettingsButtonVisibility
	{
		get;
		set
		{
			field = value;
			PropertyChanged.OnPropertyChanged(this);
		}
	}
	public Visibility GoToPreviousMenuButtonVisibility
	{
		get;
		set
		{
			field = value;
			PropertyChanged.OnPropertyChanged(this);
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;
}