using SteamPlaytimeTracker.Core;
using System.Windows;

namespace SteamPlaytimeTracker.Services.Menu;

interface IMenuService
{
	public Stack<(MenuModel Model, Window Menu)> Menus { get; }

	public event Action<MenuModel, Window> OnLoaded;

	void ShowMenu<TModel, TMenu>(bool domainMenu = false, params object[] @params) 
		where TModel : MenuModel
		where TMenu : Window, IMenuWindow<TModel>;
	void CloseMenu();
}