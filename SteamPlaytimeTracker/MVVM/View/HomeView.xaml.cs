using SteamPlaytimeTracker.MVVM.View.UserControls.Steam;
using SteamPlaytimeTracker.MVVM.ViewModel;
using System.Windows.Controls;
using WpfToolkit.Controls;

namespace SteamPlaytimeTracker.MVVM.View;

public partial class HomeView : UserControl
{
	public HomeView()
	{
		InitializeComponent();
		Loaded += (s, e) =>
		{
			ListBox_SizeChanged(lb_SteamEntries, null!);
		};
	}

	private void ListBox_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
	{
		if(sender is not ListBox lb)
		{
			return;
		}
		var vp = FindVisualChild<VirtualizingWrapPanel>(lb);
		if(vp is null)
		{
			return;
		}
		var sX = vp.ActualWidth - vp.Margin.Right - vp.Margin.Left;
		var nItemsInRow = (int)Math.Max(1, sX / SteamCapsule.BaseWidth);
		var newItemWidth = sX / nItemsInRow;
		if(DataContext is not HomeViewModel vm)
		{
			return;
		}
		vm.UniformWidth = newItemWidth;
		vm.UniformHeight = newItemWidth * SteamCapsule.HeightScaleFactor;
		lb.InvalidateArrange();
	}
}
