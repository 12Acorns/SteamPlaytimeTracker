using Microsoft.EntityFrameworkCore;
using Serilog;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.DbObject.Conversions;
using SteamPlaytimeTracker.IO;
using SteamPlaytimeTracker.MVVM.View;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.SelfConfig.Data;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.Steam;
using System.Diagnostics;
using System.IO;
using System.Windows;

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

				_config.AppData.UseExperimentalAppFetch = settingsView.ts_UseExperimentalFetch.tgl_Button.IsChecked!.Value;

				_logger.Information("Successfully saved AppData", _config.AppData);

				_navigationService!.NavigateTo<HomeViewModel>();
			}
		}, _ => !_dbBeingUpdated);
		QuerySteamGamesCommand = new RelayCommand(async o =>
		{
			if(_dbBeingUpdated)
			{
				MessageBox.Show("Please wait until the database is updated before querying Steam games.", "Database Update in Progress",
					MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			_dbBeingUpdated = true;
			var res = await SteamRequest.GetAppListAsync(_lifetimeService.CancellationToken).ConfigureAwait(false);
			res.Switch(async response =>
			{
				try
				{
					var entries = await _steamDb.SteamApps.ToHashSetAsync(_lifetimeService.CancellationToken).ConfigureAwait(false);
					await _steamDb.SteamApps.AddRangeAsync(response.Apps.SteamApps.Where(x => !entries.Contains(x))).ConfigureAwait(false);
					await _steamDb.SaveChangesAsync(_lifetimeService.CancellationToken).ConfigureAwait(false);
					_config.AppData.SteamInstallData.LastCheckedSteamApps = Stopwatch.GetTimestamp();
				}
				catch(Exception ex)
				{
					_logger.Error(ex, "Error while updating Steam App List");
					MessageBox.Show("An error occurred while updating the Steam App List. Please check the logs for more details.", "Error",
						MessageBoxButton.OK, MessageBoxImage.Error);
				}
				finally
				{
					_dbBeingUpdated = false;
				}
			}, (_) => { }, (_) => { });
		}, _ => !_dbBeingUpdated && !_config.AppData.UseExperimentalAppFetch);
	}

	public RelayCommand QuerySteamGamesCommand { get; set; }
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