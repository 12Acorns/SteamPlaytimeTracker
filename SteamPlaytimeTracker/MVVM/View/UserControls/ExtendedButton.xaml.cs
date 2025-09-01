using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SteamPlaytimeTracker.MVVM.View.UserControls;

public partial class ExtendedButton : Button
{
	private static readonly Brush _defaultHoverBorderValue = (Brush)new BrushConverter().ConvertFromString("#FF3C7FB1")!;
	private static readonly Brush _defaultPressedBorderValue = (Brush)new BrushConverter().ConvertFromString("#FF2C628B")!;
	private static readonly Brush _defaultHoverBackgroundValue = (Brush)new BrushConverter().ConvertFromString("#FFBEE6FD")!;
	private static readonly Brush _defaultPressedBackgroundValue = (Brush)new BrushConverter().ConvertFromString("#FFC4E5F6")!;

	public static readonly DependencyProperty HoverBorderProperty = DependencyProperty.Register(
		"HoverBorder", typeof(Brush), typeof(ExtendedButton), new PropertyMetadata(_defaultHoverBorderValue));
	public static readonly DependencyProperty PressedBorderProperty = DependencyProperty.Register(
		"PressedBorder", typeof(Brush), typeof(ExtendedButton), new PropertyMetadata(_defaultPressedBorderValue));
	public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register(
		"HoverBackground", typeof(Brush), typeof(ExtendedButton), new PropertyMetadata(_defaultHoverBackgroundValue));
	public static readonly DependencyProperty PressedBackgroundProperty = DependencyProperty.Register(
		"PressedBackground", typeof(Brush), typeof(ExtendedButton), new PropertyMetadata(_defaultPressedBackgroundValue));

	public ExtendedButton()
	{
		InitializeComponent();
	}

	public Brush HoverBorder
	{
		get => (Brush)GetValue(HoverBorderProperty);
		set => SetValue(HoverBorderProperty, value);
	}
	public Brush PressedBorder
	{
		get => (Brush)GetValue(PressedBorderProperty);
		set => SetValue(PressedBorderProperty, value);
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
