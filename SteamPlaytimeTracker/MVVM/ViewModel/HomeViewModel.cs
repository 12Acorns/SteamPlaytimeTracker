using SteamPlaytimeTracker.MVVM.View.UserControls.Steam;
using SteamPlaytimeTracker.Services.Localization;
using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.Steam.Data.Capsule;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Utility.Comparer;
using SteamPlaytimeTracker.Utility.Equality;
using SteamPlaytimeTracker.Services.Steam;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.MVVM.View;
using System.Collections.ObjectModel;
using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.Utility;
using SteamPlaytimeTracker.Core;
using System.Windows.Threading;
using SteamPlaytimeTracker.IO;
using System.Windows.Data;
using ValueTaskSupplement;
using Serilog;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class HomeViewModel : Core.ViewModel
{
	private readonly ILocalizationService _localizationService;
	private readonly IAsyncLifetimeService _lifetimeProvider;
	private readonly IAppService _appService;
	private readonly AppConfig _appConfig;
	private readonly DbAccess _steamDb;
	private readonly ILogger _logger;

	public HomeViewModel(INavigationService navigationService, IAsyncLifetimeService lifetimeProvider, IAppService appService, ILogger logger,
		AppConfig appConfig, DbAccess steamDb, ILocalizationService localizationService)
	{
		_localizationService = localizationService;
		NavigationService = navigationService;
		_lifetimeProvider = lifetimeProvider;
		_appService = appService;
		_logger = logger;
		_appConfig = appConfig;
		_steamDb = steamDb;
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
	public string AppContextText
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}
	public ObservableCollection<SteamAppEntry> SteamApps
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
		_logger.Information("Loading local Steam apps and syncing database...");
		var loadTask = Task.Run(LoadDataAsync, _lifetimeProvider.CancellationToken);
		AppContextText = _localizationService[GlobalData.LoadingAppsKey];
		_ = Task.Run(async () =>
		{
			while(!loadTask.IsCompleted)
			{
				for(int i = 0; i < 2; i++)
				{
					AppContextText += ".";
					await Task.Delay(100, _lifetimeProvider.CancellationToken);
				}
				AppContextText = _localizationService[GlobalData.LoadingAppsKey];
			}
			if(SteamApps.Count > 0)
			{
				AppContextText = string.Empty;
			}
			else
			{
				AppContextText = _localizationService[GlobalData.NoAppsFoundKey];
			}
			App.Current.Dispatcher.Invoke(() =>
			{
				HomeView.RefreshArrangement();
			}, DispatcherPriority.Normal, cancellationToken: _lifetimeProvider.CancellationToken);
		}, _lifetimeProvider.CancellationToken);
	}
	private async Task LoadDataAsync()
	{
		await AppendLocalAppsAndSaveToDb().ConfigureAwait(false);
		var toAdd = (await _appService.AllEntries().ConfigureAwait(false)).OrderBy(x => x.SteamApp?.Name ?? "");
		App.Current.Dispatcher.Invoke(() =>
		{
			SteamApps = new ObservableCollection<SteamAppEntry>(toAdd);
			SteamAppsView = (ListCollectionView)CollectionViewSource.GetDefaultView(SteamApps);
			SteamAppsView.IsLiveSorting = true;

			_logger.Information("Local Steam apps loaded. Found {count} apps", SteamApps.Count);
		}, DispatcherPriority.Normal, cancellationToken: _lifetimeProvider.CancellationToken);
	}
	private async Task AppendLocalAppsAndSaveToDb()
	{
		var (localApps, appEntries, fileSegmentsLookup) = await ValueTaskEx.WhenAll(
			_appService.GetLocalAppsAsync(_lifetimeProvider.CancellationToken), 
			_appService.AllEntries(_lifetimeProvider.CancellationToken),
			 PlaytimeProvider.GetPlayimeSegments(_lifetimeProvider.CancellationToken));
		if(!fileSegmentsLookup.IsNullOrEmpty())
		{
			_logger.Warning("No playtime segments could be retrieved from the primary source. Aborting sync.");
			return;
		}
		_logger.Information("Fetching local apps...");

		var appEntriesLookup = appEntries
			.Where(entry => entry.StoreDetails is { Exists: true } )
			.ToHashSet(AlternateAppLookup.Instance)
			.GetAlternateLookup<SteamStoreAppData>();
		_logger.Information("Adding new local apps to database...");

		try
		{
			bool hasNewEntry = false;
			foreach(var notFoundEntry in localApps.Where(app => app.Success && !appEntriesLookup.Contains(app)))
			{
				notFoundEntry.Id = (int)notFoundEntry.StoreData!.AppId;
				notFoundEntry.StoreData.Id = (int)notFoundEntry.StoreData.AppId;
				var appToAdd = new SteamStoreApp(notFoundEntry);
				_steamDb.UserApps.Add(new SteamAppEntry()
				{
					StoreDetails = appToAdd,
					PlaytimeSlices = fileSegmentsLookup[notFoundEntry.StoreData.AppId]
				});
				_steamDb.SteamStoreApps.Add(appToAdd);
				hasNewEntry = true;
				_logger.Information("New local app added to database: {AppName} (AppID: {AppId})",
					notFoundEntry.StoreData.Name, notFoundEntry.StoreData.AppId);
			}
			if(hasNewEntry)
			{
				await _steamDb.SaveChangesAsync(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
			}

			foreach(var app in appEntriesLookup.Set.Where(x => x.SteamApp is not null))
			{
				var segments = fileSegmentsLookup[app.SteamApp!.AppId];
				if(app.PlaytimeSlices.SequenceEqual(segments, PlaytimeSliceEquality.Instance))
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
				_logger.Information("Updated playtime segments for app: {AppName} (AppID: {AppId}) with {SegmentCount} new segments.",
					app.SteamApp.Name, app.SteamApp.AppId, uniqueSegments.Count);
			}
			await _steamDb.SaveChangesAsync(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
		}
		catch(Exception ex)
		{
			_logger.Error(ex, "An error occurred while syncing local apps with the database.");
			return;
		}
		_logger.Information("Local apps synced with database.");
	}
}