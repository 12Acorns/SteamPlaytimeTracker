using System.Windows.Controls;
using System.Windows;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.Modern.DatePicker;

public class DayOfWeek : Label
{
    static DayOfWeek()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DayOfWeek), new FrameworkPropertyMetadata(typeof(DayOfWeek)));
    }
}