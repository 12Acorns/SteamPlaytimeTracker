using SteamPlaytimeTracker.Core;

namespace SteamPlaytimeTracker.Services.Navigation;

internal interface INavigationService
{
	public ViewModel CurrentView { get; }
	void NavigateTo<T>(params object[] @params) where T : ViewModel;
}
