using SteamPlaytimeTracker.Extensions;
using System.Threading.Channels;
using System.Buffers;
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

	public bool TryEnqueue(T item)
	{
		try
		{
			_enqueueSlim.Wait();
			return _dataWriter.TryWrite(item);
		}
		finally
		{
			_enqueueSlim.TryRelease();
		}
	}
	public void StartProcessing(CancellationToken token)
	{
		var timingSemaphore = new SemaphoreSlim(1, 1);
		int currentRetriesFromTime = 0;
		int growthFactor = 1;
		var currentWaitTime = _options.MinimumWaitInterval;
		_ = Task.Run(async () =>
		{
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
		}, token);

		async ValueTask ProcessIfCountOrTimePasses(CancellationToken token)
		{
			if(_dataReader.Count < _options.MaximumBatchSize)
			{
				await ProcessAfterTime(currentWaitTime, token).ConfigureAwait(false);
				currentRetriesFromTime++;
				if(currentRetriesFromTime >= _options.MaximumRetries)
				{
					var tmp = unchecked(currentWaitTime * Math.Pow(_options.BaseGrowthFactor, growthFactor++));
					if(tmp < currentWaitTime)
					{
						tmp = currentWaitTime;
					}
					currentWaitTime = tmp;
					if(currentWaitTime > _options.MaximumWaitInterval)
					{
						currentWaitTime = _options.MaximumWaitInterval;
						growthFactor--;
					}
					currentRetriesFromTime = 0;
				}
				return;
			}
			currentWaitTime = _options.MinimumWaitInterval;
			currentRetriesFromTime = 0;
			growthFactor = 1;

			var currentDequeueed = 0;
			var entryQueue = ArrayPool<T>.Shared.Rent(_options.MaximumBatchSize);
			while(currentDequeueed < _options.MaximumBatchSize && _dataReader.Count > 0)
			{
				entryQueue[currentDequeueed++] = await _dataReader.ReadAsync(token).ConfigureAwait(false);
			}
			BatchReady?.Invoke(entryQueue.ToList()[..currentDequeueed]);
			ArrayPool<T>.Shared.Return(entryQueue);
		}
		async Task ProcessAfterTime(TimeSpan time, CancellationToken token)
		{
			try
			{
				await timingSemaphore.WaitAsync(token).ConfigureAwait(false);
				await timingSemaphore.WaitAsync(time, token).ConfigureAwait(false);
				var items = new List<T>();
				while(_dataReader.TryRead(out var item) && items.Count < _options.MaximumBatchSize)
				{
					items.Add(item);
				}
				BatchReady?.Invoke(items);
			}
			catch(Exception ex) when(ex is not OperationCanceledException)
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