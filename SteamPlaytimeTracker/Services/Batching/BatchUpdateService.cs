using SteamPlaytimeTracker.Extensions;
using System.Threading.Channels;
using Serilog;

namespace SteamPlaytimeTracker.Services.Batching;

internal sealed class BatchUpdateService<T> : IBatchUpdateService<T>
{
	private readonly SemaphoreSlim _enqueueSlim = new(1, 1);
	private readonly ChannelWriter<T> _dataWriter;
	private readonly ChannelReader<T> _dataReader;
	private readonly BatchOptions _options;
	private readonly ILogger _logger;

	public event Action<IList<T>>? BatchReady;

	public BatchUpdateService(BatchOptions options, ILogger logger)
	{
		var channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions()
		{
			SingleReader = false,
			SingleWriter = false,
		});
		(_dataReader, _dataWriter) = (channel.Reader, channel.Writer);
		_options = options;
		_logger = logger;
	}

	public void Enqueue(T item)
	{
		_enqueueSlim.Wait();
		_dataWriter.WriteAsync(item).AsTask().ContinueWith(t => 
		{
			if(t.IsFaulted && !t.IsCanceled)
			{
				_logger.Error(t.Exception, "An error occurred while enqueuing an item to the batch update service.");
			}
			_enqueueSlim.Release();
		});
	}
	public async void StartProcessing(CancellationToken token)
	{
		var timingSemaphore = new SemaphoreSlim(1, 1);
		int currentRetriesFromTime = 0;
		int growthFactor = 1;
		var currentWaitTime = _options.MaximumWaitInterval;
		try
		{
			while(true)
			{
				token.ThrowIfCancellationRequested();
				await ProcessIfCountOrTimePasses(token: token).ConfigureAwait(false);
			}
		}
		catch(Exception ex) when(ex is not OperationCanceledException)
		{
			_logger.Error(ex, "An error occurred while processing the app queue.");
		}
		finally
		{
			timingSemaphore.TryRelease();
		}

		async ValueTask ProcessIfCountOrTimePasses(CancellationToken token)
		{
			if(_dataReader.Count < _options.MaximumBatchSize)
			{
				await ProcessAfterTime(currentWaitTime, token).ConfigureAwait(false);
				currentRetriesFromTime++;
				if(currentRetriesFromTime >= _options.MaximumRetries)
				{
					currentWaitTime *= Math.Pow(_options.BaseGrowthFactor, growthFactor++);
					if(currentWaitTime > _options.MaximumWaitInterval)
					{
						currentWaitTime = _options.MaximumWaitInterval;
					}
					currentRetriesFromTime = 0;
				}
				return;
			}

			currentWaitTime = _options.MinimumWaitInterval;
			currentRetriesFromTime = 0;
			growthFactor = 1;
		}
		async Task ProcessAfterTime(TimeSpan time, CancellationToken token)
		{
			try
			{
				await timingSemaphore.WaitAsync(time, token).ConfigureAwait(false);
				var items = new List<T>();
				while(_dataReader.TryRead(out var item) && items.Count < _options.MaximumBatchSize)
				{
					items.Add(item);
				}
				BatchReady?.Invoke(items);
			}
			catch(OperationCanceledException)
			{
				// Ignore cancellation exceptions, as they are expected during shutdown.
			}
			catch(Exception ex)
			{
				_logger.Error(ex, "An error occurred while waiting to process a batch.");
			}
			finally
			{
				timingSemaphore.TryRelease();
			}
		}
	}
}