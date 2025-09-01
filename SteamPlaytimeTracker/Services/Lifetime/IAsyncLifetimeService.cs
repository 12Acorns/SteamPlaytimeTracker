namespace SteamPlaytimeTracker.Services.Lifetime;

internal interface IAsyncLifetimeService
{
	public CancellationToken CancellationToken { get; }
}
