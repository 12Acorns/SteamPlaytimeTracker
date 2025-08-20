namespace SteamPlaytimeTracker.Services;

internal interface IAsyncLifetimeService
{
	public CancellationToken CancellationToken { get; }
}
