using Microsoft.Extensions.DependencyInjection;
using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.SelfConfig.Data;
using SteamPlaytimeTracker.Services.Steam;
using SteamPlaytimeTracker.MVVM.ViewModel;
using SteamPlaytimeTracker.Utility.Cache;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.MVVM.View;
using Microsoft.EntityFrameworkCore;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.IO;
using System.ComponentModel;
using System.Windows;
using Serilog.Core;
using Config.Net;
using System.IO;
using Serilog;

namespace SteamPlaytimeTracker;

public partial class App : Application
{
	private readonly ServiceProvider _serviceProvider;

	public static event EventHandler<CancelEventArgs>? OnSessionClose;
	public static event EventHandler<SessionEndingCancelEventArgs>? OnSessionEndingA;

	public static ServiceProvider ServiceProvider { get; private set; } = default!;

	public App()
	{
		ApplicationPath.TryAddPath(GlobalData.AppDataStoreLookupName, "Steam Playtime Tracker");
		ApplicationPath.TryAddPath(GlobalData.ConfigPathLookupName, "Steam Playtime Tracker", "AppData.json");
		ApplicationPath.TryAddPath(GlobalData.DbLookupName, "Steam Playtime Tracker", "appusage.db");
		ApplicationPath.TryAddPath(GlobalData.TmpFolderName, Directory.CreateTempSubdirectory("Steam Playtime Tracker").FullName, ApplicationPathOption.CustomGlobal);

		var iConfigData = new ConfigurationBuilder<IAppData>()
			.UseJsonFile(ApplicationPath.GetPath(GlobalData.ConfigPathLookupName))
			.Build();

		iConfigData.AppVersion = "0.1.0";

		ApplicationPath.TryAddPath(GlobalData.MainTimeSliceCheckLookupName, ApplicationPathOption.CustomGlobal,
			iConfigData.SteamInstallData.SteamInstallationFolder ?? "", GlobalData.MainSliceCheckLocalPath);

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
		serviceCollection.AddSingleton<SteamAppView>(provider => new SteamAppView()
		{
			DataContext = provider.GetRequiredService<SteamAppViewModel>()
		});
		serviceCollection.AddSingleton<AppConfig>(provider => new AppConfig(iConfigData));
		serviceCollection.AddDbContext<DbAccess>(options => options.UseSqlite($"Data Source={ApplicationPath.GetPath(GlobalData.DbLookupName)}"), ServiceLifetime.Transient);

		serviceCollection.AddSingleton<HomeWindowModel>();
		serviceCollection.AddSingleton<HomeViewModel>();
		serviceCollection.AddSingleton<SettingsViewModel>();
		serviceCollection.AddSingleton<SteamAppViewModel>();
		serviceCollection.AddSingleton<INavigationService, ViewModelNavigationService>();
		serviceCollection.AddSingleton<IAppService, AppService>();
		serviceCollection.AddSingleton<ICacheManager, CacheManager>();
		serviceCollection.AddSingleton<ILogger, Logger>(provider => LoggingService.Logger);
		serviceCollection.AddSingleton<IAsyncLifetimeService, ApplicationEndAsyncLifetimeService>(provider => ApplicationEndAsyncLifetimeService.Default);

		serviceCollection.AddSingleton<Func<Type, object[], ViewModel>>(provider => (viewModelType, @params) =>
		{
			var model = (ViewModel)provider.GetRequiredService(viewModelType);
			if(!model.IsConstructed)
			{
				model.OnConstructed();
			}
			model.OnLoad(@params);
			return model;
		});

		_serviceProvider = serviceCollection.BuildServiceProvider();
		ServiceProvider = _serviceProvider;

		OnSessionClose += (sender, e) =>
		{
			if(e.Cancel)
			{
				return;
			}
			if(ApplicationPath.TryGetPath(GlobalData.TmpFolderName, out var tmpDirectory) && Directory.Exists(tmpDirectory))
			{
				Directory.Delete(tmpDirectory, true);
			}
		};
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		var db = _serviceProvider.GetRequiredService<DbAccess>();
		db.Database.Migrate();

		var mainWindow = _serviceProvider.GetRequiredService<HomeWindow>();
		mainWindow.Show();
		base.OnStartup(e);
	}

	public static void Application_Closing(object sender, CancelEventArgs e) => OnSessionClose?.Invoke(sender, e);
	private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e) => OnSessionEndingA?.Invoke(this, e);
}
