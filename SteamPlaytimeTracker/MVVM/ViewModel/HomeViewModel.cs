using SteamPlaytimeTracker.DbObject.Conversions;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.SelfConfig;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using SteamPlaytimeTracker.DbObject;
using Microsoft.EntityFrameworkCore;
using SteamPlaytimeTracker.Services;
using SteamPlaytimeTracker.Utility;
using SteamPlaytimeTracker.Steam;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.IO;
using System.Diagnostics;
using OutParsing;
using System.IO;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class HomeViewModel : Core.ViewModel
{
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
		NavigationService.NavigateTo<SteamAppViewModel>(o!);
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
		SteamApps = new(await _steamDb.SteamAppEntries.Select(x => x.SteamApp.FromDTO()).ToListAsync().ConfigureAwait(false));

		base.OnConstructed();
	}
	private async Task AppendLocalApps()
	{
		var segmentsLookup = PlaytimeProvider.GetPlayimeSegmentsAsync();

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
				PlaytimeSegments = segmentsLookup[x.Id],
			}), _lifetimeProvider.CancellationToken).ConfigureAwait(false);
			await _steamDb.SaveChangesAsync(_lifetimeProvider.CancellationToken).ConfigureAwait(true);
		}
		var segments = new Dictionary<uint, List<PlaytimeSliceDTO>>();
		var segmentsToUpdate = entriesAppId.Set.AsEnumerable().Where(x =>
		{
			var segment = segmentsLookup[x.SteamApp.AppId];
			segments.TryAdd(x.SteamApp.AppId, segment);
			return x.PlaytimeSegments.Count != segment.Count || 
				!x.PlaytimeSegments.SequenceEqual(segment, EqualityComparer<PlaytimeSliceDTO>.Create((self, other) =>
			{
				if(self == null || other == null)
				{
					return false;
				}
				return self.FromDTO() == other.FromDTO();
			}, x => x.GetHashCode()));
		});
		if(segmentsToUpdate.Any())
		{
			foreach(var app in segmentsToUpdate)
			{
				var segment = segments[app.SteamApp.AppId];
				var uniqueSegments = segment.Except(app.PlaytimeSegments, EqualityComparer<PlaytimeSliceDTO>.Create((self, other) =>
				{
					if(self == null || other == null)
					{
						return false;
					}
					return self.FromDTO() == other.FromDTO();
				}, x => x.GetHashCode()));
				app.PlaytimeSegments.AddRange(uniqueSegments);
			}
			await _steamDb.SaveChangesAsync(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
		}
	}
	private async IAsyncEnumerable<SteamApp> GetLocalApps([EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var primarySearchFile = ApplicationPath.GetPath("MainTimeSliceCheck");
		if(!File.Exists(primarySearchFile))
		{
			// In Future add fallback to alternate files and process them
			// Another method like GetOnlineApps will ask steam for the the apps a user owns
			// and gets the info steam provide like total playtime to present the most basic info (last 2 weeks, and total)
			yield break;
		}
		if(File.Exists(_tmpStorePath))
		{
			File.Delete(_tmpStorePath);
		}
		File.Copy(primarySearchFile, _tmpStorePath);

		var lookup = await _steamDb.AllSteamApps
			.GroupBy(x => x.AppId).Select(x => x.First())
			.ToDictionaryAsync(x => x.AppId, cancellationToken).ConfigureAwait(false);
		var seen = new HashSet<uint>();
		await foreach(var line in IOUtility.ReadLinesAsync(_tmpStorePath, cancellationToken).ConfigureAwait(false))
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
			if(!seen.Add(appId))
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