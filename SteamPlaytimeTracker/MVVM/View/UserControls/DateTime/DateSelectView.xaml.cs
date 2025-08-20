using SteamPlaytimeTracker.Core;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.MVVM.View.UserControls.DateTime.DateViews;
using System.ComponentModel;
using System.Windows.Controls;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.DateTime;

public partial class DateSelectView : UserControl, INotifyPropertyChanged
{
	private IDateView _dateView = null!;

	public DateSelectView()
	{
		InitializeComponent();
		DateView = DateViewFactory.Get(DateViewSelected.MonthView).AsT1.Value;
		DataContext = this;
		HeaderClickCommand = new RelayCommand(btnHeader =>
		{
			// Think of it like you are propogating up a list on each click
			// Day View -> Month View -> Year View
			// Where when you click the header on Day View you go to Month View
			// And so on until you reach the head which is Year View
			DateView = DateView switch
			{
				DateYearView _ => DateView,
				DateMonthView _ => SwitchToYearView(btnHeader!),
				DateDayView _ => SwitchToDayView(btnHeader!),
				_ => throw new InvalidEnumArgumentException()
			};
		});
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public RelayCommand HeaderClickCommand { get; set; }

	public IDateView DateView
	{
		get => _dateView;
		set
		{
			_dateView = value;
			PropertyChanged.OnPropertyChanged(this);
		}
	}
	public DateHeader HeaderContainer
	{
		get => hdr_Header;
		set
		{
			hdr_Header = value;
			PropertyChanged.OnPropertyChanged(this);
		}
	}

	private static IDateView SwitchToYearView(object o)
	{
		return DateViewFactory.Get(DateViewSelected.YearView).AsT1.Value;
	}
	private static IDateView SwitchToDayView(object o)
	{
		return DateViewFactory.Get(DateViewSelected.DayView).AsT1.Value;
	}
}
