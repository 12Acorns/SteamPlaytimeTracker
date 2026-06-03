using SteamPlaytimeTracker.MVVM.ViewModel.Window;
using SteamPlaytimeTracker.Core;
using System.Windows;

namespace SteamPlaytimeTracker.MVVM.View.Windows;

public partial class ProcessTrackingSelectionWindow : Window, IMenuWindow<ProcessTrackingSelectionWindowModel>
{
	public ProcessTrackingSelectionWindow()
	{
		InitializeComponent();
	}

	private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		BorderThickness = WindowState is WindowState.Maximized ? new Thickness(6) : new Thickness(0);
	}
}