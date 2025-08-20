using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.Services;
using System.IO;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class HomeWindowModel : Core.ViewModel
{
	private INavigationService _navigationService;

	public HomeWindowModel(INavigationService navigationService, AppConfig config)
	{
		NavigationService = navigationService;
		if(!Directory.Exists(config.AppData.SteamInstallationFolder))
		{
			NavigationService.NavigateTo<SettingsViewModel>();
			return;
		}
		NavigationService.NavigateTo<HomeViewModel>();
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
