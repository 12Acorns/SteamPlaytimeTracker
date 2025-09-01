namespace SteamPlaytimeTracker.Services.Lifetime;

internal sealed class ApplicationEndAsyncLifetimeService : IAsyncLifetimeService
{
	public static ApplicationEndAsyncLifetimeService Default { get; } = new();

	private readonly CancellationTokenSource _source;

	public ApplicationEndAsyncLifetimeService()
	{
		_source = new CancellationTokenSource();
		CancellationToken = _source.Token;
		App.OnSessionEndingA += async (sender, e) =>
		{
			if(e.Cancel)
			{
				return;
			}
			await _source.CancelAsync().ConfigureAwait(false);
		};
	}

	public CancellationToken CancellationToken { get; }
}
