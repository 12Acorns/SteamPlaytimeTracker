using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.MVVM.View.UserControls.Modern.Chevron;
using SteamPlaytimeTracker.MVVM.View.UserControls.Modern.DatePicker;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.Modern;

internal class ModernDatePicker : Control, INotifyPropertyChanged
{
	/// <summary>
	/// Look at <see cref="System.DayOfWeek"/> enum values
	/// </summary>
	private const int DayOfMonthEndValue = 6;

	static ModernDatePicker()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(ModernDatePicker), new FrameworkPropertyMetadata(typeof(ModernDatePicker)));
	}

	public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register(
		nameof(SelectedDate), typeof(DateTime?), typeof(ModernDatePicker), new PropertyMetadata(null));
	public static readonly DependencyProperty CurrentMonthProperty = DependencyProperty.Register(
		nameof(CurrentMonth), typeof(DateTime), typeof(ModernDatePicker), new PropertyMetadata(default));
	public static readonly DependencyProperty KeepPopupOpenProperty = DependencyProperty.Register(
		nameof(KeepPopupOpen), typeof(bool), typeof(ModernDatePicker), new PropertyMetadata(false));
	public static readonly DependencyProperty BlankDateTextProperty = DependencyProperty.Register(
		nameof(BlankDateText), typeof(string), typeof(ModernDatePicker), new PropertyMetadata("Enter date here"));
	public static readonly DependencyProperty MinimumAllowedDateProperty = DependencyProperty.Register(
		nameof(MinimumAllowedDate), typeof(DateTime?), typeof(ModernDatePicker), new PropertyMetadata(default));
	public static readonly DependencyProperty MaximumAllowedDateProperty = DependencyProperty.Register(
		nameof(MaximumAllowedDate), typeof(DateTime?), typeof(ModernDatePicker), new PropertyMetadata(default));

	private ModernCalanderSwitch _calenderSwitch;
	private CalanderBox _calenderListBox;
	private Popup _calenderPopup;

	public event PropertyChangedEventHandler? PropertyChanged;

	public DateTime? SelectedDate
	{
		get { return (DateTime?)GetValue(SelectedDateProperty); }
		set { SetValue(SelectedDateProperty, value); }
	}
	public DateTime? MaximumAllowedDate
	{
		get { return (DateTime?)GetValue(MaximumAllowedDateProperty); }
		set { SetValue(MaximumAllowedDateProperty, value); }
	}
	public DateTime? MinimumAllowedDate
	{
		get { return (DateTime?)GetValue(MinimumAllowedDateProperty); }
		set { SetValue(MinimumAllowedDateProperty, value); }
	}
	public DateTime CurrentMonth
	{
		get { return (DateTime)GetValue(CurrentMonthProperty); }
		set { SetValue(CurrentMonthProperty, value); }
	}
	public bool KeepPopupOpen
	{
		get { return (bool)GetValue(KeepPopupOpenProperty); }
		set { SetValue(KeepPopupOpenProperty, value); }
	}
	public string? BlankDateText
	{
		get { return (string?)GetValue(BlankDateTextProperty); }
		set { SetValue(BlankDateTextProperty, value); }
	}
	public string DisplayText
	{
		get;
		set
		{
			field = value;
			PropertyChanged.OnPropertyChanged(this);
		}
	} = string.Empty;

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		var calenderChevronLeft = (ChevronButton)GetTemplateChild("PART_CalenderChevronLeft");
		var calenderChevronRight = (ChevronButton)GetTemplateChild("PART_CalenderChevronRight");
		_calenderPopup = (Popup)GetTemplateChild("PART_Popup");
		_calenderSwitch = (ModernCalanderSwitch)GetTemplateChild("PART_CalanderSwitch");
		_calenderListBox = (CalanderBox)GetTemplateChild("PART_CalanderBox");

		ArgumentNullException.ThrowIfNull(calenderChevronLeft);
		ArgumentNullException.ThrowIfNull(calenderChevronRight);
		ArgumentNullException.ThrowIfNull(_calenderPopup);
		ArgumentNullException.ThrowIfNull(_calenderSwitch);
		ArgumentNullException.ThrowIfNull(_calenderListBox);

		_calenderPopup.Closed += _calenderPopup_Closed;
		_calenderSwitch.Click += _switch_OnClick;
		_calenderListBox.MouseLeftButtonUp += _calenderListBox_MouseLeftButtonDown;

		calenderChevronLeft.Click += (_, _) => ShiftMonthBy(-1);
		calenderChevronRight.Click += (_, _) => ShiftMonthBy(1);

		DisplayText = SelectedDate?.ToString("d") ?? BlankDateText ?? string.Empty;
	}
	private void ShiftMonthBy(int months) => GenerateCalender(CurrentMonth.AddMonths(months));

	private void _calenderPopup_Closed(object? sender, EventArgs e)
	{
		_calenderSwitch?.IsChecked = IsMouseOver;
	}

	private void _calenderListBox_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
	{
		if(_calenderListBox?.SelectedItem is not CalanderBoxItem selectedItem)
		{
			return;
		}
		SelectedDate = selectedItem.Date.ToDateTime(new TimeOnly(0, 0, 0));
		GenerateCalender(SelectedDate.Value);
		_calenderPopup.IsOpen = KeepPopupOpen;
		DisplayText = SelectedDate?.ToString("d") ?? BlankDateText ?? string.Empty;
	}
	private void _switch_OnClick(object sender, RoutedEventArgs e)
	{
		if(_calenderSwitch.IsChecked ?? false)
		{
			_calenderPopup.IsOpen = true;
			GenerateCalender(SelectedDate ?? DateTime.Now);
		}
	}
	private void GenerateCalender(DateTime referenceDate)
	{
		if(CurrentMonth.CompareTo(referenceDate, DateComparisonOptions.YearMonthComparison))
		{
			return;
		}
		CurrentMonth = referenceDate;

		_calenderListBox.Items.Clear();
		var firstDayOfMonth = referenceDate.FirstDayOfMonth();
		var lastDayOfMonth = referenceDate.LastDayOfMonth();
		// DoM -> Date of Month
		var firstDoMOffset = (int)firstDayOfMonth.DayOfWeek;
		var lastDoMOffset = DayOfMonthEndValue - (int)lastDayOfMonth.DayOfWeek;
		var firstDay = firstDayOfMonth.AddDays(-firstDoMOffset);
		var lastDay = lastDayOfMonth.AddDays(lastDoMOffset);
		for(var day = firstDay; day <= lastDay; day = day.AddDays(1))
		{
			if(MinimumAllowedDate.HasValue && day < MinimumAllowedDate.Value ||
			   MaximumAllowedDate.HasValue && day > MaximumAllowedDate.Value)
			{
				_calenderListBox.Items.Add(new Label());
				continue;
			}
			var calenderItem = new CalanderBoxItem()
			{
				Content = day.Day,
				Date = day.DateOnly,
				IsCurrentDate = day.DateOnly == DateOnly.Now,
				IsCurrentMonth = day.Month == referenceDate.Month
			};
			_calenderListBox.Items.Add(calenderItem);
		}
		if(SelectedDate.HasValue)
		{
			_calenderListBox.SelectedValue = SelectedDate.Value.ToString("yyyyMMdd");
		}
	}
}