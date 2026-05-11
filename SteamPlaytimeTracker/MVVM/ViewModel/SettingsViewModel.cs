using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.IO;
using SteamPlaytimeTracker.Localization;
using SteamPlaytimeTracker.Localization.Data;
using SteamPlaytimeTracker.MVVM.View;
using SteamPlaytimeTracker.MVVM.View.UserControls.Settings;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.Services.DataTransfer;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Services.Localization;
using SteamPlaytimeTracker.Services.Navigation;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class SettingsViewModel : Core.ViewModel
{
	private readonly FontFamily[] _availableFonts;
	private readonly ExportService _exportService;
	private readonly AppConfig _config;
	private readonly ILogger _logger;
	
	public SettingsViewModel(INavigationService navigationService, ILogger logger, AppConfig config, ILocalizationService localizationService,
		ExportService exportService, LocalizationManager localizationManager, ILifetimeService lifetimeService)
	{
		NavigationService = navigationService;
		_logger = logger;
		_config = config;

		LocalizationService = localizationService;
		_exportService = exportService;
		SteamInstallPath = _config.AppData.SteamInstallData.SteamInstallationFolder ?? string.Empty;
		AvailableLogLevels = Enum.GetNames<LogEventLevel>();
		SelectedLogLevel = _config.AppData.LoggingData.LogLevel;

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

				NavigationService.NavigateTo<HomeViewModel>();
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
				_logger.Error(ex, "Failed to open log directory");
				MessageBox.Show("Failed to open log directory. See logs for more information.", "Error Opening Log Directory",
					MessageBoxButton.OK, MessageBoxImage.Error);
			}
		});
		ExportDataCommand = new(_ =>
		{
			var path = Path.Combine(ApplicationPath.GetPath(GlobalData.AppDataStoreLookupName), "Exports");
			var name = $"PlaytimeExport_{DateTime.Now:yyyyMMdd_HHmmss_fff}.json";
			var playtimeFullPath = Path.Combine(path, name);
			_exportService.ExportAllPlaytimeDataAsync(path, name, lifetimeService.CancellationToken).ContinueWith(task =>
			{
				if(task.IsFaulted)
				{
					logger.Error(task.Exception, "Failed to export playtime data");
					Dispatcher.Invoke(() =>
					{
						MessageBox.Show("An error occurred while exporting playtime data. See logs for more information.", "Error Exporting Data",
							MessageBoxButton.OK, MessageBoxImage.Error);
					});
					return;
				}
				logger.Information("Playtime data exported successfully");
				Dispatcher.Invoke(() =>
				{
					var res = MessageBox.Show(
						messageBoxText: $"Playtime data exported successfully. Path: '{playtimeFullPath}'.\nPress Yes to copy path to clipboard.", 
						caption: "Export Successful", button: MessageBoxButton.YesNo, icon: MessageBoxImage.Information);
					if(res is MessageBoxResult.Yes)
					{
						Clipboard.SetText(playtimeFullPath);
					}
				});
			});
		});

		AvailableLocales = localizationManager.GetAvailableLocales().ToArray();

		_availableFonts = Fonts.SystemFontFamilies.OrderBy(f => f.Source).ToArray();
		AvailableFontsView = (ListCollectionView)CollectionViewSource.GetDefaultView(_availableFonts);
		SelectedFont = new FontFamily(_config.AppData.StyleData.CurrentFont ?? "Segoe UI");

		TextChangedFiltering = new(data =>
		{
			if(data is not List<object> parameters || parameters is not [SearchBarFilteredDropDownUC searchBar, string text, ..])
			{
				return;
			}
			searchBar.SearchContent.IsDropDownOpen = true;
			AvailableFontsView.Filter = item => item is FontFamily fontFamily && fontFamily.Source.Contains(text);
		});
		KeyInputCommand = new(data =>
		{
			if(data is not List<object> parameters || parameters is not [SearchBarFilteredDropDownUC searchBar, KeyEventArgs keyArgs])
			{
				return;
			}
			switch(keyArgs.Key)
			{
				case Key.Return or Key.Enter:
					if(searchBar.SearchContent.SelectedIndex == -1)
					{
						if(AvailableFontsView.Count == 0)
						{
							return;
						}
						searchBar.SearchContent.SelectedIndex = 0;
					}
					if(searchBar.SearchContent.SelectedItem is FontFamily selectedFont)
					{
						SelectedFont = selectedFont;
						searchBar.SearchContent.IsDropDownOpen = false;
						searchBar.SearchContent.RemoveFocus();
					}
					searchBar.SearchContent.SelectedIndex = -1;
					break;
				case Key.Escape:
					searchBar.SearchContent.IsDropDownOpen = false;
					searchBar.SearchContent.RemoveFocus();
					break;
			}
		});
	}

	public RelayCommand ExportDataCommand { get; set; }
	public RelayCommand OpenLogDirCommand { get; set; }
	public RelayCommand ConfirmSettingsCommand { get; set; }
	public ILocalizationService LocalizationService { get; }
	public INavigationService NavigationService { get; }
	public string SteamInstallPath
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	} = string.Empty;
	public string[] AvailableLogLevels { get; private set; }
	public LogEventLevel SelectedLogLevel
	{
		get;
		set
		{
			field = value;
			LoggingService.LoggingLevelSwitcher.MinimumLevel = field;
			_config.AppData.LoggingData.LogLevel = field;
			OnPropertyChanged();
		}
	}
	public LocaleData[] AvailableLocales
	{
		get;
		private set
		{
			field = value;
			OnPropertyChanged();
		}
	} = [];
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
	public ListCollectionView AvailableFontsView
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	} = default!;
	public FontFamily SelectedFont
	{
		get;
		set
		{
			// temp fix, i do not know why this is getting called during a view change
			if(value is null)
			{
				return;
			}
			field = value;
			AddOrUpdateResource("DefaultFontFamily", field);
			_config.AppData.StyleData.CurrentFont = field.Source;
			OnPropertyChanged();
		}
	}
	public RelayCommand TextChangedFiltering { get; }
	public RelayCommand KeyInputCommand { get; }

	private bool VerifySettings(string path, bool showMsgBox = false)
	{
		try
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
		catch(Exception e)
		{
			_logger.Error(e, "Error verifying settings for Steam installation path: {Path}", path);
			if(showMsgBox)
			{
				MessageBox.Show("An error occurred while verifying the entered Steam installation directory. See logs for more information.",
					"Error Verifying Settings!", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			return false;
		}
	}
	private string GetPathIfExistsElseEmpty(string? directory)
	{
		if(string.IsNullOrEmpty(directory))
		{
			return string.Empty;
		}
		try
		{
			if(!Directory.Exists(directory))
			{
				return string.Empty;
			}
			return directory;
		}
		catch(Exception e)
		{
			_logger.Error(e, "Error checking directory existence: {Directory}", directory);
			return string.Empty;
		}
	}
}