using SteamPlaytimeTracker.MVVM.View.UserControls.Steam;
using System.Collections.ObjectModel;
using SteamPlaytimeTracker.Services;
using SteamPlaytimeTracker.Core;

namespace SteamPlaytimeTracker.MVVM.ViewModel;

internal sealed class HomeViewModel : Core.ViewModel
{
	private INavigationService _navigationService;

	public HomeViewModel(INavigationService navigationService)
	{
		NavigationService = navigationService;
		SwitchToSettingsMenuCommand = new RelayCommand(o => NavigationService.NavigateTo<SettingsViewModel>());
		SteamCapsuleList = new ObservableCollection<SteamCapsule>();
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
}