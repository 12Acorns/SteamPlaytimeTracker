using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SteamPlaytimeTracker.MVVM.View.UserControls;

public partial class ExtendedButton : Button
{
	private static readonly Brush _defaultHoverBackgroundValue = (Brush)new BrushConverter().ConvertFromString("#FFBEE6FD")!;
	private static readonly Brush _defaultPressedBackgroundValue = (Brush)new BrushConverter().ConvertFromString("#FFC4E5F6")!;

	public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register(
	  "HoverBackground", typeof(Brush), typeof(ExtendedButton), new PropertyMetadata(_defaultHoverBackgroundValue));
	public static readonly DependencyProperty PressedBackgroundProperty = DependencyProperty.Register(
	  "PressedBackground", typeof(Brush), typeof(ExtendedButton), new PropertyMetadata(_defaultPressedBackgroundValue));

	public ExtendedButton()
	{
		InitializeComponent();
	}

	public Brush HoverBackground
	{
		get => (Brush)GetValue(HoverBackgroundProperty);
		set => SetValue(HoverBackgroundProperty, value);
	}
	public Brush PressedBackground
	{
		get => (Brush)GetValue(PressedBackgroundProperty);
		set => SetValue(PressedBackgroundProperty, value);
	}
}
