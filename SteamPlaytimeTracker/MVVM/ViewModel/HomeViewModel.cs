using Serilog;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.MVVM.View;
using SteamPlaytimeTracker.MVVM.View.UserControls.Steam;
using SteamPlaytimeTracker.MVVM.ViewModel.Window;
using SteamPlaytimeTracker.Services._App;
using SteamPlaytimeTracker.Services.Batching;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Services.Menu;
using SteamPlaytimeTracker.Services.Messaging;
using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.Services.Playtime;
using SteamPlaytimeTracker.Steam.Data.Capsule;
using SteamPlaytimeTracker.Utility;
using SteamPlaytimeTracker.Utility.Comparer;
using SteamPlaytimeTracker.Utility.Messaging;
using SteamPlaytimeTracker.Utility.ObservableCollections;
using System.Buffers;
using System.Windows.Data;
using System.Windows.Threading;
using ValueTaskSupplement;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class HomeViewModel : Core.ViewModel
{
	private delegate Dictionary<uint, Indexed<SteamAppEntry>> AppLookupCache(
		ref int cachedCount, 
		ref Dictionary<uint, Indexed<SteamAppEntry>> cache,
		ConcurrentObservableCollection<SteamAppEntry> apps);

	private Dictionary<uint, Indexed<SteamAppEntry>> _appLookupCache = [];
	private int _appLookupCacheCount = 0;
	private readonly AppLookupCache _appLookup = (ref cachedCount, ref lookupCache, apps) =>
	{
		if(cachedCount != apps.Count)
		{
			cachedCount = apps.Count;
			return lookupCache = apps
				.Index()
				.ToDictionary(k => k.Item.SteamApp!.AppId, item => (Indexed<SteamAppEntry>)item);
		}
		return lookupCache;
	};

	private readonly IMessageExchangeService _messageExchangeService;
	private readonly IMenuService _menuService;
	private readonly IAppSynchronisationService _synchronisationService;
	private readonly IBatchUpdateService<SteamAppEntry> _appBatchingService;
	private readonly ILifetimeService _lifetimeProvider;
	private readonly IPlaytimeService _playtimeService;
	private readonly IAppService _appService;
	private readonly ILogger _logger;

	private readonly Thread _queueProcessThread;

	public HomeViewModel(INavigationService navigationService, ILifetimeService lifetimeProvider, IAppService appService, ILogger logger, 
		IPlaytimeService playtimeService, IMessageExchangeService messageExchangeService, IMenuService menuService, 
		IAppSynchronisationService synchronisationService, IBatchUpdateService<SteamAppEntry> appBatchingService)
	{
		NavigationService = navigationService;
		_messageExchangeService = messageExchangeService;
		_playtimeService = playtimeService;
		_menuService = menuService;
		_synchronisationService = synchronisationService;
		_appService = appService;
		_logger = logger;
		_lifetimeProvider = lifetimeProvider;
		_appBatchingService = appBatchingService;
		_appBatchingService.BatchReady += batch =>
		{
			var lookup = _appLookup(ref _appLookupCacheCount, ref _appLookupCache, SteamApps);
			var entriesToUpdate = batch
				.Select(x => (Entry: x, Updated: lookup.TryGetValue(x.SteamApp!.AppId, out var existing) ? existing : null))
				.Where(x => x.Updated is not null).ToArray();
			Dispatcher.Invoke(() =>
			{
				foreach(var (entry, updated) in entriesToUpdate)
				{
					SteamApps[updated!.Index] = entry;
				}	
			}, DispatcherPriority.Normal, cancellationToken: _lifetimeProvider.CancellationToken);
			var queued = ArrayPool<SteamAppEntry>.Shared.Rent(batch.Count);
			var remaining = batch.Except(entriesToUpdate.Select(x => x.Entry)).ToArray();
			foreach(var proc in remaining)
			{
				_messageExchangeService.AddMessage(new Message(MessageType.Information, "Sync",
						$"Syncronized app: {proc.SteamApp!.Name} (AppID: {proc.SteamApp.AppId})"));
			}
			Dispatcher.Invoke(() =>
			{
				SteamApps.AddRange(queued);
				HomeView.RefreshArrangement();
			}, DispatcherPriority.Normal, cancellationToken: _lifetimeProvider.CancellationToken);
		};
		_queueProcessThread = new Thread(() => _appBatchingService.StartProcessing(_lifetimeProvider.CancellationToken))
		{
			Priority = ThreadPriority.BelowNormal,
			IsBackground = true,
			Name = "App Queue Processor",
		};
		_queueProcessThread.Start();

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
		var loadStreamTask = LoadAppData();
		loadStreamTask.ContinueWith(t =>
		{
			t.Exception!.Handle(ex =>
			{
				_logger.Error(ex, "An error occurred while loading local Steam apps.");
				return true;
			});
		}, TaskContinuationOptions.OnlyOnFaulted);
	}
	private async Task LoadAppData()
	{
		Dispatcher.Invoke(() =>
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
			// As a temp fix, due to how caching works, I will call this method in app startup but not await the task, allowing loading
			// and not halt startup by needing to wait on main thread if i were to wait the task (due to startup not being a async task itself)
			// The reason this can be done is due to the cache having an internal semaphore that handles retrieval
			// Meaning once the first task begins loading data, any other calls to the cache will wait until the first call has loaded data and released the semaphore,
			// allowing them to then retrieve data from the cache without needing to load data themselves
			// Giving the illusion to faster loading times, when in reality the loading is just being done in the background whilst WPF starts up
			var diskApps = await _playtimeService.GetPlayimeSegments(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
			await ValueTaskEx.WhenAll(diskApps.Select(x => FetchAndQueueApp(x.Key, _lifetimeProvider.CancellationToken))).ConfigureAwait(false);
		}
		finally
		{
			// After loading all data, syncronize data
			_ = _synchronisationService.CommitSyncToDbAsync(_lifetimeProvider.CancellationToken);
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
			_synchronisationService.EnqueueForDbSync(entry);
		}
		catch(Exception ex) when (ex is not OperationCanceledException)
		{
			_logger.Error(ex, "An error occurred while fetching and loading app with AppID {AppId}.", appId);
		}
	}
}