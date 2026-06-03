using SteamPlaytimeTracker.Utility.ProcessTracking;
using SteamPlaytimeTracker.Extensions;
using System.Windows.Controls;
using System.ComponentModel;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.Process;

public partial class ProcessInfoTrackableUserControl : UserControl, INotifyPropertyChanged
{
	private const string ProcessInfoTextTemplate = "Application Name: {0}\nPath: {1}\nTracked: {2}";
	private readonly ProcessTrackingInfo _info = default!;

	public event PropertyChangedEventHandler? PropertyChanged;

	internal ProcessInfoTrackableUserControl(ProcessTrackingInfo info)
	{
		InitializeComponent();
		DataContext = this;
		_info = info;
		ProcessInfoText = string.Format(ProcessInfoTextTemplate, _info.Info.ProcessName, _info.Info.ApplicationPath, _info.Tracked);
		ToggleState = _info.Tracked;
	}

	public ProcessInfoTrackableUserControl()
	{
		InitializeComponent();
	}

	public string ProcessInfoText 
	{ 
		get; 
		set
		{
			field = value;
			PropertyChanged.OnPropertyChanged(this);
		}
	} = ProcessInfoTextTemplate;
	public bool? ToggleState
	{
		get;
		set
		{
			field = value;
			PropertyChanged.OnPropertyChanged(this);
			ProcessInfoText = string.Format(ProcessInfoTextTemplate, _info.Info.ProcessName, _info.Info.ApplicationPath, field);
		}
	}
}
