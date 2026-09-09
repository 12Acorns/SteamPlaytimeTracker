using AppServices.Common.Client;
using AppServices.Updater;
using Config.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OneOf;
using Polly;
using Polly.Retry;
using Polly.Timeout;
using Serilog;
using Serilog.Core;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.IO;
using SteamPlaytimeTracker.Localization;
using SteamPlaytimeTracker.MVVM.View;
using SteamPlaytimeTracker.MVVM.View.Windows;
using SteamPlaytimeTracker.MVVM.ViewModel;
using SteamPlaytimeTracker.MVVM.ViewModel.Window;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.SelfConfig.Data;
using SteamPlaytimeTracker.Services._App;
using SteamPlaytimeTracker.Services.Batching;
using SteamPlaytimeTracker.Services.DataTransfer;
using SteamPlaytimeTracker.Services.Disk;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Services.Localization;
using SteamPlaytimeTracker.Services.Menu;
using SteamPlaytimeTracker.Services.Messaging;
using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.Services.Playtime;
using SteamPlaytimeTracker.Services.Process;
using SteamPlaytimeTracker.Services.Web.Steam;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.Utility;
using SteamPlaytimeTracker.Utility.Cache;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SteamPlaytimeTracker;

public partial class App : Application
{
	internal static OrderedEventInvoker<CancelEventArgs>? OnSessionClose => field ??= new();
	internal static OrderedEventInvoker<SessionEndingCancelEventArgs>? OnSessionEndingA => field ??= new();

	public static ServiceProvider ServiceProvider { get; private set; } = default!;

	public App()
	{
		InitializeComponent();

		FrameworkElement.StyleProperty.OverrideMetadata(typeof(Window), new FrameworkPropertyMetadata
		{
			DefaultValue = FindResource(typeof(Window))
		});

		ApplicationPath.TryAddPath(GlobalData.LocalizationLookupName, ApplicationPathOption.FileLocation, "locale");
		ApplicationPath.TryAddPath(GlobalData.AppDataStoreLookupName, "Steam Playtime Tracker");
		ApplicationPath.TryAddPath(GlobalData.ConfigPathLookupName, "Steam Playtime Tracker", "settings.json");
		ApplicationPath.TryAddPath(GlobalData.DbLookupName, "Steam Playtime Tracker", "appdata.db");
		ApplicationPath.TryAddPath(GlobalData.TmpFolderName, Directory.CreateTempSubdirectory("Steam Playtime Tracker").FullName, ApplicationPathOption.CustomGlobal);

		var configData = new ConfigurationBuilder<IAppData>()
			.UseJsonFile(ApplicationPath.GetPath(GlobalData.ConfigPathLookupName))
			.Build();

		configData.AppVersion = GlobalData.AppVersion.ToString();

		ApplicationPath.TryAddPath(GlobalData.MainTimeSliceCheckLookupName, ApplicationPathOption.CustomGlobal,
			configData.SteamInstallData.SteamInstallationFolder ?? "", GlobalData.MainSliceCheckLocalPath);

		var serviceCollection = new ServiceCollection();
		serviceCollection.AddSingleton(RequiredModel<HomeWindow, HomeWindowModel>);
		serviceCollection.AddTransient(RequiredModel<ApplicationInfoSubWindow, ApplicationInfoWindowModel>);
		serviceCollection.AddTransient(RequiredModel<ProcessTrackingSelectionWindow, ProcessTrackingSelectionWindowModel>);
		serviceCollection.AddSingleton(RequiredModel<SettingsView, SettingsViewModel>);
		serviceCollection.AddSingleton(RequiredModel<HomeView, HomeViewModel>);
		serviceCollection.AddSingleton(RequiredModel<SteamAppView, SteamAppViewModel>);

		serviceCollection.AddSingleton<AppConfig>(provider => new AppConfig(configData));
		serviceCollection.AddDbContext<DbAccess>(options => options.UseSqlite($"Data Source={ApplicationPath.GetPath(GlobalData.DbLookupName)}"), ServiceLifetime.Transient);
		serviceCollection.AddSingleton<HomeWindowModel>();
		serviceCollection.AddSingleton<ApplicationInfoWindowModel>();
		serviceCollection.AddSingleton<ProcessTrackingSelectionWindowModel>();
		serviceCollection.AddSingleton<HomeViewModel>();
		serviceCollection.AddSingleton<SettingsViewModel>();
		serviceCollection.AddSingleton<SteamAppViewModel>();
		serviceCollection.AddSingleton<IMenuService, MenuService>();

		serviceCollection.AddSingleton<IProcessListingService, ProcessListingService>();
		serviceCollection.AddSingleton<IMessageExchangeService, ThreadedMessageExchangeService>();
		serviceCollection.AddSingleton<INavigationService, ViewModelNavigationService>();
		serviceCollection.AddSingleton<IAppService, AppService>();
		serviceCollection.AddSingleton<ICacheManager, CacheManager>();
		serviceCollection.AddSingleton<ISteamWebService, SteamWebService>();
		serviceCollection.AddSingleton<ILogger, Logger>(provider => LoggingService.Logger);
		serviceCollection.AddSingleton<ILifetimeService, ApplicationEndAsyncLifetimeService>(provider => ApplicationEndAsyncLifetimeService.Default);
		serviceCollection.AddSingleton<ILocalizationService, LocalizationService>();
		serviceCollection.AddSingleton<LocalizationManager>();
		serviceCollection.AddSingleton<ILocalSteamAppService, LocalSteamAppService>();
		serviceCollection.AddSingleton<IPlaytimeService, PlaytimeService>();
		serviceCollection.AddSingleton<IAppSynchronisationService, AppSynchronisationService>();
		serviceCollection.AddSingleton<IBatchUpdateService<SteamAppEntry>, BatchUpdateService<SteamAppEntry>>();
		serviceCollection.AddSingleton<BatchOptions>(provider => new BatchOptions
		{
			MaximumBatchSize = 16,
			MaximumRetries = 3,
			MinimumWaitInterval = TimeSpan.FromMilliseconds(500),
			MaximumWaitInterval = TimeSpan.FromSeconds(30),
			BaseGrowthFactor = 1.5f
		});

		serviceCollection.AddSingleton<MenuService.ModelFactory>(provider => (modelType, menuType, @params) =>
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
		});

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

		// tmp remedie, do not remove
		// Causes playtime to be calculated/retrieved/scraped from disk during startup
		// Makes percieved loading time quicker as we are doing the computation earlier rather than later
		// Need to optimise in future
		_ = ServiceProvider.GetRequiredService<IPlaytimeService>().GetPlayimeIntervalsMap(
			ServiceProvider.GetRequiredService<ILifetimeService>().CancellationToken);

		var logger = ServiceProvider.GetRequiredService<ILogger>();
		OnSessionClose.Subscribe(OrderedEventPriority.Default, (sender, e) =>
		{
			if(e.Cancel)
			{
				return;
			}
			if(ApplicationPath.TryGetPath(GlobalData.TmpFolderName, out var tmpDirectory) && Directory.Exists(tmpDirectory))
			{
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
		});
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		var config = ServiceProvider.GetRequiredService<AppConfig>();

		if(LikelyNeedsUpdating(config.AppData.AppVersion))
		{
			var res = MessageBox.Show("A new version of Steam Playtime Tracker is available. Would you like to update now?", "Update Available",
				MessageBoxButton.YesNo, MessageBoxImage.Information);
			if(res is MessageBoxResult.Yes)
			{
				Update();
			}
		}

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
		localizer.ChangeLocale(config.AppData.LocalizationData.LanguageCode);

		Resources["DefaultFontFamily"] = new FontFamily(config.AppData.StyleData.CurrentFont ?? "Segoe UI");

		var menuService = ServiceProvider.GetRequiredService<IMenuService>();
		var navigationService = ServiceProvider.GetRequiredService<INavigationService>();
		navigationService.OnNavigatedTo.Subscribe(OrderedEventPriority.Default, (sender, e) =>
		{
			if(menuService.Menus.FirstOrDefault(x => x.Model is HomeWindowModel).Model is not HomeWindowModel homeMenuModel)
			{
				return;
			}
			homeMenuModel.SettingsButtonVisibility = e.ViewModel switch
			{
				SettingsViewModel => Visibility.Hidden,
				_ => Visibility.Visible,
			};
		});

		menuService.ShowMenu<HomeWindowModel, HomeWindow>(true);
	}

	public static void Application_Closing(object sender, CancelEventArgs e) => OnSessionClose?.Invoke(sender, e);
	private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e) => OnSessionEndingA?.Invoke(this, e);

	private static TView RequiredModel<TView, TModel>(IServiceProvider provider)
		where TView : ContentControl, new()
		where TModel : notnull => new()
	{
		DataContext = provider.GetRequiredService<TModel>()
	};
	[DoesNotReturn]
	private static void Update()
	{
		Process.Start(new ProcessStartInfo()
		{
			FileName = "Updater.exe",
			UseShellExecute = true,
			Arguments = $"--app-path \"{AppDomain.CurrentDomain.BaseDirectory}\""
		});
		Environment.Exit(0);
	}
	private static bool LikelyNeedsUpdating(string configVersionStr)
	{
		var assemblyVersion = GlobalData.AppVersion;
		var updateHelper = new UpdateHelper(new GitHubClient(GlobalData.Program.GitHubRepoOwner, GlobalData.Program.GitHubRepoName));
		var newReleaseAvailableTask = updateHelper.NewReleaseAvailableAsync(assemblyVersion);
		var (newRelease, _) = newReleaseAvailableTask.Result(TimeSpan.FromSeconds(10));
		return newRelease;
	}
}
