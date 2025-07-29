using SteamPlaytimeTracker.Core;

namespace SteamPlaytimeTracker.Services;

internal interface INavigationService
{
	public ViewModel CurrentView { get; }
	void NavigateTo<T>() where T : ViewModel;
}
