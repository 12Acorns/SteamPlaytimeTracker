using Microsoft.EntityFrameworkCore;
using Serilog;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.DbObject.Conversions;
using SteamPlaytimeTracker.IO;
using SteamPlaytimeTracker.MVVM.View;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.Services;
using SteamPlaytimeTracker.Steam;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class SettingsViewModel : Core.ViewModel
{
	private readonly AppConfig _config;
	private readonly DbAccess _steamDb;
	private readonly ILogger _logger;
	private INavigationService _navigationService;

	private string _steamInstallPath = string.Empty;
	private bool? _autoRefreshSteamApps = true;

	public SettingsViewModel(INavigationService navigationService, ILogger logger, AppConfig config, DbAccess steamDb)
	{
		NavigationService = navigationService;
		_logger = logger;
		_config = config;
		_steamDb = steamDb;
		AutoRefreshSteamApps = _config.AppData.CheckForSteamAppsPeriodically;
		SteamInstallPath = _config.AppData.SteamInstallationFolder;

		ConfirmSettingsCommand = new RelayCommand(o =>
		{
			var settingsView = (SettingsView)o!;
			if(VerifySettings(settingsView.fsv_SteamInstall.tf_AppInstall.Text))
			{
				_navigationService!.NavigateTo<HomeViewModel>();
				_config.AppData.SteamInstallationFolder = settingsView.fsv_SteamInstall.tf_AppInstall.Text;
				_config.AppData.CheckForSteamAppsPeriodically = settingsView.tgl_AutoFetchSteamApps.IsChecked!.Value;

				ApplicationPath.AddOrUpdatePath(GlobalData.MainTimeSliceCheckLookupName, GlobalData.MainSliceCheckLocalPath);

				_logger.Information("Successfully saved AppData", _config.AppData);
			}
		});
		QuerySteamGamesCommand = new RelayCommand(async o =>
		{
			var res = await SteamRequest.GetAppListAsync().ConfigureAwait(false);
			res.Switch(async response =>
			{
				var entries = await _steamDb.AllSteamApps.ToHashSetAsync().ConfigureAwait(false);
				_steamDb.AllSteamApps.UpdateRange(response.Apps.SteamApps.Select(x => x.ToDTO()).Where(x => !entries.Contains(x)));
				await _steamDb.SaveChangesAsync().ConfigureAwait(false);
				_config.AppData.LastCheckedSteamApps = Stopwatch.GetTimestamp();
			}, (_) => { }, (_) => { });
		});	
	}

	public RelayCommand QuerySteamGamesCommand { get; set; }
	public RelayCommand ConfirmSettingsCommand { get; set; }

	public INavigationService NavigationService
	{
		get => _navigationService;
		set
		{
			_navigationService = value;
			OnPropertyChanged();
		}
	}
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