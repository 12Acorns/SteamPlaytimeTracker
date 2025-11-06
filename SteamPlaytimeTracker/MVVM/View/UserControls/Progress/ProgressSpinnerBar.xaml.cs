using SteamPlaytimeTracker.Extensions;
using System.ComponentModel;
using System.Windows;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.Progress;

public partial class ProgressSpinnerBar : Window, INotifyPropertyChanged
{
	private ProgressSpinnerBar()
	{
		InitializeComponent();
	}

	public ProgressSpinner? Spinner
	{
		get;
		private set
		{
			value = field;
			PropertyChanged.OnPropertyChanged(this);
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public static ProgressSpinnerBar Create(TimeSpan timeForOneRotation, (int Width, int Height) dimensions = default)
	{
		if(dimensions == default)
		{
			dimensions = (256, 164);
		}
		var progressSpinner = new ProgressSpinnerBar
		{
			Spinner = ProgressSpinner.CreateSpinner(timeForOneRotation),
			MaxWidth = dimensions.Width,
			MaxHeight = dimensions.Height
		};
		return progressSpinner;
	}

	private void pBar_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		DragMove();
	}
}
