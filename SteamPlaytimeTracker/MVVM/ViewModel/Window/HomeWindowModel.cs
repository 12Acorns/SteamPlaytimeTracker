using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.Localization;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.Core;

namespace SteamPlaytimeTracker.MVVM.ViewModel.Window;

internal sealed class HomeWindowModel : MenuModel
{
	public HomeWindowModel(INavigationService navigationService,  AppConfig config, LocalizationManager localization)
	{
		// TODO: add localization support for the title
		Title = Random.Shared.Next(0, 1000) == 28 ? "OwO :3" : "Playtime Tracker";
		NavigationService = navigationService;

#if DEBUG_TEST_STEAM_APP_VIEW
		NavigationService.NavigateTo<SteamAppViewModel>();
#else
		if(!Directory.Exists(config.AppData.SteamInstallData.SteamInstallationFolder))
		{
			NavigationService.NavigateTo<SettingsViewModel>();
		}
		else
		{
			NavigationService.NavigateTo<HomeViewModel>();
		}
#endif
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
