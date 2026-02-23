
using SteamPlaytimeTracker.Core;
using System.Windows.Controls;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.MVVM.ViewModel;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.WindowChrome;

public partial class WindowChromeBar : UserControl
{
	private readonly INavigationService _navigationService;

    public WindowChromeBar()
    {
        InitializeComponent();
        DataContext = this;

		_navigationService = App.ServiceProvider.GetRequiredService<INavigationService>();

		SettingsCommand = new(_ =>
		{
			_navigationService.NavigateTo<SettingsViewModel>();
		});
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

	public RelayCommand SettingsCommand { get; }
	public RelayCommand MinimizeCommand { get; }
	public RelayCommand ToggleSizeCommand { get; }
	public RelayCommand QuitApplicationCommand { get; }
}