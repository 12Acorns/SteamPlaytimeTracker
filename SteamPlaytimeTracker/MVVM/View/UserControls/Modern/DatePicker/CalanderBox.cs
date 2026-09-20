using System.Windows.Controls;
using System.Windows;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.Modern.DatePicker;

public class CalanderBox : ListBox
{
    static CalanderBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CalanderBox), new FrameworkPropertyMetadata(typeof(CalanderBox)));
    }
}