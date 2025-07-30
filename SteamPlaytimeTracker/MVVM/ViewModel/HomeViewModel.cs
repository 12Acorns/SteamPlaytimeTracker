using SteamPlaytimeTracker.MVVM.View.UserControls.Steam;
using SteamPlaytimeTracker.DbObject.Conversions;
using System.Collections.ObjectModel;
using SteamPlaytimeTracker.Services;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.SelfConfig;
using OutParsing;
using System.IO;
using SteamPlaytimeTracker.DbObject;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class HomeViewModel : Core.ViewModel
{
	private readonly AppConfig _appConfig;
	private readonly DbAccess _steamDb;
	private INavigationService _navigationService;

	public HomeViewModel(INavigationService navigationService, AppConfig appConfig, DbAccess steamDb)
	{
		NavigationService = navigationService;
		_appConfig = appConfig;
		_steamDb = steamDb;
		SwitchToSettingsMenuCommand = new RelayCommand(o => NavigationService.NavigateTo<SettingsViewModel>());

		// Tmp, eventually move to own class and call that here
		_steamDb.SteamAppEntries.UpdateRange(GetLocalApps().Select(x => new SteamAppEntry()
		{
			SteamApp = x.ToDTO(),
		}));
		_steamDb.SaveChanges();

		SteamCapsuleList = new ObservableCollection<SteamCapsule>(_steamDb.SteamAppEntries.Select(x => new SteamCapsule($"https://steamcdn-a.akamaihd.net/steam/apps/{x.SteamApp.AppId}/library_600x900_2x.jpg ", x.SteamApp.FromDTO())));
		foreach(var item in SteamCapsuleList)
		{
			item.Width = 120;
			item.Height = 160;
		}
	}

	public RelayCommand SwitchToSettingsMenuCommand { get; set; }
	public INavigationService NavigationService
	{
		get => _navigationService;
		set
		{
			_navigationService = value;
			OnPropertyChanged();
		}
	}

	public ObservableCollection<SteamCapsule> SteamCapsuleList { get; set; }

	private IEnumerable<SteamApp> GetLocalApps()
	{
		var steamPath = _appConfig.AppData.SteamInstallationFolder;
		var logPath = Path.Combine(steamPath, "logs");
		var primarySearchFile = Path.Combine(logPath, "gameprocess_log.txt");
		if(!File.Exists(primarySearchFile))
		{
			// In Future add fallback to alternate files and process them
			// Another method like GetOnlineApps will ask steam for the the apps a user owns
			// and gets the info steam provide like total playtime to present the most basic info
			return [];
		}
		var apps = new HashSet<SteamApp>();
		var lookup = _steamDb.AllSteamApps.ToArray().DistinctBy(x => x.AppId).ToDictionary(x => x.AppId);
		foreach(var line in File.ReadAllLines(primarySearchFile))
		{
			if(!OutParser.TryParse(line, "[{date}] AppID {appId} adding PID {pidId} as a tracked process {appPath}",
				out string date, out int appId, out int pidId, out string appPath))
			{
				continue;
			}
			var appName = "n/a";
			if(lookup.TryGetValue(appId, out var dto))
			{
				appName = dto.AppName;
			}
			apps.Add(new SteamApp(appId, appName));
		}
		return apps;
	}
}