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
	public static readonly DependencyProperty InstallPlaceholderProperty =
		DependencyProperty.RegisterAttached("InstallPlaceholder", typeof(string), typeof(FolderSelectView), new PropertyMetadata(string.Empty));
	public static readonly DependencyProperty InstallIndicatorProperty =
		DependencyProperty.RegisterAttached("InstallIndicator", typeof(string), typeof(FolderSelectView), new PropertyMetadata(string.Empty));
	public static readonly DependencyProperty InstallFolderTitleProperty =
		DependencyProperty.RegisterAttached("InstallFolderTitle", typeof(string), typeof(FolderSelectView), new PropertyMetadata(string.Empty));
	public static readonly DependencyProperty InstallFolderNotSelectedWarningProperty =
		DependencyProperty.RegisterAttached("InstallFolderNotSelectedWarning", typeof(string), typeof(FolderSelectView), new PropertyMetadata(string.Empty));

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
		get => (string)GetValue(InstallPlaceholderProperty);
		set => SetValue(InstallPlaceholderProperty, value);
	}
	public string InstallIndicator
	{
		get => (string)GetValue(InstallIndicatorProperty);
		set => SetValue(InstallIndicatorProperty, value);
	}
	public string InstallFolderTitle
	{
		get => (string)GetValue(InstallFolderTitleProperty);
		set => SetValue(InstallFolderTitleProperty, value);
	}
	public string InstallFolderNotSelectedWarning
	{
		get => (string)GetValue(InstallFolderNotSelectedWarningProperty);
		set => SetValue(InstallFolderNotSelectedWarningProperty, value);
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	private void btn_OpenInstallFolder_Click(object sender, RoutedEventArgs e)
	{
		var openFolder = new OpenFolderDialog()
		{
			Title = InstallFolderTitle,
			Multiselect = false
		};
		if(!(openFolder.ShowDialog() ?? false))
		{
			MessageBox.Show(InstallFolderNotSelectedWarning, "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
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
