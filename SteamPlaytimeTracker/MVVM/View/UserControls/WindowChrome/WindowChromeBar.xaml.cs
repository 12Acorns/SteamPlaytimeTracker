
using SteamPlaytimeTracker.Core;
using System.Windows.Controls;
using System.Windows;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.WindowChrome;

public partial class WindowChromeBar : UserControl
{
    public WindowChromeBar()
    {
        InitializeComponent();
        DataContext = this;

		MinimizeCommand = new(_ =>
		{
			Application.Current.MainWindow.WindowState = WindowState.Minimized;
		});
		ToggleSizeCommand = new(_ =>
		{
			Application.Current.MainWindow.WindowState = Application.Current.MainWindow.WindowState switch
			{
				WindowState.Maximized => WindowState.Normal,
				WindowState.Normal => WindowState.Maximized,
				_ => WindowState.Normal
			};
		});
		QuitApplicationCommand = new(_ =>
		{
			Application.Current.MainWindow.Close();
		});
	}

	public RelayCommand MinimizeCommand { get; }
	public RelayCommand ToggleSizeCommand { get; }
	public RelayCommand QuitApplicationCommand { get; }
}