using SteamPlaytimeTracker.Steam.Data.App;

namespace SteamPlaytimeTracker.Services._App;

internal interface IAppSynchronisationService
{
	public void EnqueueForDbSync(SteamAppEntry entry);
	public Task CommitSyncToDbAsync(CancellationToken token);
}