using Serilog;
using SteamPlaytimeTracker.Utility;

namespace SteamPlaytimeTracker.Services.Lifetime;

internal sealed class ApplicationEndAsyncLifetimeService : ILifetimeService
{
	public static ApplicationEndAsyncLifetimeService Default { get; } = new(LoggingService.Logger);

	private readonly CancellationTokenSource _source;

	public ApplicationEndAsyncLifetimeService(ILogger logger)
	{
		_source = new CancellationTokenSource();
		CancellationToken = _source.Token;
		App.OnSessionEndingA.Subscribe(OrderedEventPriority.Minimum, async (sender, e) =>
		{
			if(e.Cancel)
			{
				logger.Information("App was requested to end and cancelled");
				return;
			}
			await _source.CancelAsync().ConfigureAwait(false);
			logger.Information("App was requested to end");
		});
		App.OnSessionClose.Subscribe(OrderedEventPriority.Minimum, async (sender, e) =>
		{
			if(e.Cancel)
			{
				logger.Information("App was requested to close and cancelled");
				return;
			}
			await _source.CancelAsync().ConfigureAwait(false);
			logger.Information("App was requested to close");
		});
	}

	public CancellationToken CancellationToken { get; }
}