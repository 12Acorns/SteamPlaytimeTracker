using SteamPlaytimeTracker.Services.Menu;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.Utility;

namespace SteamPlaytimeTracker.Services.Navigation;

class ViewModelNavigationService : ObservableObject, INavigationService
{
	private readonly Func<Type, object[], ViewModel> _modelViewFactory;

	public ViewModelNavigationService(IMenuService menuService, Func<Type, object[], ViewModel> modelViewFactory)
	{
		MenuService = menuService;
		_modelViewFactory = modelViewFactory ?? throw new ArgumentNullException(nameof(modelViewFactory));
	}
	public ViewModel CurrentView
	{
		get;
		set
		{
			field = value;
			OnPropertyChanged();
		}
	}

	public IMenuService MenuService { get; }
	public OrderedEventInvoker<NavigagtionEventArgs> OnNavigatedTo { get; } = new();

	public void NavigateTo<TViewModel>(params object[] @params)
		where TViewModel : ViewModel
	{
		CurrentView = _modelViewFactory(typeof(TViewModel), @params);
		OnNavigatedTo.Invoke(this, new NavigagtionEventArgs(CurrentView, @params));
	}
}