using Microsoft.EntityFrameworkCore;
using OutParsing;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.DbObject.Conversions;
using SteamPlaytimeTracker.IO;
using SteamPlaytimeTracker.MVVM.View.UserControls.Steam;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.Services;
using SteamPlaytimeTracker.Steam;
using SteamPlaytimeTracker.Steam.Data.App;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class HomeViewModel : Core.ViewModel
{
	private static readonly string _tmpStorePath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
		"Steam Playtime Tracker",
		"tmp.txt");

	private readonly AppConfig _appConfig;
	private readonly DbAccess _steamDb;
	private INavigationService _navigationService;

	public HomeViewModel(INavigationService navigationService, AppConfig appConfig, DbAccess steamDb)
	{
		NavigationService = navigationService;
		_appConfig = appConfig;
		_steamDb = steamDb;
		SwitchToSettingsMenuCommand = new RelayCommand(o => NavigationService.NavigateTo<SettingsViewModel>());

		if(!_steamDb.AllSteamApps.Any())
		{
			var res = SteamRequest.GetAppListAsync().GetAwaiter().GetResult();
			res.Switch(response =>
			{
				_steamDb.AllSteamApps.UpdateRange(response.Apps.SteamApps.Select(x => x.ToDTO()));
				_steamDb.SaveChanges();
				_appConfig.AppData.LastCheckedSteamApps = Stopwatch.GetTimestamp();
			}, (_) => { }, (_) => { });

			_steamDb.AddRange();
		}

		var entriesAppId = _steamDb.SteamAppEntries.Select(x => x.SteamApp.AppId).ToHashSet();

		var localApps = GetLocalApps();
		var notFoundInEntriesApps = localApps.Where(x => !entriesAppId.Contains(x.Id)).ToList();

		if(notFoundInEntriesApps.Count > 0)
		{
			// Tmp, eventually move to own class and call that here
			_steamDb.SteamAppEntries.AddRange(notFoundInEntriesApps.Select(x => new SteamAppEntry()
			{
				SteamApp = x.ToDTO(),
				PlaytimeSegments = GetSegments(x),
			}));
			_steamDb.SaveChanges();
		}

		SteamCapsuleList = new ObservableCollection<SteamCapsule>(_steamDb.SteamAppEntries.Select(x => 
			new SteamCapsule($"https://steamcdn-a.akamaihd.net/steam/apps/{x.SteamApp.AppId}/library_600x900_2x.jpg", x.SteamApp.FromDTO())));
		foreach(var item in SteamCapsuleList)
		{
			item.Width = 120;
			item.Height = 180;
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

	private static List<PlaytimeSliceDTO> GetSegments(SteamApp app)
	{
		var primarySearchFile = ApplicationPath.GetPath("MainTimeSliceCheck");
		if(!File.Exists(primarySearchFile))
		{
			return [];
		}
		if(File.Exists(_tmpStorePath))
		{
			File.Delete(_tmpStorePath);
		}
		File.Copy(primarySearchFile, _tmpStorePath);

		var playSegmentStart = new Queue<(string Date, uint AppId)>(capacity: 16);
		var segments = new List<PlaytimeSliceDTO>();
		foreach(var line in File.ReadAllLines(_tmpStorePath))
		{
			if(OutParser.TryParse(line, "[{startDate}] AppID {appIdS} adding PID {pidIdS} as a tracked process {appPath}", 
				out string startDate, out uint appIdS, out int pidIdS, out string appPath) && appIdS == app.Id)
			{
				playSegmentStart.Enqueue((startDate, appIdS));
				continue;
			}
			if(OutParser.TryParse(line, "[{endDate}] AppID {appId} no longer tracking PID {pidId}, exit code {exitCode}",
				out string endDate, out uint appId, out int pidId, out int exitCode) && appId == app.Id)
			{
				if(!playSegmentStart.TryDequeue(out (string, uint) dequeue))
				{
					continue;
				}
				var startDateString = dequeue.Item1;
				var startDateOffset = DateTimeOffset.ParseExact(
					startDateString,
					"yyyy-MM-dd HH:mm:ss",
					CultureInfo.InvariantCulture,
					DateTimeStyles.AssumeLocal);
				var endDateOffset = DateTimeOffset.ParseExact(
					endDate,
					"yyyy-MM-dd HH:mm:ss",
					CultureInfo.InvariantCulture,
					DateTimeStyles.AssumeLocal);
				var dateDelta = endDateOffset - startDateOffset;
				segments.Add(new PlaytimeSliceDTO(startDateOffset, dateDelta, appId));
			}
		}
		File.Delete(_tmpStorePath);
		return segments;
	}
	private IEnumerable<SteamApp> GetLocalApps()
	{
		var primarySearchFile = ApplicationPath.GetPath("MainTimeSliceCheck");
		if(!File.Exists(primarySearchFile))
		{
			// In Future add fallback to alternate files and process them
			// Another method like GetOnlineApps will ask steam for the the apps a user owns
			// and gets the info steam provide like total playtime to present the most basic info
			return [];
		}
		if(File.Exists(_tmpStorePath))
		{
			File.Delete(_tmpStorePath);
		}
		File.Copy(primarySearchFile, _tmpStorePath);

		var apps = new HashSet<SteamApp>();
		var lookup = _steamDb.AllSteamApps.ToArray().DistinctBy(x => x.AppId).ToDictionary(x => x.AppId);
		foreach(var line in File.ReadAllLines(_tmpStorePath))
		{
			if(!OutParser.TryParse(line, "[{date}] AppID {appId} adding PID {pidId} as a tracked process {appPath}",
				out string date, out uint appId, out int pidId, out string appPath))
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
		File.Delete(_tmpStorePath);
		return apps;
	}
}