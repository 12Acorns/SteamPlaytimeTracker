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
		get => field;
		private set
		{
			value = field;
			PropertyChanged.OnPropertyChanged(this);
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public static ProgressSpinnerBar Create(TimeSpan timeForOneRotation, (int Width, int Height) spinnerDimensions = default)
	{
		var progressSpinner = new ProgressSpinnerBar
		{
			Spinner = ProgressSpinner.CreateSpinner(timeForOneRotation, spinnerDimensions)
		};
		return progressSpinner;
	}
}
