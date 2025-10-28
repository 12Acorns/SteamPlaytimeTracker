using Serilog;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.IO;
using SteamPlaytimeTracker.MVVM.View.UserControls.Progress;
using SteamPlaytimeTracker.MVVM.View.UserControls.Steam;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.Services.Steam;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.Steam.Data.Capsule;
using SteamPlaytimeTracker.Utility;
using SteamPlaytimeTracker.Utility.Comparer;
using SteamPlaytimeTracker.Utility.Equality;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Data;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class HomeViewModel : Core.ViewModel
{
	private readonly IAsyncLifetimeService _lifetimeProvider;
	private readonly IAppService _appService;
	private readonly AppConfig _appConfig;
	private readonly DbAccess _steamDb;
	private readonly ILogger _logger;

	public HomeViewModel(INavigationService navigationService, IAsyncLifetimeService lifetimeProvider, IAppService appService, ILogger logger,
		AppConfig appConfig, DbAccess steamDb)
	{
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
	public Visibility NoAppsFoundVisibility => SteamApps.Count == 0 ? Visibility.Visible : Visibility.Hidden;
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
		var progress = ProgressSpinnerBar.Create(TimeSpan.FromSeconds(1.5d), (32, 32));
		progress.ShowInTaskbar = false;
		_logger.Information("Loading local Steam apps and syncing database...");
		var loadTask = Task.Run(LoadDataAsync);
		_ = Task.Run(() =>
		{
			while(!loadTask.IsCompleted)
			{

			}
			App.Current.Dispatcher.Invoke(progress.Close);
		});
		progress.ShowDialog();
	}
	private async Task LoadDataAsync()
	{
		await AppendLocalAppsAndSaveToDb();
		var toAdd = (await _appService.AllEntries()).OrderBy(x => x.SteamApp!.Name);
		App.Current.Dispatcher.Invoke(() =>
		{
			SteamApps = new ObservableCollection<SteamAppEntry>(toAdd);
			_logger.Information("Local Steam apps loaded. Found {count} apps", SteamApps.Count);
			SteamAppsView = (ListCollectionView)CollectionViewSource.GetDefaultView(SteamApps);
			SteamAppsView.IsLiveSorting = true;
		});
	}
	private async Task AppendLocalAppsAndSaveToDb()
	{
		var localAppsTask = _appService.GetLocalAppsAsync(_lifetimeProvider.CancellationToken).AsTask();
		var appEntriesTask = _appService.AllEntries().AsTask();
		var fileSegmentsLookup = PlaytimeProvider.GetPlayimeSegments();

		await Task.WhenAll(localAppsTask, appEntriesTask).ConfigureAwait(false);

		if(fileSegmentsLookup == null)
		{
			_logger.Warning("No playtime segments could be retrieved from the primary source. Aborting sync.");
			return;
		}


		_logger.Information("Fetching local apps...");

		var localApps = localAppsTask.Result;
		var appEntries = appEntriesTask.Result
			.Where(x => x.StoreDetails is not null && x.StoreDetails.Exists)
			.ToHashSet(AlternateAppLookup.Instance)
			.GetAlternateLookup<SteamStoreAppData>();


		_logger.Information("Adding new local apps to database...");

		try
		{
			bool hasNewEntry = false;
			foreach(var notFoundEntry in localApps.Where(app => app.Success && !appEntries.Contains(app)))
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

			foreach(var app in appEntries.Set.Where(x => x.SteamApp is not null))
			{
				var segments = fileSegmentsLookup[app.SteamApp!.AppId];
				if(app.PlaytimeSlices.SequenceEqual(segments, PlaytimeSliceEquality.Instance))
				{
					continue;
				}
				var uniqueSegments = segments.Except(app.PlaytimeSlices, PlaytimeSliceEquality.Instance).ToList();
				if(uniqueSegments.Count == 0)
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