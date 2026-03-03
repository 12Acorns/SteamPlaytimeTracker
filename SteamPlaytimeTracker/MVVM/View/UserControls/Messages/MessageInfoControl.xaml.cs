using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.Messages;

public partial class MessageInfoControl : UserControl
{
    private static readonly DependencyProperty _iconDataProperty = DependencyProperty.Register(
        nameof(IconData), typeof(Geometry), typeof(MessageInfoControl), new PropertyMetadata(Geometry.Empty));
	private static readonly DependencyProperty _messageProperty = DependencyProperty.Register(
        nameof(Message), typeof(string), typeof(MessageInfoControl), new PropertyMetadata(string.Empty));
    private static readonly DependencyProperty _iconColourMultiplierProperty = DependencyProperty.Register(
        nameof(IconColourMultiplier), typeof(SolidColorBrush), typeof(MessageInfoControl), new PropertyMetadata(Brushes.White));


	public MessageInfoControl()
    {
        InitializeComponent();
	}

    public Geometry IconData 
    {
        get => (Geometry)GetValue(_iconDataProperty);
        set => SetValue(_iconDataProperty, value);
	}
    public SolidColorBrush IconColourMultiplier
    {
        get => (SolidColorBrush)GetValue(_iconColourMultiplierProperty);
        set => SetValue(_iconColourMultiplierProperty, value);
	}
	public string Message
    {
        get => (string)GetValue(_messageProperty);
        set => SetValue(_messageProperty, value);
	}
}
