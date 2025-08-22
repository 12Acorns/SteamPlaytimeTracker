using SteamPlaytimeTracker.Core;

namespace SteamPlaytimeTracker.Services;

class ViewModelNavigationService : ObservableObject, INavigationService
{
	private readonly Func<Type, ReadOnlySpan<object>, ViewModel> _modelViewFactory;
	private ViewModel _currentView = null!;

	public ViewModelNavigationService(Func<Type, ReadOnlySpan<object>, ViewModel> modelViewFactory)
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
	public void NavigateTo<TViewModel>(params ReadOnlySpan<object> @params) where TViewModel : ViewModel
	{
		CurrentView = _modelViewFactory(typeof(TViewModel), @params);
	}
}