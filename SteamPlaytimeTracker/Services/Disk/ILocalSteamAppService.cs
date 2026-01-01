namespace SteamPlaytimeTracker.Services.Disk;

internal interface ILocalSteamAppService
{
	public ValueTask<HashSet<uint>> GetLocalAppIds(CancellationToken token = default);
}