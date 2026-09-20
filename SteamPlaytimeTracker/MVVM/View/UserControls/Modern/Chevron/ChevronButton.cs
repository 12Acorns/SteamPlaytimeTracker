using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;
using System.Reflection;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.Modern.Chevron;

public class ChevronButton : Button
{
	static ChevronButton()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(ChevronButton), new FrameworkPropertyMetadata(typeof(ChevronButton)));
	}

	public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register(
		nameof(Direction), typeof(ChevronButtonDirection), typeof(ChevronButton), new PropertyMetadata(ChevronButtonDirection.Down));
	public static readonly DependencyProperty HoverColourProperty = DependencyProperty.Register(
		nameof(HoverColour), typeof(Brush), typeof(ChevronButton), new PropertyMetadata(new SolidColorBrush(Colors.White)));
	public static readonly DependencyProperty PressedColourProperty = DependencyProperty.Register(
		nameof(PressedColour), typeof(Brush), typeof(ChevronButton), new PropertyMetadata(new SolidColorBrush(Colors.White)));

	public ChevronButtonDirection Direction
	{
		get { return (ChevronButtonDirection)GetValue(DirectionProperty); }
		set { SetValue(DirectionProperty, value); }
	}
	public Brush HoverColour
	{
		get { return (Brush)GetValue(HoverColourProperty); }
		set { SetValue(HoverColourProperty, value); }
	}
	public Brush PressedColour
	{
		get { return (Brush)GetValue(PressedColourProperty); }
		set { SetValue(PressedColourProperty, value); }
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		var path = (Path)GetTemplateChild("Path");

		AddHandler(MouseEnterEvent, new RoutedEventHandler((_, _) => path.SetCurrentValue(Shape.FillProperty, HoverColour)), true);
		AddHandler(MouseLeaveEvent, new RoutedEventHandler((_, _) => path.InvalidateProperty(Shape.FillProperty)), true);

		AddHandler(MouseLeftButtonDownEvent, new RoutedEventHandler((_, _) => path.SetCurrentValue(Shape.FillProperty, PressedColour)), true);
		AddHandler(MouseLeftButtonUpEvent, new RoutedEventHandler((_, _) => path.InvalidateProperty(Shape.FillProperty)), true);
	}
}
public enum ChevronButtonDirection
{
	Up,
	Down,
	Left,
	Right
}