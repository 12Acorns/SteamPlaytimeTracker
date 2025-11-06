using System.Windows.Controls;

namespace SteamPlaytimeTracker.MVVM.View.UserControls.Progress;

// https://robcrocombe.com/2014/03/16/how-to-progress-spinner-wpf/
public partial class ProgressSpinner : UserControl
{
	public ProgressSpinner()
	{
		InitializeComponent();
	}

	public static ProgressSpinner CreateSpinner(TimeSpan timeForOneRotation)
	{
		var spinner = new ProgressSpinner();
		var sbRoot = spinner.StoryBoardRoot;
		var sb = sbRoot.Storyboard.Clone();
		sb.Duration = timeForOneRotation;
		sbRoot.Storyboard = sb;
		return spinner;
	}
}
