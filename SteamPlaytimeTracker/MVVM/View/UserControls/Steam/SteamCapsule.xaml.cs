using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using SteamPlaytimeTracker.Core;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.Steam;

public partial class SteamCapsule : UserControl, INotifyPropertyChanged
{
	private static readonly DependencyProperty _imageUrlProperty =
		DependencyProperty.Register(nameof(ImageUrl), typeof(string), typeof(SteamCapsule));
	private static readonly DependencyProperty _titleProperty =
		DependencyProperty.Register(nameof(Title), typeof(string), typeof(SteamCapsule));
	private static readonly DependencyProperty _command =
		DependencyProperty.Register(nameof(Command), typeof(RelayCommand), typeof(SteamCapsule));
	private static readonly DependencyProperty CommandParameterProperty =
		DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(SteamCapsule));

	public SteamCapsule()
	{
		InitializeComponent();
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public string ImageUrl
	{
		get => (string)GetValue(_imageUrlProperty);
		set => SetValue(_imageUrlProperty, value);
	}
	public string Title
	{
		get => (string)GetValue(_titleProperty);
		set => SetValue(_titleProperty, value);
	}
	public RelayCommand Command
	{
		get => (RelayCommand)GetValue(_command);
		set => SetValue(_command, value);
	}
	public object CommandParameter
	{
		get => GetValue(CommandParameterProperty);
		set => SetValue(CommandParameterProperty, value);
	}
}
