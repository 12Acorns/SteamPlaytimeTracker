using SteamPlaytimeTracker.Core;
using System.Windows;

namespace SteamPlaytimeTracker.Services.Menu;

internal sealed class MenuService : ObservableObject, IMenuService
{
	public delegate (MenuModel Model, Window Menu) ModelFactory(Type model, Type menu, params object[] @params);
	
	private readonly ModelFactory _menuModelFactory;

	public MenuService(ModelFactory menuModelFactory) => _menuModelFactory = menuModelFactory ?? throw new ArgumentNullException(nameof(menuModelFactory));

	public Stack<(MenuModel Model, Window Menu)> Menus { get; } = [];

	public void ShowMenu<TModel, TMenu>(bool domainMenu = false, params object[] @params)
		where TModel : MenuModel
		where TMenu : Window, IMenuWindow<TModel>
	{
		Menus.Push(_menuModelFactory(typeof(TModel), typeof(TMenu), @params ?? []));
		var curr = Menus.Peek();
		curr.Menu.Title = curr.Model.Title;
		if(domainMenu)
		{
			curr.Menu.ShowDialog();
		}
		else
		{
			curr.Menu.Show();
		}
		curr.Menu.Closed += OnClosing();
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