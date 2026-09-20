using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.MVVM.View.UserControls.Modern.DatePicker;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.Modern;

public class ModernCalanderSwitch : ToggleButton
{
	static ModernCalanderSwitch()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(ModernCalanderSwitch), new FrameworkPropertyMetadata(typeof(ModernCalanderSwitch)));
	}
}
