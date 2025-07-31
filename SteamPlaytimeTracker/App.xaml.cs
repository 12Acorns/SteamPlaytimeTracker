using Microsoft.Extensions.DependencyInjection;
using SteamPlaytimeTracker.SelfConfig.Data;
using SteamPlaytimeTracker.MVVM.ViewModel;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.MVVM.View;
using SteamPlaytimeTracker.Services;
using Microsoft.EntityFrameworkCore;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.IO;
using System.Windows;
using Serilog.Core;
using Config.Net;
using Serilog;

namespace SteamPlaytimeTracker;

public partial class App : Application
{
	private readonly ServiceProvider _serviceProvider;

	public App()
	{
		ApplicationPath.TryAddPath(GlobalData.AppDataStoreLookupName, "Steam Playtime Tracker");
		ApplicationPath.TryAddPath(GlobalData.ConfigPathLookupName, "Steam Playtime Tracker", "AppData.json");
		ApplicationPath.TryAddPath(GlobalData.DbLookupName, "Steam Playtime Tracker", "appusage.db");

		var iConfigData = new ConfigurationBuilder<IAppData>()
			.UseJsonFile(ApplicationPath.GetPath(GlobalData.ConfigPathLookupName))
			.Build();

		ApplicationPath.TryAddPath(GlobalData.MainTimeSliceCheckLookupName, ApplicationPathOption.CustomGlobal,
			iConfigData.SteamInstallationFolder, GlobalData.MainSliceCheckLocalPath);

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
		serviceCollection.AddSingleton<AppConfig>(provider => new AppConfig(iConfigData));
		serviceCollection.AddDbContext<DbAccess>(options => options.UseSqlite($"Data Source={ApplicationPath.GetPath(GlobalData.DbLookupName)}"));

		serviceCollection.AddSingleton<HomeWindowModel>();
		serviceCollection.AddSingleton<HomeViewModel>();
		serviceCollection.AddSingleton<SettingsViewModel>();
		serviceCollection.AddSingleton<INavigationService, ViewModelNavigationService>();
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
