using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.Localization.Data;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Localization;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.MVVM.View;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.IO;
using System.Diagnostics;
using System.Windows;
using System.IO;
using Serilog;
using SteamPlaytimeTracker.Services.Localization;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class SettingsViewModel : Core.ViewModel
{
	private readonly INavigationService _navigationService;
	private readonly AppConfig _config;
	private readonly ILogger _logger;

	public SettingsViewModel(INavigationService navigationService, ILogger logger, AppConfig config, ILocalizationService localizationService)
	{
		_navigationService = navigationService;
		_logger = logger;
		_config = config;
		LocalizationService = localizationService;
		SteamInstallPath = _config.AppData.SteamInstallData.SteamInstallationFolder ?? string.Empty;

		ConfirmSettingsCommand = new RelayCommand(o =>
		{
			var settingsView = (SettingsView)o!;
			if(VerifySettings(settingsView.fsv_SteamInstall.tf_AppInstall.Text))
			{
				_config.AppData.SteamInstallData.SteamInstallationFolder = settingsView.fsv_SteamInstall.tf_AppInstall.Text;

				ApplicationPath.AddOrUpdatePath(GlobalData.MainTimeSliceCheckLookupName, 
					Path.Combine(_config.AppData.SteamInstallData.SteamInstallationFolder, GlobalData.MainSliceCheckLocalPath), 
					ApplicationPathOption.CustomGlobal);

				_logger.Information("Successfully saved AppData", _config.AppData);

				_navigationService!.NavigateTo<HomeViewModel>();
			}
		});
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
		});
		AvailableLocales = LocalizationManager.GetAvailableLocales().ToArray();
	}

	public RelayCommand OpenLogDirCommand { get; set; }
	public RelayCommand ConfirmSettingsCommand { get; set; }
	public ILocalizationService LocalizationService { get; }

	public INavigationService NavigationService => _navigationService;
	public string SteamInstallPath
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	} = string.Empty;
	public LocaleData[] AvailableLocales { get; private set; } = [];
	public LocaleData CurrentLocale
	{
		get
		{
			var match = AvailableLocales.FirstOrDefault(x => x.Code.Equals(_config.AppData.LocalizationData.LanguageCode, StringComparison.OrdinalIgnoreCase));
			return match ?? AvailableLocales.First(x => x.Code.Equals("en-gb", StringComparison.OrdinalIgnoreCase));
		}
		set
		{
			_config.AppData.LocalizationData.LanguageCode = value.Code;
			LocalizationService.ChangeLocale(value);
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