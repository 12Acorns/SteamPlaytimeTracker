using System.Windows.Media;
using System.Windows;

namespace SteamPlaytimeTracker.Utility;

public static class VisualTreeUtility
{
	public static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
	{
		if(parent == null)
			return null;

		int count = VisualTreeHelper.GetChildrenCount(parent);
		for(int i = 0; i < count; i++)
		{
			var child = VisualTreeHelper.GetChild(parent, i);
			if(child is T tChild)
				return tChild;

			var result = FindVisualChild<T>(child);
			if(result != null)
				return result;
		}

		return null;
	}
}