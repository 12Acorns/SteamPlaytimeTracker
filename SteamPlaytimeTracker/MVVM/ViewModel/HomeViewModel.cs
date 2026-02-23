using OneOf;
using Serilog;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.MVVM.View;
using SteamPlaytimeTracker.MVVM.View.UserControls.Steam;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.Services._App;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Services.Localization;
using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.Services.Playtime;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.Steam.Data.Capsule;
using SteamPlaytimeTracker.Steam.Data.Playtime;
using SteamPlaytimeTracker.Utility;
using SteamPlaytimeTracker.Utility.Comparer;
using SteamPlaytimeTracker.Utility.Equality;
using SteamPlaytimeTracker.Utility.ObservableCollections;
using System.Collections.Concurrent;
using System.Net;
using System.Threading.Channels;
using System.Windows.Data;
using System.Windows.Threading;
using ValueTaskSupplement;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class HomeViewModel : Core.ViewModel
{
	private readonly ILocalizationService _localizationService;
	private readonly IAsyncLifetimeService _lifetimeProvider;
	private readonly IPlaytimeService _playtimeService;
	private readonly IAppService _appService;
	private readonly DbAccess _steamDb;
	private readonly ILogger _logger;

	private readonly ChannelWriter<SteamAppEntry> _appWriter;
	private readonly ChannelReader<SteamAppEntry> _appReader;
	private readonly Thread _queueProcessThread;

	private readonly ConcurrentQueue<OneOf<ParseResult, HttpStatusCode>> _webFetchErrors = [];

	private readonly ConcurrentQueue<SteamAppEntry> _entriesToSync = [];
	private Thread? _entrySyncThread;

	public HomeViewModel(INavigationService navigationService, IAsyncLifetimeService lifetimeProvider, IAppService appService, ILogger logger,
		AppConfig appConfig, DbAccess steamDb, ILocalizationService localizationService, IPlaytimeService playtimeService)
	{
		var channel = Channel.CreateUnbounded<SteamAppEntry>(new UnboundedChannelOptions()
		{
			SingleReader = false,
			SingleWriter = false,
		});
		(_appReader, _appWriter) = (channel.Reader, channel.Writer);
		_queueProcessThread = new Thread(ProcessQueuedApps)
		{
			Priority = ThreadPriority.BelowNormal,
			IsBackground = true,
			Name = "App Queue Processor",
		};
		_queueProcessThread.Start();

		NavigationService = navigationService;
		_localizationService = localizationService;
		_lifetimeProvider = lifetimeProvider;
		_playtimeService = playtimeService;
		_appService = appService;
		_steamDb = steamDb;
		_logger = logger;

		SwitchToSettingsMenuCommand = new RelayCommand(o => NavigationService.NavigateTo<SettingsViewModel>());
		PlaytimeOrderImagePath = GlobalData.PlaytimeOrderImagePathFirstLast;
		NameOrderImagePath = GlobalData.NameOrderImagePathFirstLast;
		PlaytimeOrderButtonCommand = new(_ =>
		{
			var sortAscending = PlaytimeOrderImagePath == GlobalData.PlaytimeOrderImagePathFirstLast;
			PlaytimeOrderImagePath = sortAscending
				? GlobalData.PlaytimeOrderImagePathLastFirst
				: GlobalData.PlaytimeOrderImagePathFirstLast;
			CapsuleSortType = CapsuleSortType.Playtime;
			if(!sortAscending)
			{
				SteamAppsView.CustomSort = new AppPlaytimeComparer(descending: true);
				CapsuleSortType |= CapsuleSortType.Ascending;
			}
			else
			{
				SteamAppsView.CustomSort = new AppPlaytimeComparer(descending: false);
			}
		});

		NameOrderButtonCommand = new(_ =>
		{
			var sortAscending = NameOrderImagePath == GlobalData.NameOrderImagePathFirstLast;
			NameOrderImagePath = sortAscending
				? GlobalData.NameOrderImagePathLastFirst
				: GlobalData.NameOrderImagePathFirstLast;
			CapsuleSortType = CapsuleSortType.Name;
			if(!sortAscending)
			{
				CapsuleSortType |= CapsuleSortType.Ascending;
				SteamAppsView.CustomSort = new AppNameComparer(false);
			}
			else
			{
				SteamAppsView.CustomSort = new AppNameComparer(true);
			}
		});
		CapsuleSortType = CapsuleSortType.Name | CapsuleSortType.Ascending;
	}

	public RelayCommand NavigateToPlayTimeViewCommand => new(o =>
	{
		NavigationService.NavigateTo<SteamAppViewModel>(o!);
	}, o => o is SteamAppEntry);
	public RelayCommand SwitchToSettingsMenuCommand { get; set; }
	public RelayCommand PlaytimeOrderButtonCommand { get; set; }
	public RelayCommand NameOrderButtonCommand { get; set; }
	public INavigationService NavigationService { get; set; }
	public ConcurrentObservableCollection<SteamAppEntry> SteamApps
	{
		get => field;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	} = [];
	public ListCollectionView SteamAppsView
	{
		get => field;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	} = default!;
	public double UniformWidth
	{
		get => field;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	} = SteamCapsule.BaseWidth;
	public double UniformHeight
	{
		get => field;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	} = SteamCapsule.BaseWidth * SteamCapsule.HeightScaleFactor;
	public string PlaytimeOrderImagePath
	{
		get => field;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public string NameOrderImagePath
	{
		get => field;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public CapsuleSortType CapsuleSortType
	{
		get => field;
		private set
		{
			field = value;
			var sortWayText = (field & CapsuleSortType.Ascending) is 0 ? "Descending" : "Ascending";
			CurrentSortType = $"Sort: {(CapsuleSortType)((int)field & 1)} | {sortWayText}";
			OnPropertyChanged();
		}
	} = CapsuleSortType.Name | CapsuleSortType.Ascending;
	public string CurrentSortType
	{
		get => field;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	} = string.Empty;

	public override void OnConstructed()
	{
		base.OnConstructed();
		_logger.Debug("Loading local Steam apps and syncing database...");
		_ = LoadDataStreamedAsync();
	}
	private async Task LoadDataStreamedAsync()
	{
		App.Current.Dispatcher.Invoke(() =>
		{
			SteamApps = [];
			SteamAppsView = (ListCollectionView)CollectionViewSource.GetDefaultView(SteamApps);
			SteamAppsView.IsLiveSorting = true;

			_logger.Debug("Local Steam apps loaded. Found {count} apps", SteamApps.Count);
		}, DispatcherPriority.Normal, cancellationToken: _lifetimeProvider.CancellationToken);


		var localApps = await _appService.AllEntries(_lifetimeProvider.CancellationToken).ConfigureAwait(true);
		SteamApps.AddRange(localApps);

		var diskApps = await _playtimeService.GetPlayimeSegments(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
		await Parallel.ForEachAsync(diskApps.Chunk(32), new ParallelOptions() 
		{ 
			MaxDegreeOfParallelism = 4,
			CancellationToken = _lifetimeProvider.CancellationToken
		}, (apps, token) =>
		{
			var tasks = new ValueTask[apps.Length];
			for(int i = 0; i < tasks.Length; i++)
			{
				tasks[i] = FetchAndQueueApp(apps[i].Key, _lifetimeProvider.CancellationToken);
			}
			return ValueTaskEx.WhenAll(tasks);
		}).ConfigureAwait(false);

		// After loading all data, syncronize data
		_entrySyncThread = new Thread(SyncDataBackground)
		{
			Priority = ThreadPriority.BelowNormal,
			IsBackground = true,
			Name = "Sync Apps To Db"
		};
		_entrySyncThread.Start();
	}
	private async void SyncDataBackground()
	{
		try
		{
			var allEntriesLookup = (await _appService.AllEntries(_lifetimeProvider.CancellationToken).ConfigureAwait(false))
				.ToDictionary(x => x.SteamApp!.AppId);
			while(_entriesToSync.TryDequeue(out var rawEntry))
			{
				_lifetimeProvider.CancellationToken.ThrowIfCancellationRequested();
				if(rawEntry.SteamApp is null)
				{
					continue;
				}
				if(!allEntriesLookup.TryGetValue(rawEntry.SteamApp.AppId, out var dbEntry))
				{
					_steamDb.UserApps.Add(rawEntry);
					continue;
				}
				if(SequencesEqual(dbEntry.PlaytimeSlices, rawEntry.PlaytimeSlices, PlaytimeSliceEquality.Instance))
				{
					continue;
				}
				dbEntry.StoreDetails = rawEntry.StoreDetails;
				var uniqueSegments = rawEntry.PlaytimeSlices.Except(dbEntry.PlaytimeSlices, PlaytimeSliceEquality.Instance).ToList();
				if(uniqueSegments.Count is 0)
				{
					continue;
				}
				_steamDb.PlaytimeSlices.AddRange(uniqueSegments);
				dbEntry.PlaytimeSlices.AddRange(uniqueSegments);
				_steamDb.UserApps.Update(dbEntry);
				_logger.Verbose("Updated playtime segments for app: {AppName} (AppID: {AppId}) with {SegmentCount} new segments.",
					dbEntry.SteamApp!.Name, dbEntry.SteamApp.AppId, uniqueSegments.Count);
			}
			await _steamDb.SaveChangesAsync(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
		}
		catch(Exception ex) when (ex is not OperationCanceledException)
		{
			_logger.Error(ex, "Failed to sync apps to database");
		}
	}
	private async void ProcessQueuedApps()
	{
		var timingSemaphore = new SemaphoreSlim(1, 1);
		try
		{
			while(true)
			{
				_lifetimeProvider.CancellationToken.ThrowIfCancellationRequested();
				await ProcessIfCountOrTimePasses(count: 4, time: TimeSpan.FromMilliseconds(500), token: _lifetimeProvider.CancellationToken).ConfigureAwait(false);
			}
		}
		catch { }
		finally
		{
			timingSemaphore.TryRelease();
		}

		async ValueTask ProcessIfCountOrTimePasses(int count, TimeSpan time, CancellationToken token)
		{
			if(_appReader.Count < count)
			{
				await ProcessAfterTime(count, time, token).ConfigureAwait(false);
				return;
			}
			await AppendApps(count, token).ConfigureAwait(false);
		}
		async Task ProcessAfterTime(int count, TimeSpan time, CancellationToken token)
		{
			await timingSemaphore.WaitAsync(time, token).ConfigureAwait(false);
			await AppendApps(count, token).ConfigureAwait(false);
		}
		async ValueTask AppendApps(int count, CancellationToken token)
		{
			await _appReader.WaitToReadAsync(token).ConfigureAwait(true);
			App.Current.Dispatcher.Invoke(() =>
			{
				var processed = 0;
				while(_appReader.TryRead(out var entry))
				{
					if(processed >= count)
					{
						break;
					}
					SteamApps.Add(entry);
					processed++;
				}
				HomeView.RefreshArrangement();
			}, DispatcherPriority.Normal, cancellationToken: token);
		}
	}
	// TODO
	// Change how loading operates
	// Instead of directly adding to the ObservableCollection from FetchAndLoadApp,
	// gather entries and add to a Channel as each gets loaded
	// Have another task reading from the Channel and adding to the ObservableCollection in chunks
	// (IE for every 16 items or if more than one second passes add all items present in channel to the collection)
	// And for each item in the channel begin a seperate task that handles loading and syncing data to the database
	// And if a item is being loaded and a user clicks to view said item, display a loading indicator on the capsule until loading is complete
	// And then figure out if anything particulary special needs to be performed for that or if its that simple
	private async ValueTask FetchAndQueueApp(uint appId, CancellationToken token = default)
	{
		if(token == default)
		{
			token = _lifetimeProvider.CancellationToken;
		}
		try
		{
			// Instead load data from db and perform syncing to online source in the background

			var potentialStoreApp = await _appService.GetStoreAppDetailsAsync(appId, token).ConfigureAwait(false);
			if(!potentialStoreApp.IsT0)
			{
				_logger.Warning("Requested to stream app with AppID {AppId}, but no such app was found.", appId);
				var error = potentialStoreApp.Match<OneOf<ParseResult, HttpStatusCode>>(
					app => throw new InvalidOperationException("Unreachable code, expected error result."),
					parseResult => parseResult,
					httpStatusCode => httpStatusCode);
				_webFetchErrors.Enqueue(error);
				return;
			}
			var storeApp = potentialStoreApp.AsT0;
			if(storeApp is not { Success: true }) // equiv -> storeApp is { Success: false }
			{
				return;
			}
			var segmentsForEntry = (await _playtimeService.TryGetSegmentsForApp(storeApp.StoreData.AppId, token).ConfigureAwait(false)).DefaultWith(() => []);
			var entry = new SteamAppEntry()
			{
				StoreDetails = storeApp,
				PlaytimeSlices = segmentsForEntry
			};
			_entriesToSync.Enqueue(entry);
			await _appWriter.WriteAsync(entry, token).ConfigureAwait(false);
		}
		catch(Exception ex) when (ex is not OperationCanceledException)
		{
			_logger.Error(ex, "An error occurred while fetching and loading app with AppID {AppId}.", appId);
		}
	}

	private async Task AppendLocalAppsAndSaveToDb()
	{
		var (localApps, appEntries, fileSegmentsLookup) = await ValueTaskEx.WhenAll(
			_appService.GetLocalAppsAsync(_lifetimeProvider.CancellationToken), 
			_appService.AllEntries(_lifetimeProvider.CancellationToken),
			 _playtimeService.GetPlayimeSegments(_lifetimeProvider.CancellationToken));
		if(fileSegmentsLookup.IsNullOrEmpty())
		{
			_logger.Warning("No playtime segments could be retrieved from the primary source. This could indicate no log file was found.");
			return;
		}
		_logger.Debug("Fetching local apps...");

		var appEntriesLookup = appEntries
			.Where(entry => entry.StoreDetails is { Exists: true } )
			.ToHashSet(AlternateAppLookup.Instance)
			.GetAlternateLookup<SteamStoreAppData>();

		_logger.Debug("Adding new local apps to database...");

		try
		{
			bool hasNewEntry = false;
			foreach(var notFoundEntry in localApps.Where(app => app.Success && !appEntriesLookup.Contains(app)))
			{
				notFoundEntry.Id = (int)notFoundEntry.StoreData!.AppId;
				notFoundEntry.StoreData.Id = (int)notFoundEntry.StoreData.AppId;
				var appToAdd = new SteamStoreApp(notFoundEntry);
				if(!fileSegmentsLookup.TryGetValue(notFoundEntry.StoreData.AppId, out var segments))
				{
					segments = [];
				}
				_steamDb.UserApps.Add(new SteamAppEntry()
				{
					StoreDetails = appToAdd,
					PlaytimeSlices = segments
				});
				_steamDb.SteamStoreApps.Add(appToAdd);
				hasNewEntry = true;
				_logger.Verbose("New local app added to database: {AppName} (AppID: {AppId})",
					notFoundEntry.StoreData.Name, notFoundEntry.StoreData.AppId);
			}
			if(hasNewEntry)
			{
				await _steamDb.SaveChangesAsync(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
			}

			foreach(var app in appEntriesLookup.Set.Where(x => x.SteamApp is not null))
			{
				var segments = fileSegmentsLookup[app.SteamApp!.AppId];
				if(SequencesEqual(app.PlaytimeSlices, segments, PlaytimeSliceEquality.Instance))
				{
					continue;
				}
				var uniqueSegments = segments.Except(app.PlaytimeSlices, PlaytimeSliceEquality.Instance).ToList();
				if(uniqueSegments.Count is 0)
				{
					continue;
				}

				_steamDb.PlaytimeSlices.AddRange(uniqueSegments);
				app.PlaytimeSlices.AddRange(uniqueSegments);
				_steamDb.UserApps.Update(app);
				_logger.Verbose("Updated playtime segments for app: {AppName} (AppID: {AppId}) with {SegmentCount} new segments.",
					app.SteamApp.Name, app.SteamApp.AppId, uniqueSegments.Count);
			}
			await _steamDb.SaveChangesAsync(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
		}
		catch(Exception ex)
		{
			_logger.Error(ex, "An error occurred while syncing local apps with the database.");
			return;
		}
		_logger.Debug("Local apps synced with database.");
	}
	private static bool SequencesEqual(IEnumerable<PlaytimeSlice> first, IEnumerable<PlaytimeSlice> second, IEqualityComparer<PlaytimeSlice> comparer) =>
		first.ToHashSet(comparer).SetEquals(second);
}