using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.Extensions;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.DateTime;

public partial class DateHeader : UserControl, INotifyPropertyChanged
{
	private static readonly DependencyProperty _headerClickCommand =
		DependencyProperty.RegisterAttached("HeaderClickCommand", typeof(RelayCommand), typeof(DateHeader), new PropertyMetadata(null));
	private static readonly DependencyProperty _fontSizeProperty =
		DependencyProperty.Register("HeaderFontSize", typeof(double), typeof(DateHeader), new PropertyMetadata(16d));

	private string _headerText = string.Empty;

	public DateHeader()
	{
		InitializeComponent();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public string HeaderText
	{
		get => _headerText;
		set
		{
			_headerText = value;
			PropertyChanged.OnPropertyChanged(this);
		}
	}
	public double HeaderFontSize
	{
		get => (double)GetValue(_fontSizeProperty);
		set => SetValue(_fontSizeProperty, value);
	}
	public RelayCommand HeaderClickCommand
	{
		get => (RelayCommand)GetValue(_headerClickCommand);
		set => SetValue(_headerClickCommand, value);
	}
}
