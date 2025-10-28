using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.SelfConfig;
using System.IO;
using SteamPlaytimeTracker.Localization;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class HomeWindowModel : Core.ViewModel
{
	private INavigationService _navigationService;

	public HomeWindowModel(INavigationService navigationService,  AppConfig config, LocalizationManager localization)
	{
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
		get => _navigationService;
		set
		{
			_navigationService = value;
			OnPropertyChanged();
		}
	}
}
