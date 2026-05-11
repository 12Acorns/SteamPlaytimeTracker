namespace SteamPlaytimeTracker.Services.Lifetime;

internal interface ILifetimeService
{
	public CancellationToken CancellationToken { get; }
}
