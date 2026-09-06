using ScottPlot.Colormaps;
using Serilog;
using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.Services.Lifetime;
using System.Windows;
using System.Windows.Threading;
using WpfToolkit.Controls;

namespace SteamPlaytimeTracker.Services.Menu;

internal sealed class MenuService : ObservableObject, IMenuService
{
	public delegate (MenuModel Model, Window Menu) ModelFactory(Type model, Type menu, params object[] @params);
	
	private readonly ModelFactory _menuModelFactory;
	private readonly ILogger _logger;

	public event Action<MenuModel, Window> OnLoaded;

	public MenuService(ModelFactory menuModelFactory, ILogger logger)
	{
		_menuModelFactory = menuModelFactory ?? throw new ArgumentNullException(nameof(menuModelFactory));
		_logger = logger;
	}

	public Stack<(MenuModel Model, Window Menu)> Menus { get; } = [];

	public void ShowMenu<TModel, TMenu>(bool domainMenu = false, params object[] @params)
		where TModel : MenuModel
		where TMenu : Window, IMenuWindow<TModel>
	{
		Menus.Push(_menuModelFactory(typeof(TModel), typeof(TMenu), @params ?? []));
		var curr = Menus.Peek();
		curr.Menu.Title = curr.Model.Title;
		void Loaded(object sender, RoutedEventArgs e)
		{
			OnLoaded?.Invoke(curr.Model, curr.Menu);
			curr.Menu.Loaded -= Loaded;
		}
		curr.Menu.Closed += OnClosing();
		curr.Menu.Loaded += Loaded;
		try
		{
			if(domainMenu)
			{
				curr.Menu.ShowDialog();
			}
			else
			{
				curr.Menu.Show();
			}
		}
		catch(Exception ex)
		{
			_logger.Error(ex, "Failed to open menu");
			throw;
		}
	}

	private EventHandler OnClosing() => (s, e) => CloseMenu();
	public void CloseMenu()
	{
		if(Menus.Count is 0)
		{
			throw new Exception("No window is open");
		}
		var curr = Menus.Peek();
		curr.Menu.Closed -= OnClosing();
		if(PresentationSource.FromVisual(curr.Menu) != null)
		{
			curr.Menu.Close();
		}
		Menus.Pop();
	}
}