using SteamPlaytimeTracker.Extensions;
using System.Windows.Controls;
using System.Windows;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.Modern.DatePicker;

public class CalanderBoxItem : ListBoxItem
{
    public static readonly DependencyProperty IsCurrentMonthProperty =
        DependencyProperty.Register(nameof(IsCurrentMonth), typeof(bool), typeof(CalanderBoxItem), new PropertyMetadata(false));
    public static readonly DependencyProperty IsCurrentDateProperty = DependencyProperty.Register(
        nameof(IsCurrentDate), typeof(bool), typeof(CalanderBoxItem), new PropertyMetadata(false));

    static CalanderBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CalanderBoxItem), new FrameworkPropertyMetadata(typeof(CalanderBoxItem)));
    }

    public bool IsCurrentMonth
	{
		get => (bool)GetValue(IsCurrentMonthProperty); 
        set => SetValue(IsCurrentMonthProperty, value);
	}
    public bool IsCurrentDate
    {
        get { return (bool)GetValue(IsCurrentDateProperty); }
        set { SetValue(IsCurrentDateProperty, value); }
    }


    public DateOnly Date { get; set; }
    public string DateFormat => Date.ToString("yyyyMMdd");
}