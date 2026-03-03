using Serilog;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.MVVM.View;
using SteamPlaytimeTracker.MVVM.View.UserControls.Steam;
using SteamPlaytimeTracker.MVVM.ViewModel.Window;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.Services._App;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Services.Localization;
using SteamPlaytimeTracker.Services.Menu;
using SteamPlaytimeTracker.Services.Messaging;
using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.Services.Playtime;
using SteamPlaytimeTracker.Steam.Data.Capsule;
using SteamPlaytimeTracker.Steam.Data.Playtime;
using SteamPlaytimeTracker.Utility.Comparer;
using SteamPlaytimeTracker.Utility.Equality;
using SteamPlaytimeTracker.Utility.Messaging;
using SteamPlaytimeTracker.Utility.ObservableCollections;
using System.Buffers;
using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Windows.Data;
using System.Windows.Threading;
using ValueTaskSupplement;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class HomeViewModel : Core.ViewModel
{
	private delegate Dictionary<uint, (int Index, SteamAppEntry Entry)> AppLookupCache(
		ref int cachedCount, 
		ref Dictionary<uint, (int Index, SteamAppEntry Entry)> cache,
		ConcurrentObservableCollection<SteamAppEntry> apps);

	private Dictionary<uint, (int Index, SteamAppEntry Entry)> _appLookupCache = [];
	private int _appLookupCacheCount = 0;
	private readonly AppLookupCache _appLookup = (ref cachedCount, ref lookupCache, apps) =>
	{
		if(cachedCount != apps.Count)
		{
			cachedCount = apps.Count;
			return lookupCache = apps.Index().ToDictionary(x => x.Item.SteamApp!.AppId);
		}
		return lookupCache;
	};

	private readonly IMessageExchangeService _messageExchangeService;
	private readonly IMenuService _menuService;
	private readonly ILocalizationService _localizationService;
	private readonly IAsyncLifetimeService _lifetimeProvider;
	private readonly IPlaytimeService _playtimeService;
	private readonly IAppService _appService;
	private readonly DbAccess _steamDb;
	private readonly ILogger _logger;

	private readonly ChannelWriter<SteamAppEntry> _appWriter;
	private readonly ChannelReader<SteamAppEntry> _appReader;
	private readonly Thread _queueProcessThread;

	private readonly ConcurrentQueue<SteamAppEntry> _entriesToSync = [];
	private Thread? _entrySyncThread;

	public HomeViewModel(INavigationService navigationService, IAsyncLifetimeService lifetimeProvider, IAppService appService, ILogger logger,
		AppConfig appConfig, DbAccess steamDb, ILocalizationService localizationService, IPlaytimeService playtimeService, 
		IMessageExchangeService messageExchangeService, IMenuService menuService)
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
		_messageExchangeService = messageExchangeService;
		_localizationService = localizationService;
		_lifetimeProvider = lifetimeProvider;
		_playtimeService = playtimeService;
		_menuService = menuService;
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
		OpenMessgaeMenu = new RelayCommand(_ =>
		{
			if(_menuService.Menus.TryPeek(out var menu) && menu.Menu is ApplicationInfoSubWindow)
			{
				return;
			}
			_menuService.ShowMenu<ApplicationInfoWindowModel, ApplicationInfoSubWindow>();
		});
	}

	public RelayCommand NavigateToPlayTimeViewCommand => new(o => NavigationService.NavigateTo<SteamAppViewModel>(o!), o => o is SteamAppEntry);
	public RelayCommand SwitchToSettingsMenuCommand { get; set; }
	public RelayCommand PlaytimeOrderButtonCommand { get; set; }
	public RelayCommand NameOrderButtonCommand { get; set; }
	public RelayCommand OpenMessgaeMenu { get; set; }
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
	public string FooterText
	{
		get;
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
		var loadStreamTask = LoadDataStreamedAsync();
		loadStreamTask.ContinueWith(t =>
		{
			t.Exception!.Handle(ex =>
			{
				_logger.Error(ex, "An error occurred while loading local Steam apps.");
				return true;
			});
		}, TaskContinuationOptions.OnlyOnFaulted);
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

		try
		{
			// Long running, need to optimise
			var diskApps = await _playtimeService.GetPlayimeSegments(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
			var tasks = new ValueTask[diskApps.Count];
			foreach(var lookup in diskApps.Index())
			{
				tasks[lookup.Index] = FetchAndQueueApp(lookup.Item.Key, _lifetimeProvider.CancellationToken);
			}
			await ValueTaskEx.WhenAll(tasks).ConfigureAwait(false);
		}
		finally
		{
			// After loading all data, syncronize data
			_entrySyncThread = new Thread(SyncDataBackground)
			{
				Priority = ThreadPriority.BelowNormal,
				IsBackground = true,
				Name = "Sync Apps To Db"
			};
			_entrySyncThread.Start();
		}

	}
	private async void SyncDataBackground()
	{
		try
		{
			var allEntriesLookup = (await _appService.AllEntries(_lifetimeProvider.CancellationToken).ConfigureAwait(false))
				.ToDictionary(x => x.SteamApp!.AppId);
			var appsToSync = new List<SteamAppEntry>();
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
					appsToSync.Add(rawEntry);
					continue;
				}
				dbEntry.StoreDetails = rawEntry.StoreDetails;
				if(SequencesEqual(dbEntry.PlaytimeSlices, rawEntry.PlaytimeSlices, PlaytimeSliceEquality.Instance))
				{
					continue;
				}
				var uniqueSegments = rawEntry.PlaytimeSlices.Except(dbEntry.PlaytimeSlices, PlaytimeSliceEquality.Instance).ToList();
				if(uniqueSegments.Count is 0)
				{
					_steamDb.UserApps.Update(dbEntry);
					appsToSync.Add(dbEntry);
					continue;
				}
				_steamDb.PlaytimeSlices.AddRange(uniqueSegments);
				dbEntry.PlaytimeSlices.AddRange(uniqueSegments);
				_steamDb.UserApps.Update(dbEntry);
				appsToSync.Add(dbEntry);
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
		int currentRetriesFromTime = 0;
		int growthFactor = 1;
		var currentWaitTime = TimeSpan.FromMilliseconds(500);
		try
		{
			while(true)
			{
				_lifetimeProvider.CancellationToken.ThrowIfCancellationRequested();
				await ProcessIfCountOrTimePasses(count: 16, time: currentWaitTime, token: _lifetimeProvider.CancellationToken).ConfigureAwait(false);
			}
		}
		catch(Exception ex) when(ex is not OperationCanceledException) 
		{ 
			_logger.Error(ex, "An error occurred while processing the app queue.");
		}
		finally
		{
			timingSemaphore.TryRelease();
		}

		async ValueTask ProcessIfCountOrTimePasses(int count, TimeSpan time, CancellationToken token)
		{
			if(_appReader.Count < count)
			{
				await ProcessAfterTime(count, time, token).ConfigureAwait(false);
				currentRetriesFromTime++;
				if(currentRetriesFromTime >= 3)
				{
					currentWaitTime *= Math.Pow(2.5, growthFactor++);
					if(currentWaitTime.Seconds > 30)
					{
						currentWaitTime = TimeSpan.FromSeconds(30);
					}
					currentRetriesFromTime = 0;
				}
				return;
			}
			await AppendApps(count, token).ConfigureAwait(false);
			currentWaitTime = TimeSpan.FromMilliseconds(500);
			currentRetriesFromTime = 0;
			growthFactor = 1;
		}
		async Task ProcessAfterTime(int count, TimeSpan time, CancellationToken token)
		{
			await timingSemaphore.WaitAsync(time, token).ConfigureAwait(false);
			await AppendApps(count, token).ConfigureAwait(false);
		}
		async ValueTask AppendApps(int count, CancellationToken token)
		{
			App.Current.Dispatcher.Invoke(() =>
			{
				FooterText = $"Syncing {Math.Min(count, _appReader.Count)} apps.";
			}, DispatcherPriority.Normal, cancellationToken: _lifetimeProvider.CancellationToken);
			await _appReader.WaitToReadAsync(token).ConfigureAwait(true);
			var addProcessed = 0;
			var processed = 0;
			var lookup = _appLookup(ref _appLookupCacheCount, ref _appLookupCache, SteamApps);
			var queued = ArrayPool<SteamAppEntry>.Shared.Rent(count);
			while(_appReader.TryRead(out var updatedEntry))
			{
				token.ThrowIfCancellationRequested();
				if(processed >= count)
				{
					break;
				}
				if(lookup.TryGetValue(updatedEntry.SteamApp!.AppId, out var existingEntry))
				{
					App.Current.Dispatcher.Invoke(() =>
					{
						SteamApps[existingEntry.Index] = updatedEntry;
					}, DispatcherPriority.Normal, cancellationToken: _lifetimeProvider.CancellationToken);
				}
				else
				{
					queued[addProcessed] = updatedEntry;
				}
				processed++;
				_messageExchangeService.AddMessage(new Message(MessageType.Information, "Sync",
						$"Syncronized app: {updatedEntry.SteamApp.Name} (AppID: {updatedEntry.SteamApp.AppId})"));
			}
			App.Current.Dispatcher.Invoke(() =>
			{
				SteamApps.AddRange(queued.AsSpan(0, addProcessed));
				HomeView.RefreshArrangement();
				FooterText = $"Syncing no apps.";
			}, DispatcherPriority.Normal, cancellationToken: _lifetimeProvider.CancellationToken);
			ArrayPool<SteamAppEntry>.Shared.Return(queued);
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
				var error = potentialStoreApp.Match<Message>(
					app => throw new InvalidOperationException("Unreachable code, expected error result."),
					parseResult => new Message(MessageType.Error, "Web", $"Failed to parse Store details for App '{appId}'. Parsing Error: {parseResult}"),
					httpStatusCode => new Message(MessageType.Error, "Web", $"Failed to fetch Store details for app '{appId}'. Status Code: {httpStatusCode}"));
				_messageExchangeService.AddMessage(error);
				return;
			}
			var storeApp = potentialStoreApp.AsT0;
			if(storeApp is not { Success: true })
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
	private static bool SequencesEqual(IEnumerable<PlaytimeSlice> first, IEnumerable<PlaytimeSlice> second, IEqualityComparer<PlaytimeSlice> comparer) =>
		first.ToHashSet(comparer).SetEquals(second);
}