using SteamPlaytimeTracker.Extensions;
using System.ComponentModel;
using System.Windows;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.Progress;

public partial class ProgressSpinnerBar : Window, INotifyPropertyChanged
{
	public ProgressSpinnerBar()
	{
		InitializeComponent();
		DataContext = this;
	}

	public ProgressSpinner? Spinner
	{
		get;
		private set
		{
			field = value;
			PropertyChanged.OnPropertyChanged(this);
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	private void pBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => DragMove();
}
