using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.SelfConfig;
using System.IO;
using SteamPlaytimeTracker.Core;
using System.Windows;
using SteamPlaytimeTracker.Services.Lifetime;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class HomeWindowModel : Core.ViewModel
{
	private readonly IAsyncLifetimeService _tokenLifetime;
	private INavigationService _navigationService;

	public HomeWindowModel(INavigationService navigationService, IAsyncLifetimeService tokenLifetime,  AppConfig config)
	{
		NavigationService = navigationService;
		_tokenLifetime = tokenLifetime;
		if(!Directory.Exists(config.AppData.SteamInstallationFolder))
		{
			NavigationService.NavigateTo<SettingsViewModel>();
			return;
		}
		NavigationService.NavigateTo<HomeViewModel>();

		MinimizeCommand = new(_ =>
		{
			Application.Current.MainWindow.WindowState = WindowState.Minimized;
		});
		ToggleSizeCommand = new(_ =>
		{
			Application.Current.MainWindow.WindowState = Application.Current.MainWindow.WindowState switch
			{
				WindowState.Maximized => WindowState.Normal,
				WindowState.Normal => WindowState.Maximized,
				_ => WindowState.Normal
			};
		});
		QuitApplicationCommand = new(_ =>
		{
			Application.Current.MainWindow.Close();
		});
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

	public RelayCommand MinimizeCommand { get; private set; }
	public RelayCommand ToggleSizeCommand { get; private set; }
	public RelayCommand QuitApplicationCommand { get; private set; }
}
