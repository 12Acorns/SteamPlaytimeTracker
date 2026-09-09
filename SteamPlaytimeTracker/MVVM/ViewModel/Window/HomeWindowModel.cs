using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.Localization;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.Core;
using System.IO;
using System.Windows;

namespace SteamPlaytimeTracker.MVVM.ViewModel.Window;

internal sealed class HomeWindowModel : MenuModel
{
	public HomeWindowModel(INavigationService navigationService,  AppConfig config, LocalizationManager localization)
	{
		// TODO: add localization support for the title
		Title = Random.Shared.Next(0, 1000) == 28 ? "OwO :3" : "Playtime Tracker";
		NavigationService = navigationService;

		if(!Directory.Exists(config.AppData.SteamInstallData.SteamInstallationFolder))
		{
			NavigationService.NavigateTo<SettingsViewModel>();
		}
		else
		{
			NavigationService.NavigateTo<HomeViewModel>();
		}
	}

	public INavigationService NavigationService
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public Visibility SettingsButtonVisibility
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
}
