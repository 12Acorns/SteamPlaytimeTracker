using Microsoft.EntityFrameworkCore;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.IO;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Services.Navigation;
using SteamPlaytimeTracker.Services.Steam;
using SteamPlaytimeTracker.Steam;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.Steam.Data.Capsule;
using SteamPlaytimeTracker.Utility;
using SteamPlaytimeTracker.Utility.Comparer;
using SteamPlaytimeTracker.Utility.Equality;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Data;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class HomeViewModel : Core.ViewModel
{
	private readonly IAsyncLifetimeService _lifetimeProvider;
	private readonly IAppService _appService;
	private readonly AppConfig _appConfig;
	private readonly DbAccess _steamDb;

	public HomeViewModel(INavigationService navigationService, IAsyncLifetimeService lifetimeProvider, IAppService appService, AppConfig appConfig, DbAccess steamDb)
	{
		NavigationService = navigationService;
		_lifetimeProvider = lifetimeProvider;
		_appService = appService;
		_appConfig = appConfig;
		_steamDb = steamDb;

		SwitchToSettingsMenuCommand = new RelayCommand(o => NavigationService.NavigateTo<SettingsViewModel>());
		PlaytimeOrderImagePath = "/resources/Playtime-Order-Icon_First-Last.png";
		NameOrderImagePath = "/resources/Name-Order-Icon_First-Last.png";
		PlaytimeOrderButtonCommand = new(_ =>
		{
			var sortAscending = PlaytimeOrderImagePath == "/resources/Playtime-Order-Icon_First-Last.png";
			PlaytimeOrderImagePath = sortAscending
				? "/resources/Playtime-Order-Icon_Last-First.png"
				: "/resources/Playtime-Order-Icon_First-Last.png";
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
			var sortAscending = NameOrderImagePath == "/resources/Name-Order-Icon_First-Last.png";
			NameOrderImagePath = sortAscending
				? "/resources/Name-Order-Icon_Last-First.png"
				: "/resources/Name-Order-Icon_First-Last.png";
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
	}, o => o is SteamApp);
	public RelayCommand SwitchToSettingsMenuCommand { get; set; }
	public RelayCommand PlaytimeOrderButtonCommand { get; set; }
	public RelayCommand NameOrderButtonCommand { get; set; }
	public INavigationService NavigationService { get; set; }
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

	public override async void OnConstructed()
	{
		if(!await _steamDb.SteamApps.AnyAsync().ConfigureAwait(false))
		{
			var res = await SteamRequest.GetAppListAsync(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
			if(res.IsT0)
			{
				var response = res.AsT0;
				_steamDb.SteamApps.AddRange(response.Apps.SteamApps);
				_appConfig.AppData.LastCheckedSteamApps = Stopwatch.GetTimestamp();
				await _steamDb.SaveChangesAsync(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
			}
		}
		await AppendLocalAppsAndSaveToDb().ConfigureAwait(false);
		var toAdd = (await _steamDb.LocalApps.Include(x => x.SteamApp).Include(x => x.PlaytimeSlices).ToListAsync().ConfigureAwait(false))
			.OrderBy(x => x.SteamApp.Name, StringComparer.InvariantCultureIgnoreCase);
		SteamApps = new(toAdd);
		SteamAppsView = (ListCollectionView)CollectionViewSource.GetDefaultView(SteamApps);
		SteamAppsView.IsLiveSorting = true;
		base.OnConstructed();
	}
	private async Task AppendLocalAppsAndSaveToDb()
	{
		var fileSegmentsLookup = PlaytimeProvider.GetPlayimeSegments();

		var localApps = await _appService.GetLocalAppsAsync(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
		var steamAppEntriesSet = (await _steamDb.LocalApps
			.Include(x => x.SteamApp)
			.Include(x => x.PlaytimeSlices)
			.AsNoTracking()
			.ToHashSetAsync(AlternateAppLookup.Instance).ConfigureAwait(false))
			.GetAlternateLookup<SteamApp>();

		bool hasNewEntry = false;
		foreach(var notFoundEntry in localApps.Where(x => !steamAppEntriesSet.Contains(x)))
		{
			// Tmp, eventually move to own class and call that here
			_steamDb.LocalApps.Add(new SteamAppEntry()
			{
				SteamApp = notFoundEntry,
				PlaytimeSlices = fileSegmentsLookup[notFoundEntry.AppId],
			});
			hasNewEntry = true;
		}
		if(hasNewEntry)
		{
			await _steamDb.SaveChangesAsync(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
		}

		var entriesEnumerable = steamAppEntriesSet.Set.AsEnumerable();
		foreach(var app in entriesEnumerable)
		{
			var segments = fileSegmentsLookup[app.SteamApp.AppId];
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
			_steamDb.LocalApps.Update(app);
		}
		await _steamDb.SaveChangesAsync(_lifetimeProvider.CancellationToken).ConfigureAwait(false);
	}
}