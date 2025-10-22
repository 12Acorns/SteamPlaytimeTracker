using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.SelfConfig;
using System.IO;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class HomeWindowModel : Core.ViewModel
{
	public HomeWindowModel(INavigationService navigationService,  AppConfig config)
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
		get => field;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
}
