using SteamPlaytimeTracker.Services.Menu;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.Utility;

namespace SteamPlaytimeTracker.Services.Navigation;

internal interface INavigationService
{
	public IMenuService MenuService { get; }
	public ViewModel CurrentView { get; }

	public OrderedEventInvoker<NavigagtionEventArgs> OnNavigatedTo { get; }

	void NavigateTo<TViewModel>(params object[] @params) where TViewModel : ViewModel;
}