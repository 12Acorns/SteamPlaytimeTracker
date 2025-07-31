using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.Extensions;
using System.Windows.Controls;
using System.ComponentModel;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.Steam;

public partial class SteamCapsule : UserControl, INotifyPropertyChanged
{
	private string _capsuleImagePath = string.Empty;
	private string _capsuleName = string.Empty;

	public SteamCapsule(string imagePath, SteamApp steamAppData)
	{
		InitializeComponent();
		DataContext = this;
		CapsuleImagePath = imagePath;
		CapsuleName = steamAppData.Name;
	}
	public SteamCapsule() : this("", new SteamApp(0, "n/a")) { }

	public event PropertyChangedEventHandler? PropertyChanged;

	public string CapsuleImagePath
	{
		get => _capsuleImagePath;
		set
		{
			_capsuleImagePath = value;
			PropertyChanged.OnPropertyChanged(this);
		}
	}
	public string CapsuleName
	{
		get => _capsuleName;
		set
		{
			_capsuleName = value;
			PropertyChanged.OnPropertyChanged(this);
		}
	}
}
