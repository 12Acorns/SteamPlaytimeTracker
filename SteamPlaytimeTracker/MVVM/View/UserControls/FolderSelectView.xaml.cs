using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace SteamPlaytimeTracker.MVVM.View.UserControls;

public partial class FolderSelectView : UserControl, INotifyPropertyChanged
{
	public static readonly DependencyProperty FolderPathProperty =
		DependencyProperty.Register("FolderPath", typeof(string), typeof(FolderSelectView), new PropertyMetadata(string.Empty));

	private string _installFolderTitle = string.Empty;
	private string _installFolderNotSelectedWarning = string.Empty;
	private string _installPlaceholder = string.Empty;
	private string _installIndicator = string.Empty;

	public FolderSelectView()
	{
		DataContext = this;
		InitializeComponent();
	}

	public string FolderPath
	{
		get => (string)GetValue(FolderPathProperty);
		set => SetValue(FolderPathProperty, value);
	}
	public string InstallPlaceholder
	{
		get => _installPlaceholder;
		set
		{
			_installPlaceholder = value;
			OnPropertyChanged();
		}
	}
	public string InstallIndicator
	{
		get => _installIndicator;
		set
		{
			_installIndicator = value;
			OnPropertyChanged();
		}
	}
	public string InstallFolderTitle
	{
		get => _installFolderTitle;
		set
		{
			_installFolderTitle = value;
			OnPropertyChanged();
		}
	}
	public string InstallFolderNotSelectedWarning
	{
		get => _installFolderNotSelectedWarning;
		set
		{
			_installFolderNotSelectedWarning = value;
			OnPropertyChanged();
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	private void btn_OpenInstallFolder_Click(object sender, System.Windows.RoutedEventArgs e)
	{
		var openFolder = new OpenFolderDialog()
		{
			Title = _installFolderTitle,
			Multiselect = false
		};
		if(!(openFolder.ShowDialog() ?? false))
		{
			MessageBox.Show(_installFolderNotSelectedWarning, "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
			return;
		}
		tf_AppInstall.Text = openFolder.FolderName;
		tb_AppInstall.Visibility = Visibility.Hidden;
	}
	private void tf_AppInstall_TextChanged(object sender, TextChangedEventArgs e)
	{
		if(string.IsNullOrWhiteSpace(tf_AppInstall.Text))
		{
			tb_AppInstall.Visibility = Visibility.Visible;
			return;
		}
		tb_AppInstall.Visibility = Visibility.Hidden;
	}

	private void OnPropertyChanged([CallerMemberName] string? propName = null) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}
