using Microsoft.Extensions.DependencyInjection;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.MVVM.ViewModel;
using SteamPlaytimeTracker.Services.ChromeBar;
using SteamPlaytimeTracker.Services.Navigation;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.WindowChrome;

public partial class WindowChromeBar : UserControl, INotifyPropertyChanged
{
	private static readonly DependencyProperty _titleProperty = DependencyProperty.Register(
		nameof(Title), typeof(string), typeof(WindowChromeBar), new PropertyMetadata(string.Empty));
	private readonly INavigationService _navigationService;

	public WindowChromeBar()
    {
		DataContext = this;
        InitializeComponent();

		_navigationService = App.ServiceProvider?.GetRequiredService<INavigationService>() ?? default!;
		Options = App.ServiceProvider?.GetRequiredService<IChromeBarOptions>() ?? default!;

		Loaded += Load;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public string Title
	{
		get => (string)GetValue(_titleProperty);
		set => SetValue(_titleProperty, value);
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
	public IChromeBarOptions Options { get; }

	private void Load(object sender, RoutedEventArgs e)
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
		Loaded -= Load;
	}
}