using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.Localization;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.Core;
using System.IO;

namespace SteamPlaytimeTracker.MVVM.ViewModel.Window;

internal sealed class HomeWindowModel : MenuModel
{
	public HomeWindowModel(INavigationService navigationService,  AppConfig config, LocalizationManager localization)
	{
		// TODO: add localization support for the title
		Title = Random.Shared.Next(0, 1000) == 54 ? "Home Window :3" : "Home Window";
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
}
