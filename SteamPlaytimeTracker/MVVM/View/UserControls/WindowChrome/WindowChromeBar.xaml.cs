using Microsoft.Extensions.DependencyInjection;
using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.MVVM.ViewModel;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.Core;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.WindowChrome;

public partial class WindowChromeBar : UserControl, INotifyPropertyChanged
{
	private static readonly DependencyProperty _titleProperty = DependencyProperty.Register(
		nameof(Title), typeof(string), typeof(WindowChromeBar), new PropertyMetadata(string.Empty));
	private static readonly DependencyProperty _settingsButtonVisibility = DependencyProperty.Register(
		nameof(SettingsButtonVisibility), typeof(Visibility), typeof(WindowChromeBar), new PropertyMetadata(Visibility.Visible));

	private readonly INavigationService _navigationService;

    public WindowChromeBar()
    {
        InitializeComponent();
		DataContext = this;

		_navigationService = App.ServiceProvider?.GetRequiredService<INavigationService>() ?? default!;

		Loaded += (_, _) => Load();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public string Title
	{
		get => (string)GetValue(_titleProperty);
		set => SetValue(_titleProperty, value);
	}
	public Visibility SettingsButtonVisibility
	{
		get => (Visibility)GetValue(_settingsButtonVisibility);
		set => SetValue(_settingsButtonVisibility, value);
	}
	public RelayCommand SettingsCommand
	{
		get;
		private set
		{
			field = value;
			PropertyChanged.OnPropertyChanged(this);
		}
	} = default!;
	public RelayCommand MinimizeCommand
	{
		get;
		private set
		{
			field = value;
			PropertyChanged.OnPropertyChanged(this);
		}
	} = default!;
	public RelayCommand ToggleSizeCommand
	{
		get;
		private set
		{
			field = value;
			PropertyChanged.OnPropertyChanged(this);
		}
	} = default!;
	public RelayCommand QuitApplicationCommand
	{
		get;
		private set
		{
			field = value;
			PropertyChanged.OnPropertyChanged(this);
		}
	} = default!;

	private void Load()
	{
		var window = Window.GetWindow(this);

		SettingsCommand = new(_ => _navigationService.NavigateTo<SettingsViewModel>());
		MinimizeCommand = new(_ => window.WindowState = WindowState.Minimized);
		ToggleSizeCommand = new(_ =>
		{
			window.WindowState = window.WindowState switch
			{
				WindowState.Maximized => WindowState.Normal,
				WindowState.Normal => WindowState.Maximized,
				_ => WindowState.Normal
			};
		});
		QuitApplicationCommand = new(_ => window.Close());
		Loaded -= (_, _) => Load();
	}
}