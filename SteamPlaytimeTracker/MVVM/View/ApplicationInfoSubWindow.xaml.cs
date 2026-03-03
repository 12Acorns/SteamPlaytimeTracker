using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.MVVM.ViewModel.Window;
using System.Windows;

namespace SteamPlaytimeTracker.MVVM.View;

public partial class ApplicationInfoSubWindow : Window, IMenuWindow<ApplicationInfoWindowModel>
{
    public ApplicationInfoSubWindow()
    {
        InitializeComponent();
    }

	private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		BorderThickness = WindowState is WindowState.Maximized ? new Thickness(6) : new Thickness(0);
	}
}
