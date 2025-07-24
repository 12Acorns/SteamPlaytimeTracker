using SteamPlaytimeTracker.Steam;
using System.ComponentModel;
using Microsoft.Win32;
using System.Windows;
using System.Runtime.CompilerServices;
using System.IO;

namespace SteamPlaytimeTracker;

public partial class MainWindow : Window, INotifyPropertyChanged
{
	private string _steamInstallPath = "";

    public MainWindow()
    {
		DataContext = this;
        InitializeComponent();
    }

	public event PropertyChangedEventHandler? PropertyChanged;

	public string SteamInstallPath
	{
		get => _steamInstallPath;
		set
		{
			_steamInstallPath = value;
			OnPropertyChanged();
		}
	}

	private async void btn_QuerySteamGames_Click(object sender, RoutedEventArgs e)
	{
        var res = await SteamRequest.GetAppListAsync().ConfigureAwait(false);
	}
	private void btn_ConfirmSettings_Click(object sender, RoutedEventArgs e)
	{
		VerifySettings();
	}
	private void tf_SteamInstall_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
	{
		if(string.IsNullOrWhiteSpace(tf_SteamInstall.Text))
		{
			tb_SteamInstall.Visibility = Visibility.Visible;
			return;
		}
		tb_SteamInstall.Visibility = Visibility.Hidden;
	}
	private void btn_OpenSteamInstallFolder_Click(object sender, RoutedEventArgs e)
	{
		var openFolder = new OpenFolderDialog()
		{
			Title = "Select Steam Installation Folder",
			Multiselect = false
		};
		if(!(openFolder.ShowDialog() ?? false))
		{
			MessageBox.Show("Steam Install not Selected.", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
			return;
		}
		tf_SteamInstall.Text = openFolder.FolderName;
		tb_SteamInstall.Visibility = Visibility.Hidden;
	}

	private void OnPropertyChanged([CallerMemberName] string? propName = null) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
	private void VerifySettings()
	{
		if(!Directory.Exists(tf_SteamInstall.Text))
		{
			MessageBox.Show("Steam directory not found. Please enter your steam installation directory.", "Invalid Location Set!",
				MessageBoxButton.OK, MessageBoxImage.Warning);
			return;
		}
		if(!File.Exists(Path.Combine(tf_SteamInstall.Text, "steam.exe")))
		{
			MessageBox.Show("Steam executable not found under entered directory. Ensure the correct steam installation directory has been " +
				"entered", "No Steam Executable Found!", MessageBoxButton.OK, MessageBoxImage.Warning);
			return;
		}

		MessageBox.Show("Successfully saved settings.", "Success :D", MessageBoxButton.OK, MessageBoxImage.Information);
	}
}