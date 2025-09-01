using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.DbObject;

namespace SteamPlaytimeTracker.Services.Steam;

internal interface IAppService
{
	public ValueTask<SteamAppEntry?> GetEntryAsync(uint appId, CancellationToken token = default);
	public ValueTask<IEnumerable<SteamApp>> GetLocalAppsAsync(CancellationToken token = default);
}
