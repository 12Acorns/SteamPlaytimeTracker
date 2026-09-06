using Microsoft.Win32;
using ScottPlot.Colormaps;
using Serilog;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.DataTransfer;
using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.IO;
using SteamPlaytimeTracker.MVVM.View;
using SteamPlaytimeTracker.MVVM.View.UserControls.Steam;
using SteamPlaytimeTracker.MVVM.View.Windows;
using SteamPlaytimeTracker.MVVM.ViewModel.Window;
using SteamPlaytimeTracker.Services._App;
using SteamPlaytimeTracker.Services.Batching;
using SteamPlaytimeTracker.Services.DataTransfer;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Services.Menu;
using SteamPlaytimeTracker.Services.Messaging;
using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.Services.Playtime;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.Steam.Data.Capsule;
using SteamPlaytimeTracker.Steam.Data.Playtime;
using SteamPlaytimeTracker.Utility;
using SteamPlaytimeTracker.Utility.Equality;
using SteamPlaytimeTracker.Utility.Messaging;
using SteamPlaytimeTracker.Utility.ObservableCollections;
using System.Buffers;
using System.ComponentModel;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using ValueTaskSupplement;
using WpfToolkit.Controls;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class HomeViewModel : Core.ViewModel
{
	private static readonly JsonSerializerOptions _options = new()
	{
		Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
		WriteIndented = true
	};

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
				.Where(x => x is not null)
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
	private readonly ExportService _exportService;
	private readonly DbAccess _dbAccess;
	private readonly IAppService _appService;
	private readonly ILogger _logger;

	private bool _importInProgress = false;

	public HomeViewModel(INavigationService navigationService, ILifetimeService lifetimeProvider, IAppService appService, ILogger logger, 
		IPlaytimeService playtimeService, IMessageExchangeService messageExchangeService, IMenuService menuService, 
		IAppSynchronisationService synchronisationService, IBatchUpdateService<SteamAppEntry> appBatchingService,
		ExportService exportService, DbAccess dbAccess)
	{
		SteamApps = [];
		SteamAppsView = (ListCollectionView)CollectionViewSource.GetDefaultView(SteamApps);
		SteamAppsView.IsLiveSorting = true;
		SteamAppsView.LiveSortingProperties.AddRange(["SteamApp.Name", nameof(SteamAppEntry.TotalPlaytime)]);

		NavigationService = navigationService;
		_messageExchangeService = messageExchangeService;
		_playtimeService = playtimeService;
		_menuService = menuService;
		_synchronisationService = synchronisationService;
		_appService = appService;
		_logger = logger;
		_lifetimeProvider = lifetimeProvider;
		_appBatchingService = appBatchingService;
		_exportService = exportService;
		_dbAccess = dbAccess;
		_appBatchingService.BatchReady += batch =>
		{
			var lookup = _appLookup(ref _appLookupCacheCount, ref _appLookupCache, SteamApps);
			var entriesToUpdate = batch
				.Select<SteamAppEntry, (SteamAppEntry Entry, Indexed<SteamAppEntry>? Updated)>(
					x => (Entry: x, Updated: lookup.TryGetValue(x.SteamApp!.AppId, out var existing) ? existing : null))
				.Where(x => x.Updated is not null)
				.Cast<(SteamAppEntry Entry, Indexed<SteamAppEntry> Updated)>()
				.ToArray();
			Dispatcher.Invoke(() =>
			{
				using(SteamAppsView.DeferRefresh())
				{
					foreach(var (entry, updated) in entriesToUpdate)
					{
						SteamApps[updated.Index] = entry;
					}	
				}
			}, DispatcherPriority.Normal, cancellationToken: _lifetimeProvider.CancellationToken);
			var buffer = ArrayPool<SteamAppEntry>.Shared.Rent(batch.Count);
			var queued = batch.Except(entriesToUpdate.Select(x => x.Entry)).ToArray();
			foreach(var proc in queued)
			{
				_messageExchangeService.AddMessage(new Message(MessageType.Information, "Sync",
						$"Syncronized app: {proc.SteamApp!.Name} (AppID: {proc.SteamApp.AppId})"));
			}
			Dispatcher.Invoke(() =>
			{
				SteamApps.AddRange(queued);
			}, DispatcherPriority.Normal, cancellationToken: _lifetimeProvider.CancellationToken);
			ArrayPool<SteamAppEntry>.Shared.Return(buffer);
		};

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
			var sortDir = sortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending;
			SteamAppsView.SortDescriptions.Clear();
			SteamAppsView.SortDescriptions.Add(new(nameof(SteamAppEntry.TotalPlaytime), sortDir));
			if(!sortAscending)
			{
				CapsuleSortType |= CapsuleSortType.Ascending;
			}
		});
		NameOrderButtonCommand = new(_ =>
		{
			var sortAscending = NameOrderImagePath == GlobalData.NameOrderImagePathFirstLast;
			NameOrderImagePath = sortAscending
				? GlobalData.NameOrderImagePathLastFirst
				: GlobalData.NameOrderImagePathFirstLast;
			CapsuleSortType = CapsuleSortType.Name;
			var sortDir = sortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending;
			SteamAppsView.SortDescriptions.Clear();
			SteamAppsView.SortDescriptions.Add(new("SteamApp.Name", sortDir));
			if(!sortAscending)
			{
				CapsuleSortType |= CapsuleSortType.Ascending;
			}
		});
		CapsuleSortType = CapsuleSortType.Name | CapsuleSortType.Ascending;
		OpenMessageMenu = new RelayCommand(_ =>
		{
			if(_menuService.Menus.TryPeek(out var menu) && menu.Menu is ApplicationInfoSubWindow)
			{
				return;
			}
			_menuService.ShowMenu<ApplicationInfoWindowModel, ApplicationInfoSubWindow>();
		});
		OpenProcessTrackingMenu = new RelayCommand(_ =>
		{
			if(_menuService.Menus.TryPeek(out var menu) && menu.Menu is ProcessTrackingSelectionWindow)
			{
				return;
			}
			_menuService.ShowMenu<ProcessTrackingSelectionWindowModel, ProcessTrackingSelectionWindow>();
		});
		ImportDataCommand = new(async _ =>
		{
			_importInProgress = true;
			try
			{
				using var selectedFileStream = new OpenFileDialog()
				{
					Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
				}.OpenFile();
				var importedPlaytimes = await JsonSerializer.DeserializeAsync<PlaytimeExportStructureContainer>(selectedFileStream, _options);
				foreach(var app in importedPlaytimes.Structure)
				{
					// TODO
					// Check if app exists in db, if not add it, if it does, update it
					// When updating, check if a playtime segment exists already, if so skip, else ensure they do not overlap
					// The two (though really three) cases for overlap are:
					// 1. The new segment is entirely within an existing segment, in which case skip the new segment
					// 2. The new segment entirely encompasses an existing segment, in which case remove the existing segment and add the new segment
					// 3. The new segment partially overlaps an existing segment, in which case merge the two segments into one and add that
					var appEntry = await _appService.GetEntryAsync((uint)app.AppId, _lifetimeProvider.CancellationToken);
					var importedSlices = app.PlaytimeSlices.Select(x => new PlaytimeSlice()
					{
						SessionStart = DateTimeOffset.Parse(x.StartTime),
						SessionLength = TimeSpan.Parse(x.Duration),
						AppId = (uint)app.AppId,
					}).ToList();
					if(appEntry is null)
					{
						var storeDetails = await _appService.GetStoreAppDetailsAsync((uint)app.AppId, _lifetimeProvider.CancellationToken);
						if(!storeDetails.IsT0)
						{
							continue;
						}
						appEntry = new SteamAppEntry()
						{
							StoreDetails = storeDetails.AsT0!,
							PlaytimeSlices = importedSlices
						};
						_dbAccess.UserApps.Add(appEntry);
						_dbAccess.SteamStoreApps.Add(storeDetails.AsT0!);
						_dbAccess.PlaytimeSlices.AddRange(appEntry.PlaytimeSlices);
						continue;
					}
					var dbSlices = appEntry.PlaytimeSlices;
					if(PlaytimeSliceEquality.SequencesEqual(dbSlices, importedSlices))
					{
						continue;
					}

					var orderedIntervals = PlaytimeUtility.CorrectIntervalOverlap(dbSlices.Concat(importedSlices).ToList());
					appEntry.PlaytimeSlices = orderedIntervals;
					_dbAccess.PlaytimeSlices.UpdateRange(orderedIntervals);
					_dbAccess.UserApps.Update(appEntry);
				}
				await _dbAccess.SaveChangesAsync();
			}
			catch(Exception ex)
			{
				_logger.Error(ex, "An error occurred while importing playtime data.");
				_messageExchangeService.AddMessage(new Message(MessageType.Error, "IO", $"An error occurred while importing playtime data. Error: \'{ex}\'"));
				MessageBox.Show("An error occurred while importing playtime data. See logs or messages for more information.", "Error Importing Data",
					MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				_importInProgress = false;
			}
		}, o => !_importInProgress);
		ExportDataCommand = new(_ =>
		{
			var path = Path.Combine(ApplicationPath.GetPath(GlobalData.AppDataStoreLookupName), "Exports");
			var name = $"PlaytimeExport_{DateTime.Now:yyyyMMdd_HHmmss_fff}.json";
			var playtimeFullPath = Path.Combine(path, name);
			_exportService.ExportAllPlaytimeDataAsync(path, name, _lifetimeProvider.CancellationToken).ContinueWith(task =>
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
	}

	public RelayCommand NavigateToPlayTimeViewCommand => new(o => NavigationService.NavigateTo<SteamAppViewModel>(o!), o => o is SteamAppEntry);
	public RelayCommand SwitchToSettingsMenuCommand { get; set; }
	public RelayCommand PlaytimeOrderButtonCommand { get; set; }
	public RelayCommand NameOrderButtonCommand { get; set; }
	public RelayCommand OpenProcessTrackingMenu { get; set; }
	public RelayCommand OpenMessageMenu { get; set; }
	public RelayCommand ImportDataCommand { get; set; }
	public RelayCommand ExportDataCommand { get; set; }
	public INavigationService NavigationService { get; set; }
	public ConcurrentObservableCollection<SteamAppEntry> SteamApps
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	} = [];
	public ListCollectionView SteamAppsView
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	} = default!;
	public double UniformWidth
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	} = SteamCapsule.BaseWidth;
	public double UniformHeight
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	} = SteamCapsule.BaseWidth * SteamCapsule.HeightScaleFactor;
	public string PlaytimeOrderImagePath
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public string NameOrderImagePath
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public CapsuleSortType CapsuleSortType
	{
		get;
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
		get;
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
		var loadStreamTask = LoadAppData().ContinueWith(x =>
		{
			if(x.IsFaulted)
			{
				x.Exception.Handle(ex =>
				{
					_logger.Error(ex, "An error occurred while loading local Steam apps.");
					return true;
				});
				return;
			}
			_appBatchingService.StartProcessing(_lifetimeProvider.CancellationToken);
		}, cancellationToken: _lifetimeProvider.CancellationToken);
	}
	private async Task LoadAppData()
	{
		var storedApps = await _appService.AllEntries(_lifetimeProvider.CancellationToken).ConfigureAwait(true);
		SteamApps.AddRange(storedApps);

		try
		{
			// Long running, need to optimise
			// As a temp fix, due to how caching works, I will call this method in app startup but not await the task, allowing loading
			// and not halt startup by needing to wait on main thread if i were to wait the task (due to startup not being a async task itself)
			// The reason this can be done is due to the cache having an internal semaphore that handles retrieval
			// Meaning once the first task begins loading data, any other calls to the cache will wait until the first call has loaded data and released the semaphore,
			// allowing them to then retrieve data from the cache without needing to load data themselves
			// Giving the illusion to faster loading times, when in reality the loading is just being done in the background whilst WPF starts up
			var diskApps = await _playtimeService.GetPlayimeIntervalsMap(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
			await ValueTaskEx.WhenAll(diskApps.Select(x => FetchAndQueueApp(x.Key, _lifetimeProvider.CancellationToken))).ConfigureAwait(false);
		}
		finally
		{
			// After loading all data, syncronize data
			await _synchronisationService.CommitSyncToDbAsync(_lifetimeProvider.CancellationToken);
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
			storeApp.Id = (int)storeApp.StoreData.AppId;
			storeApp.StoreData.Id = storeApp.Id;
			var entry = new SteamAppEntry()
			{
				StoreDetails = (SteamStoreApp)storeApp, // converted into SteamStoreApp, the conversion sets the id in constructor
				PlaytimeSlices = segmentsForEntry
			};
			_synchronisationService.EnqueueForDbSync(entry);
			var success = _appBatchingService.TryEnqueue(entry);
		}
		catch(Exception ex) when (ex is not OperationCanceledException)
		{
			_logger.Error(ex, "An error occurred while fetching and loading app with AppID {AppId}.", appId);
		}
	}
}