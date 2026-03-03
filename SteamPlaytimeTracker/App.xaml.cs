using SteamPlaytimeTracker.Services.Localization;
using SteamPlaytimeTracker.MVVM.ViewModel.Window;
using SteamPlaytimeTracker.Services.DataTransfer;
using Microsoft.Extensions.DependencyInjection;
using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.Services.Web.Steam;
using SteamPlaytimeTracker.Services.Messaging;
using SteamPlaytimeTracker.Services.Playtime;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.SelfConfig.Data;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.MVVM.ViewModel;
using SteamPlaytimeTracker.Services._App;
using SteamPlaytimeTracker.Services.Disk;
using SteamPlaytimeTracker.Services.Menu;
using SteamPlaytimeTracker.Utility.Cache;
using SteamPlaytimeTracker.Localization;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.MVVM.View;
using Microsoft.EntityFrameworkCore;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.IO;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows;
using Polly.Timeout;
using Serilog.Core;
using Polly.Retry;
using System.Net;
using Config.Net;
using System.IO;
using Serilog;
using OneOf;
using Polly;

namespace SteamPlaytimeTracker;

public partial class App : Application
{
	public static event EventHandler<CancelEventArgs>? OnSessionClose;
	public static event EventHandler<SessionEndingCancelEventArgs>? OnSessionEndingA;

	public static ServiceProvider ServiceProvider { get; private set; } = default!;

	public App()
	{
		InitializeComponent();

		FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata
		{
			DefaultValue = FindResource(typeof(Window))
		});

		ApplicationPath.TryAddPath(GlobalData.LocalizationLookupName, ApplicationPathOption.ExeLocation, "locale");
		ApplicationPath.TryAddPath(GlobalData.AppDataStoreLookupName, "Steam Playtime Tracker");
		ApplicationPath.TryAddPath(GlobalData.ConfigPathLookupName, "Steam Playtime Tracker", "AppData.json");
		ApplicationPath.TryAddPath(GlobalData.DbLookupName, "Steam Playtime Tracker", "appusage.db");
		ApplicationPath.TryAddPath(GlobalData.TmpFolderName, Directory.CreateTempSubdirectory("Steam Playtime Tracker").FullName, ApplicationPathOption.CustomGlobal);

		var iConfigData = new ConfigurationBuilder<IAppData>()
			.UseJsonFile(ApplicationPath.GetPath(GlobalData.ConfigPathLookupName))
			.Build();

		iConfigData.AppVersion = GlobalData.AppVersion;

		ApplicationPath.TryAddPath(GlobalData.MainTimeSliceCheckLookupName, ApplicationPathOption.CustomGlobal,
			iConfigData.SteamInstallData.SteamInstallationFolder ?? "", GlobalData.MainSliceCheckLocalPath);

		var serviceCollection = new ServiceCollection();
		serviceCollection.AddTransient(RequiredModel<HomeWindow, HomeWindowModel>);
		serviceCollection.AddTransient(RequiredModel<ApplicationInfoSubWindow, ApplicationInfoWindowModel>);
		serviceCollection.AddSingleton(RequiredModel<SettingsView, SettingsViewModel>);
		serviceCollection.AddSingleton(RequiredModel<HomeView, HomeViewModel>);
		serviceCollection.AddSingleton(RequiredModel<SteamAppView, SteamAppViewModel>);

		serviceCollection.AddSingleton<AppConfig>(provider => new AppConfig(iConfigData));
		serviceCollection.AddDbContext<DbAccess>(options => options.UseSqlite($"Data Source={ApplicationPath.GetPath(GlobalData.DbLookupName)}"), ServiceLifetime.Transient);
		serviceCollection.AddSingleton<HomeWindowModel>();
		serviceCollection.AddSingleton<ApplicationInfoWindowModel>();
		serviceCollection.AddSingleton<HomeViewModel>();
		serviceCollection.AddSingleton<SettingsViewModel>();
		serviceCollection.AddSingleton<SteamAppViewModel>();
		serviceCollection.AddSingleton<IMenuService, MenuService>(provider =>
		{
			(MenuModel Model, Window Menu) Factory(Type modelType, Type menuType, object[] @params)
			{
				var logger = provider.GetRequiredService<ILogger>();
				var model = (MenuModel)provider.GetRequiredService(modelType);
				var menu = (Window)provider.GetRequiredService(menuType);
				if(!model.IsConstructed)
				{
					model.OnConstructed();
					logger.Information("Post-Constructed MenuModel: {MenuModelType}", modelType.FullName);
				}
				model.OnLoad(@params);
				logger.Information("Loaded MenuModel: {MenuModelType}", modelType.FullName);
				return (model, menu);
			}
			return new MenuService(Factory);
		});

		serviceCollection.AddSingleton<IMessageExchangeService, ThreadedMessageExchangeService>();
		serviceCollection.AddSingleton<INavigationService, ViewModelNavigationService>();
		serviceCollection.AddSingleton<IAppService, AppService>();
		serviceCollection.AddSingleton<ICacheManager, CacheManager>();
		serviceCollection.AddSingleton<ISteamWebService, SteamWebService>();
		serviceCollection.AddSingleton<ILogger, Logger>(provider => LoggingService.Logger);
		serviceCollection.AddSingleton<IAsyncLifetimeService, ApplicationEndAsyncLifetimeService>(provider => ApplicationEndAsyncLifetimeService.Default);
		serviceCollection.AddSingleton<ILocalizationService, LocalizationService>();
		serviceCollection.AddSingleton<LocalizationManager>();
		serviceCollection.AddSingleton<ILocalSteamAppService, LocalSteamAppService>();
		serviceCollection.AddSingleton<IPlaytimeService, PlaytimeService>();

		serviceCollection.AddHttpClient(GlobalData.SteamHttpClientKey, client =>
		{
			client.BaseAddress = new Uri(GlobalData.SingleAppDetailsUrl);
		});
		serviceCollection.AddResiliencePipeline(GlobalData.SteamHttpPipelineKey, pipeline =>
		{
			pipeline
				.AddTimeout(new TimeoutStrategyOptions()
				{
					Timeout = TimeSpan.FromSeconds(10),
					OnTimeout = timeoutArgs =>
					{
						var logger = ServiceProvider.GetRequiredService<ILogger>();
						logger.Warning("HTTP request timed out after {Timeout} seconds.", $"{timeoutArgs.Timeout.TotalSeconds:n2}");
						return ValueTask.CompletedTask;
					}
				})
				.AddRetry(new RetryStrategyOptions()
				{
					ShouldHandle = retryArgs => new ValueTask<bool>(retryArgs.Outcome.Result is
						OneOf<SteamStoreAppData, ParseResult, HttpStatusCode> { IsT0: false }),
					MaxRetryAttempts = 2,
					BackoffType = DelayBackoffType.Exponential,
					Delay = TimeSpan.FromSeconds(3),
					MaxDelay = TimeSpan.FromSeconds(30)
				});
		});

		serviceCollection.AddTransient<ExportService>();

		serviceCollection.AddSingleton<Func<Type, object[], ViewModel>>(provider => (viewModelType, @params) =>
		{
			var logger = provider.GetRequiredService<ILogger>();
			var model = (ViewModel)provider.GetRequiredService(viewModelType);
			if(!model.IsConstructed)
			{
				model.OnConstructed();
				logger.Information("Post-Constructed ViewModel: {ViewModelType}", viewModelType.FullName);
			}
			model.OnLoad(@params);
			logger.Information("Loaded ViewModel: {ViewModelType}", viewModelType.FullName);
			return model;
		});

		ServiceProvider = serviceCollection.BuildServiceProvider();

		OnSessionClose += (sender, e) =>
		{
			if(e.Cancel)
			{
				return;
			}
			if(ApplicationPath.TryGetPath(GlobalData.TmpFolderName, out var tmpDirectory) && Directory.Exists(tmpDirectory))
			{
				var logger = ServiceProvider.GetRequiredService<ILogger>();
				try
				{
					Directory.Delete(tmpDirectory, true);
				}
				catch(Exception ex)
				{
					logger.Error(ex, "Failed to delete temporary directory: {TmpDirectory}", tmpDirectory);
					return;
				}
				logger.Information("Deleted temporary directory: {TmpDirectory}", tmpDirectory);
			}
		};
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		var db = ServiceProvider.GetRequiredService<DbAccess>();
		var logger = ServiceProvider.GetRequiredService<ILogger>();
		logger.Information("Applying database migrations...");
		try
		{
			db.Database.Migrate();
		}
		catch(Exception ex)
		{
			logger.Fatal(ex, "Failed to apply database migrations. Application will exit.");
			var res = MessageBox.Show("A fatal error occurred while initializing the database. The application will now exit.\nClick Yes to open logs.\n\n" +
				"Error details:\n" + ex.Message, "Fatal Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
			if(res is MessageBoxResult.Yes)
			{
				try
				{
					var logDir = LoggingService.CurrentLogFilePath;
					if(!Path.IsPathRooted(logDir))
					{
						logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logDir);
					}
					if(!Directory.Exists(logDir))
					{
						Directory.CreateDirectory(logDir);
					}
					Process.Start(new ProcessStartInfo()
					{
						FileName = logDir,
						UseShellExecute = true,
						Verb = "open"
					});
				}
				catch(Exception ex2)
				{
					logger.Error(ex2, "Failed to open log directory");
					MessageBox.Show("Failed to open log directory. See logs for more information.", "Error Opening Log Directory",
						MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
			Current.Shutdown();
			return;
		}
		logger.Information("Database migrations applied successfully.");

		var localizer = ServiceProvider.GetRequiredService<ILocalizationService>();
		var config = ServiceProvider.GetRequiredService<AppConfig>();
		localizer.ChangeLocale(config.AppData.LocalizationData.LanguageCode);

		Resources["DefaultFontFamily"] = new FontFamily(config.AppData.StyleData.CurrentFont ?? "Segoe UI");

		var menuService = ServiceProvider.GetRequiredService<IMenuService>();
		menuService.ShowMenu<HomeWindowModel, HomeWindow>(true);
		base.OnStartup(e);

		var exportService = ServiceProvider.GetRequiredService<ExportService>();
		exportService.ExportPlaytimeDataAsync(Path.Combine(ApplicationPath.GetPath(GlobalData.AppDataStoreLookupName), "Exports"),
			$"PlaytimeExport_{DateTime.Now:yyyyMMdd_HHmmss}.json").ContinueWith(task =>
		{
			if(task.IsFaulted)
			{
				logger.Error(task.Exception, "Failed to export playtime data");
			}
			else
			{
				logger.Information("Playtime data exported successfully");
			}
		});
	}

	public static void Application_Closing(object sender, CancelEventArgs e) => OnSessionClose?.Invoke(sender, e);
	private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e) => OnSessionEndingA?.Invoke(this, e);

	private static TView RequiredModel<TView, TModel>(IServiceProvider provider)
		where TView : ContentControl, new()
		where TModel : notnull => new()
	{
		DataContext = provider.GetRequiredService<TModel>()
	};
}
