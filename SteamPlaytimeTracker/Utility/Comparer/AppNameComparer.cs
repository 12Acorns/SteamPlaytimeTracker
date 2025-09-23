using SteamPlaytimeTracker.DbObject;
using System.Collections;

namespace SteamPlaytimeTracker.Utility.Comparer;

internal sealed class AppNameComparer : IComparer, IComparer<string>
{
	private readonly int _return = 1;

	public AppNameComparer(bool descending) => _return = descending ? -1 : 1;

	public int Compare(string? x, string? y) => StringComparer.InvariantCultureIgnoreCase.Compare(x, y) * _return;
	public int Compare(object? x, object? y)
	{
		if(x == y) return 0;
		if(x == null) return -_return;
		if(y == null) return _return;
		if(x is not SteamAppEntry appLeft || y is not SteamAppEntry appRight)
		{
			throw new Exception($"Either argument passed to comparer is not of type {nameof(SteamAppEntry)}");
		}
		return Compare(appLeft.SteamApp.Name, appRight.SteamApp.Name);
	}
}
