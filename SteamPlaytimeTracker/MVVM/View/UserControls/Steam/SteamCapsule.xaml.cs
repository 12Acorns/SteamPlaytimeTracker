using Microsoft.Extensions.DependencyInjection;
using SteamPlaytimeTracker.Utility.Cache;
using SteamPlaytimeTracker.Extensions;
using System.Windows.Media.Imaging;
using SteamPlaytimeTracker.Core;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.Steam;

public partial class SteamCapsule : UserControl, INotifyPropertyChanged
{
	public const int BaseWidth = 120;
	public const double HeightScaleFactor = 1.5d;

	private static readonly DependencyProperty _imageUrlProperty =
		DependencyProperty.Register(nameof(ImageUrl), typeof(string), typeof(SteamCapsule),
			new PropertyMetadata(OnImageUrlChanged));
	private static readonly DependencyProperty _titleProperty =
		DependencyProperty.Register(nameof(Title), typeof(string), typeof(SteamCapsule));
	private static readonly DependencyProperty _command =
		DependencyProperty.Register(nameof(Command), typeof(RelayCommand), typeof(SteamCapsule));
	private static readonly DependencyProperty _commandParameterProperty =
		DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(SteamCapsule));

	private readonly ICacheManager _imageCache;

	public SteamCapsule()
	{
		InitializeComponent();
		_imageCache = App.ServiceProvider.GetRequiredService<ICacheManager>();
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
		get => GetValue(_commandParameterProperty);
		set => SetValue(_commandParameterProperty, value);
	}

	public BitmapImage CapsuleImage
	{
		get => field;
		set
		{
			field = value;
			PropertyChanged.OnPropertyChanged(this);
		}
	}

	private static void OnImageUrlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var capsule = (SteamCapsule)d;
		var url = (string)e.NewValue;
		if(capsule._imageCache.TryGet<BitmapImage>(url, out var bmp))
		{
			capsule.CapsuleImage = bmp;
			return;
		}
		bmp = LoadBitmap(url);
		capsule._imageCache.Set(url, bmp, TimeSpan.FromHours(2));
		capsule.CapsuleImage = bmp;
	}
	private static BitmapImage LoadBitmap(string url)
	{
		var bmp = new BitmapImage();
		bmp.BeginInit();
		bmp.UriSource = new Uri(url, UriKind.Absolute);
		bmp.CacheOption = BitmapCacheOption.OnDemand;
		bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
		bmp.EndInit();
		return bmp;
	}
}
