using SteamPlaytimeTracker.Services.Steam;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.Extensions;
using System.Collections;

namespace SteamPlaytimeTracker.Utility.Comparer;

internal sealed class AppPlaytimeComparer : IComparer, IComparer<SteamApp>
{
	private readonly IAppService _appService;
	private readonly int _return = 1;

	public AppPlaytimeComparer(IAppService appService, bool descending)
	{
		_appService = appService;
		_return = descending ? -1 : 1;
	}
	public AppPlaytimeComparer(IAppService appService) => _appService = appService;

	public int Compare(SteamApp? x, SteamApp? y)
	{
		if(x == y) return 0;
		if(x == null) return - _return;
		if(y == null) return _return;

		var lRes = _appService.GetEntryAsync(x.AppId).AsTask().Result();
		var rRes = _appService.GetEntryAsync(y.AppId).AsTask().Result();
		var playtimeLeft = lRes!.PlaytimeSlices.Sum(x => x.SessionLength.TotalHours);
		var playtimeRight = rRes!.PlaytimeSlices.Sum(x => x.SessionLength.TotalHours);
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
		if(x is not SteamApp appLeft || y is not SteamApp appRight)
		{
			throw new Exception($"Either argument passed to comparer is not of type {nameof(SteamApp)}");
		}
		return Compare(appLeft, appRight);
	}

}
