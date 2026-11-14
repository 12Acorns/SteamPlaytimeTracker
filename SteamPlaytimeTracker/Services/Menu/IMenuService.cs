using SteamPlaytimeTracker.Core;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace SteamPlaytimeTracker.Services.Menu;

interface IMenuService
{
	public Stack<(MenuModel Model, Window Menu)> Menus { get; }
	public Window? CurrentMenu
	{
		get
		{
			if(Menus.TryPeek(out var res))
			{
				return res.Menu;
			}
			return null;
		}
	}
	public MenuModel? CurrentModel
	{
		get
		{
			if(Menus.TryPeek(out var res))
			{
				return res.Model;
			}
			return null;
		}
	}

	public event Action<MenuModel, Window> OnLoaded;

	void ShowMenu<TModel, TMenu>(bool domainMenu = false, params object[] @params) 
		where TModel : MenuModel
		where TMenu : Window, IMenuWindow<TModel>;
	void CloseMenu();
	bool TryGetModel<TModel>([NotNullWhen(true)] out TModel? model)
		where TModel : MenuModel
	{
		model = (TModel?)Menus.FirstOrDefault(x => x.Model is TModel).Model;
		return model is not null;
	}
	bool TryGetMenu<TMenu>([NotNullWhen(true)] out TMenu? menu)
		where TMenu : Window
	{
		menu = (TMenu?)Menus.FirstOrDefault(x => x.Menu is TMenu).Menu;
		return menu is not null;
	}
}