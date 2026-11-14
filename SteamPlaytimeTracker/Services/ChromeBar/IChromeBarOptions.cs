using System.ComponentModel;
using System.Windows;

namespace SteamPlaytimeTracker.Services.ChromeBar;

public interface IChromeBarOptions : INotifyPropertyChanged
{
	public Visibility SettingsButtonVisibility { get; set; }
	public Visibility GoToPreviousMenuButtonVisibility { get; set; }
}