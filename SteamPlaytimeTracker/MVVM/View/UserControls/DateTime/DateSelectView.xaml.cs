using SteamPlaytimeTracker.MVVM.View.UserControls.DateTime.DateViews;
using SteamPlaytimeTracker.Extensions;
using System.Windows.Controls;
using System.ComponentModel;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.DateTime;

public partial class DateSelectView : UserControl, INotifyPropertyChanged
{
	private IDateView _dateView = null!;

	public DateSelectView()
	{
		InitializeComponent();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public IDateView DateView
	{
		get => _dateView;
		set
		{
			_dateView = value;
			PropertyChanged.OnPropertyChanged(this);
		}
	}
}
