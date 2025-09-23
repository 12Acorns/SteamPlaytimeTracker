using SteamPlaytimeTracker.Services.Steam;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.Extensions;
using System.Collections;
using SteamPlaytimeTracker.DbObject;

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

		var playtimeLeft = x.PlaytimeSlices.Sum(x => x.SessionLength.TotalHours);
		var playtimeRight = y.PlaytimeSlices.Sum(x => x.SessionLength.TotalHours);
		if(playtimeLeft > playtimeRight)
		{
			return _return;
		}
		if(playtimeLeft < playtimeRight)
		{
			return -_return;
		}
		return 0;
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
