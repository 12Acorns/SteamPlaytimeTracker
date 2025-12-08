using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.DbObject;

namespace SteamPlaytimeTracker.Services.Steam;

internal interface IAppService
{
	public ValueTask<List<SteamAppEntry>> AllEntries(CancellationToken token = default);
	public ValueTask<SteamAppEntry?> GetEntryAsync(uint appId, CancellationToken token = default);
	public ValueTask<IEnumerable<SteamStoreAppData>> GetLocalAppsAsync(CancellationToken token = default);
}
