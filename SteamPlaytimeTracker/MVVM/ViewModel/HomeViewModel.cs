using Microsoft.EntityFrameworkCore;
using OutParsing;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.DbObject.Conversions;
using SteamPlaytimeTracker.IO;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.Services;
using SteamPlaytimeTracker.Steam;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.Steam.Data.Playtime;
using SteamPlaytimeTracker.Utility;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class HomeViewModel : Core.ViewModel
{
	private static readonly Lock _lock = new();

	private static readonly string _tmpStorePath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
		"Steam Playtime Tracker",
		"tmp.txt");

	private readonly IAsyncLifetimeService _lifetimeProvider;
	private readonly INavigationService _navigationService;
	private readonly AppConfig _appConfig;
	private readonly DbAccess _steamDb;
	private ObservableCollection<SteamApp> _steamCapsules = [];

	public HomeViewModel(INavigationService navigationService, IAsyncLifetimeService lifetimeProvider, AppConfig appConfig, DbAccess steamDb)
	{
		_navigationService = navigationService;
		_lifetimeProvider = lifetimeProvider;
		_appConfig = appConfig;
		_steamDb = steamDb;
		SwitchToSettingsMenuCommand = new RelayCommand(o => NavigationService.NavigateTo<SettingsViewModel>());
		SteamApps = [];
	}

	public RelayCommand NavigateToPlayTimeViewCommand => new(o =>
	{
		NavigationService.NavigateTo<SteamAppViewModel>();
		((SteamAppViewModel)NavigationService.CurrentView).SelectedApp = (SteamApp)o!;
	}, o => o is SteamApp);
	public RelayCommand SwitchToSettingsMenuCommand { get; set; }
	public INavigationService NavigationService => _navigationService;

	public ObservableCollection<SteamApp> SteamApps
	{
		get => _steamCapsules;
		set
		{
			_steamCapsules = value;
			OnPropertyChanged();
		}
	}

	public override async void OnConstructed()
	{
		if(!await _steamDb.AllSteamApps.AnyAsync().ConfigureAwait(false))
		{
			var res = await SteamRequest.GetAppListAsync(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
			if(res.IsT0)
			{
				var response = res.AsT0;
				await _steamDb.AllSteamApps.AddRangeAsync(response.Apps.SteamApps.Select(x => x.ToDTO()), _lifetimeProvider.CancellationToken).ConfigureAwait(false);
				await _steamDb.SaveChangesAsync(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
				_appConfig.AppData.LastCheckedSteamApps = Stopwatch.GetTimestamp();
			}
		}
		await AppendLocalApps().ConfigureAwait(false);
		SteamApps = new(await _steamDb.SteamAppEntries.Select(x => CreateSteamApp(x)).ToListAsync().ConfigureAwait(false));
		base.OnConstructed();
	}
	private static SteamApp CreateSteamApp(SteamAppEntry app) => app.SteamApp.FromDTO();
	private async Task AppendLocalApps()
	{
		// Force loading of all steam apps, prevents null ref during entiresAppid due to the referenced Apps not being loaded (hence null, idk a better way to fix)
		await _steamDb.AllSteamApps.LoadAsync(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
		var localApps = await GetLocalApps(_lifetimeProvider.CancellationToken).ToListAsync().ConfigureAwait(false);
		var entriesAppId = (await _steamDb.SteamAppEntries.ToHashSetAsync(AlternateAppLookup.Instance)).GetAlternateLookup<SteamApp>();
		var notFoundInEntriesApps = localApps.Where(x => !entriesAppId.Contains(x));
		if(notFoundInEntriesApps.Any())
		{
			// Tmp, eventually move to own class and call that here
			await _steamDb.SteamAppEntries.AddRangeAsync(notFoundInEntriesApps.Select(x => new SteamAppEntry()
			{
				SteamApp = x.ToDTO(),
				PlaytimeSegments = GetSegments(x),
			}), _lifetimeProvider.CancellationToken).ConfigureAwait(false);
			await _steamDb.SaveChangesAsync(_lifetimeProvider.CancellationToken).ConfigureAwait(true);
		}
		var segments = new Dictionary<uint, List<PlaytimeSliceDTO>>();
		var segmentsToUpdate = entriesAppId.Set.AsEnumerable().Where(x =>
		{
			var segment = GetSegments(x.SteamApp.FromDTO());
			segments.TryAdd(x.SteamApp.AppId, segment);
			return x.PlaytimeSegments != segment;
		});
		if(segmentsToUpdate.Any())
		{
			foreach(var app in segmentsToUpdate)
			{
				var segment = segments[app.SteamApp.AppId];
				var uniqueSegments = segment.Except(app.PlaytimeSegments);
				app.PlaytimeSegments.AddRange(uniqueSegments);
			}
			await _steamDb.SaveChangesAsync(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
		}
	}
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
		var segments = new List<PlaytimeSliceDTO>(capacity: 120);
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
	private async IAsyncEnumerable<SteamApp> GetLocalApps([EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var dicLookupTask = _steamDb.AllSteamApps.GroupBy(x => x.AppId).Select(x => x.First()).ToDictionaryAsync(x => x.AppId, cancellationToken).ConfigureAwait(false);
		var primarySearchFile = ApplicationPath.GetPath("MainTimeSliceCheck");
		if(!File.Exists(primarySearchFile))
		{
			// In Future add fallback to alternate files and process them
			// Another method like GetOnlineApps will ask steam for the the apps a user owns
			// and gets the info steam provide like total playtime to present the most basic info
			yield break;
		}
		if(File.Exists(_tmpStorePath))
		{
			File.Delete(_tmpStorePath);
		}
		File.Copy(primarySearchFile, _tmpStorePath);

		var lookup = await dicLookupTask;
		await foreach(var line in IOUtility.ReadLinesAsync(_tmpStorePath, cancellationToken))
		{
			if(cancellationToken.IsCancellationRequested)
			{
				File.Delete(_tmpStorePath);
				yield break;
			}
			if(line == string.Empty)
			{
				continue;
			}
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
			else
			{
				continue;
			}
			if(cancellationToken.IsCancellationRequested)
			{
				File.Delete(_tmpStorePath);
				yield break;
			}
			yield return new SteamApp(appId, appName);
		}
		File.Delete(_tmpStorePath);
	}
}