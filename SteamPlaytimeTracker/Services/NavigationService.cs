using SteamPlaytimeTracker.Core;

namespace SteamPlaytimeTracker.Services;

class NavigationService : ObservableObject, INavigationService
{
	private readonly Func<Type, ViewModel> _modelViewFactory;
	private ViewModel _currentView;

	public NavigationService(Func<Type, ViewModel> modelViewFactory)
	{
		_modelViewFactory = modelViewFactory ?? throw new ArgumentNullException(nameof(modelViewFactory));
	}


	public ViewModel CurrentView
	{
		get => _currentView;
		set
		{
			_currentView = value;
			OnPropertyChanged();
		}
	}
	public void NavigateTo<TViewModel>() where TViewModel : ViewModel
	{
		CurrentView = _modelViewFactory(typeof(TViewModel));
	}
}