using System.Windows;
using System.Windows.Input;

namespace SteamPlaytimeTracker.MVVM.View;

public partial class HomeWindow : Window
{
	public static event EventHandler<MouseButtonEventArgs>? OnMouseDownA;

	public HomeWindow()
	{
		InitializeComponent();
	}


	private void Window_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		OnMouseDownA?.Invoke(sender, e);
	}
}