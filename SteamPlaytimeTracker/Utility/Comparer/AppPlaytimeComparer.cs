using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.DbObject;
using System.Collections;

namespace SteamPlaytimeTracker.Utility.Comparer;

internal sealed class AppPlaytimeComparer : IComparer, IComparer<SteamAppEntry>
{
	private readonly int _return = 1;

	public AppPlaytimeComparer(bool descending)
	{
		_return = descending ? -1 : 1;
	}

	public int Compare(SteamAppEntry? x, SteamAppEntry? y)
	{
		if(x == y) return 0;
		if(x == null) return - _return;
		if(y == null) return _return;

		var playtimeLeft = TimeSpan.FromTicks(x.TotalPlaytime).TotalHours;
		var playtimeRight = TimeSpan.FromTicks(y.TotalPlaytime).TotalHours;
		return playtimeLeft.CompareTo(playtimeRight) * _return;
	}
	public int Compare(object? x, object? y)
	{
		if(x == y) return 0;
		if(x == null) return -_return;
		if(y == null) return _return;
		if(x is not SteamAppEntry appLeft || y is not SteamAppEntry appRight)
		{
			throw new Exception($"Either argument passed to comparer is not of type {nameof(SteamAppEntry)}");
		}
		return Compare(appLeft, appRight);
	}
}
