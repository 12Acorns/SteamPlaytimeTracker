using Serilog;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.IO;
using SteamPlaytimeTracker.MVVM.View;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Services.Navigation;
using System.Diagnostics;
using System.Windows;
using System.IO;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class SettingsViewModel : Core.ViewModel
{
	private readonly IAsyncLifetimeService _lifetimeService;
	private readonly INavigationService _navigationService;
	private readonly AppConfig _config;
	private readonly DbAccess _steamDb;
	private readonly ILogger _logger;

	private string _steamInstallPath = string.Empty;
	private bool? _autoRefreshSteamApps = true;

	private bool _dbBeingUpdated = false;

	public SettingsViewModel(INavigationService navigationService, IAsyncLifetimeService lifetimeService, ILogger logger, AppConfig config, DbAccess steamDb)
	{
		_navigationService = navigationService;
		_lifetimeService = lifetimeService;
		_logger = logger;
		_config = config;
		_steamDb = steamDb;
		AutoRefreshSteamApps = _config.AppData.SteamInstallData.CheckForSteamAppsPeriodically;
		SteamInstallPath = _config.AppData.SteamInstallData.SteamInstallationFolder ?? string.Empty;

		ConfirmSettingsCommand = new RelayCommand(o =>
		{
			if(_dbBeingUpdated)
			{
				MessageBox.Show("Please wait until the database is updated before confirming settings.", "Database Update in Progress",
					MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			var settingsView = (SettingsView)o!;
			if(VerifySettings(settingsView.fsv_SteamInstall.tf_AppInstall.Text))
			{
				_config.AppData.SteamInstallData.SteamInstallationFolder = settingsView.fsv_SteamInstall.tf_AppInstall.Text;
				_config.AppData.SteamInstallData.CheckForSteamAppsPeriodically = settingsView.tgl_AutoFetchSteamApps.IsChecked!.Value;

				ApplicationPath.AddOrUpdatePath(GlobalData.MainTimeSliceCheckLookupName, 
					Path.Combine(_config.AppData.SteamInstallData.SteamInstallationFolder, GlobalData.MainSliceCheckLocalPath), 
					ApplicationPathOption.CustomGlobal);

				_logger.Information("Successfully saved AppData", _config.AppData);

				_navigationService!.NavigateTo<HomeViewModel>();
			}
		}, _ => !_dbBeingUpdated);
		OpenLogDirCommand = new RelayCommand(o =>
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
			catch(Exception ex)
			{
				_logger.Error("Failed to open log directory", ex);
				MessageBox.Show("Failed to open log directory. See logs for more information.", "Error Opening Log Directory",
					MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}, _ => true);
	}

	public RelayCommand OpenLogDirCommand { get; set; }
	public RelayCommand ConfirmSettingsCommand { get; set; }

	public INavigationService NavigationService => _navigationService;
	public bool? AutoRefreshSteamApps
	{
		get => _autoRefreshSteamApps;
		set
		{
			_autoRefreshSteamApps = value;
			OnPropertyChanged();
		}
	}
	public string SteamInstallPath
	{
		get => _steamInstallPath;
		set
		{
			_steamInstallPath = value;
			OnPropertyChanged();
		}
	}

	private static bool VerifySettings(string path, bool showMsgBox = false)
	{
		if(!Directory.Exists(path))
		{
			if(showMsgBox)
			{
				MessageBox.Show("Steam directory not found. Please enter your steam installation directory.", "Invalid Location Set!",
					MessageBoxButton.OK, MessageBoxImage.Warning);
			}
			return false;
		}
		if(!File.Exists(Path.Combine(path, "steam.exe")))
		{
			if(showMsgBox)
			{
				MessageBox.Show("Steam executable not found under entered directory. Ensure the correct steam installation directory has been " +
					"entered", "No Steam Executable Found!", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
			return false;
		}
		if(showMsgBox)
		{
			MessageBox.Show("Successfully saved settings.", "Success :D", MessageBoxButton.OK, MessageBoxImage.Information);
		}
		return true;
	}
}