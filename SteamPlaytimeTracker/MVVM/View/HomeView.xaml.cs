using SteamPlaytimeTracker.MVVM.View.UserControls.Steam;
using SteamPlaytimeTracker.MVVM.ViewModel;
using System.Windows;
using System.Windows.Controls;
using WpfToolkit.Controls;

namespace SteamPlaytimeTracker.MVVM.View;

public partial class HomeView : UserControl
{
	private static HomeView? _instance;

	public HomeView()
	{
		InitializeComponent();
		_instance = this;
		Loaded += (s, e) =>
		{
			ListBox_SizeChanged(lb_SteamEntries, null!);
		};
	}

	private void ListBox_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if(sender is not ListBox lb || DataContext is not HomeViewModel vm)
		{
			return;
		}
		var vp = FindVisualChild<VirtualizingWrapPanel>(lb);
		if(vp is null)
		{
			return;
		}
		var sX = vp.ActualWidth - vp.Margin.Right - vp.Margin.Left - 0.5d;
		var nItemsInRow = (int)Math.Max(1, sX / SteamCapsule.BaseWidth);
		var newItemWidth = sX / nItemsInRow;
		vm.UniformWidth = newItemWidth;
		vm.UniformHeight = newItemWidth * SteamCapsule.HeightScaleFactor;
		lb.InvalidateArrange();
	}

	public static void RefreshArrangement() => _instance?.ListBox_SizeChanged(_instance.lb_SteamEntries, null!);
}
