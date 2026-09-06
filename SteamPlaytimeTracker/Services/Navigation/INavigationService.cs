using SteamPlaytimeTracker.Services.Menu;
using SteamPlaytimeTracker.Core;

namespace SteamPlaytimeTracker.Services.Navigation;

internal interface INavigationService
{
	public IMenuService MenuService { get; }
	public ViewModel CurrentView { get; }
	void NavigateTo<TViewModel>(params object[] @params) where TViewModel : ViewModel;
}
