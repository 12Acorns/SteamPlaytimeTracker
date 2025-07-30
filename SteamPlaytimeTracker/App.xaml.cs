using Microsoft.Extensions.DependencyInjection;
using SteamPlaytimeTracker.SelfConfig.Data;
using SteamPlaytimeTracker.MVVM.ViewModel;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.MVVM.View;
using SteamPlaytimeTracker.Services;
using SteamPlaytimeTracker.Core;
using System.Windows;
using Config.Net;
using System.IO;
using Serilog;
using Serilog.Core;

namespace SteamPlaytimeTracker;

public partial class App : Application
{
	private readonly ServiceProvider _serviceProvider;

	public App()
	{
		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSingleton<HomeWindow>(provider => new HomeWindow()
		{
			DataContext = provider.GetRequiredService<HomeWindowModel>()
		});
		serviceCollection.AddSingleton<SettingsView>(provider => new SettingsView()
		{
			DataContext = provider.GetRequiredService<SettingsViewModel>()
		});
		serviceCollection.AddSingleton<HomeView>(provider => new HomeView()
		{
			DataContext = provider.GetRequiredService<HomeViewModel>()
		});
		serviceCollection.AddSingleton<AppConfig>(provider => new AppConfig(new ConfigurationBuilder<IAppData>()
			.UseJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Steam Playtime Tracker", "AppData.json"))
			.Build()));

		serviceCollection.AddSingleton<DbAccess>();
		serviceCollection.AddSingleton<HomeWindowModel>();
		serviceCollection.AddSingleton<HomeViewModel>();
		serviceCollection.AddSingleton<SettingsViewModel>();
		serviceCollection.AddSingleton<INavigationService, NavigationService>();
		serviceCollection.AddSingleton<ILogger, Logger>(provider => LoggingService.Logger);

		serviceCollection.AddSingleton<Func<Type, ViewModel>>(provider => viewModelType => (ViewModel)provider.GetRequiredService(viewModelType));

		_serviceProvider = serviceCollection.BuildServiceProvider();
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		var mainWindow = _serviceProvider.GetRequiredService<HomeWindow>();
		mainWindow.Show();
		base.OnStartup(e);
	}
}
