using System.ComponentModel;
using System.Windows.Input;
using System.Windows;

namespace SteamPlaytimeTracker.MVVM.View;

public partial class HomeWindow : Window
{
	public static event EventHandler<MouseButtonEventArgs>? OnMouseDownA;

	public HomeWindow()
	{
		InitializeComponent();
	}


	private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e) => OnMouseDownA?.Invoke(sender, e);
	private void Window_Closing(object sender, CancelEventArgs e) => App.Application_Closing(sender, e);
	private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		BorderThickness = WindowState is WindowState.Maximized ? new Thickness(6) : new Thickness(0);
	}
}