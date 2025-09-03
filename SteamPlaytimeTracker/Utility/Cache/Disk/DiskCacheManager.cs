using OneOf.Monads;
using ScottPlot.Interactivity.UserActions;
using Serilog;
using SkiaSharp;
using SteamPlaytimeTracker.SelfConfig;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Utility.Cache.Disk;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace SteamPlaytimeTracker.Utility.Cache;

internal sealed class DiskCacheManager : IAsyncCacheManager
{
	private static readonly Lock _lock = new();

	private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

	private readonly IAsyncLifetimeService _lifetimeService;
	private readonly AppConfig _config;
	private readonly ILogger _logger;

	private DateTimeOffset _lastCacheValidationTime;
	private TimeSpan _minTimeUntilNextCacheCleanup = default;

	public DiskCacheManager(ILogger logger, IAsyncLifetimeService lifetimeService, AppConfig config)
	{
		_lifetimeService = lifetimeService;
		_logger = logger;
		_config = config;
	}

	public async ValueTask<T> GetAsync<T>(string key, Func<FileStream, ValueTask<T>>? converter)
		where T : unmanaged
	{
		if(string.IsNullOrWhiteSpace(key))
		{
			return default!;
		}
		if(!_config.AppData.UseDiskCache)
		{
			return default!;
		}
		converter ??= async stream =>
		{
			byte[] finalBuffer = new byte[stream.Length];
			byte[] buffer = new byte[4096];
			int prevBytes = 0;
			int bytesRead;
			while((bytesRead = await stream.ReadAsync(buffer, _lifetimeService.CancellationToken).ConfigureAwait(false)) > 0)
			{
				buffer.CopyTo(finalBuffer.AsSpan()[prevBytes..]);
				prevBytes += bytesRead;
			}
			return MemoryMarshal.AsRef<T>(finalBuffer);
		};
		try
		{
			var fileName = $"{key.GetHashCode()}.cdat";
			var path = Path.Combine(_config.AppData.CacheFolder, fileName);
			using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan | FileOptions.Asynchronous);
			return await converter(stream).ConfigureAwait(false);
		}
		catch(Exception ex)
		{
			_logger.Error(ex, "Failed to read file from disk cache. File likely does not exist.");
			return default!;
		}
		finally
		{
			await CacheCleanup().ConfigureAwait(false);
		}
	}
	public async ValueTask<Option<T>> TryGetAsync<T>(string key, Func<FileStream, ValueTask<T>>? converter)
		where T : unmanaged
	{
		if(!_config.AppData.UseDiskCache)
		{
			return Option<T>.None();
		}
		try
		{
			var res = await GetAsync(key, converter).ConfigureAwait(false);
			if(!res.Equals(default))
			{
				return Option<T>.None();
			}
			return Option<T>.Some(res);
		}
		catch(Exception ex)
		{
			_logger.Error(ex, "Failed to read file from disk cache. File likely does not exist.");
			return Option<T>.None();
		}
		finally
		{
			await CacheCleanup().ConfigureAwait(false);
		}
	}
	public ValueTask SetAsync(string key, ReadOnlySpan<byte> data, TimeSpan cacheTime)
	{
		if(string.IsNullOrWhiteSpace(key))
		{
			return default;
		}
		throw new NotImplementedException();
	}
	public ValueTask SetAsync(string key, ReadOnlySpan<byte> data, int cacheTimeMinutes = ICacheManager.DefaultCacheTime)
	{
		if(string.IsNullOrWhiteSpace(key))
		{
			return default;
		}
		throw new NotImplementedException();
	}
	public ValueTask IsSetAsync(string key)
	{
		throw new NotImplementedException();

	}

	public ValueTask RemoveAsync(string key)
	{
		throw new NotImplementedException();

	}


	public ValueTask ClearAsync()
	{
		throw new NotImplementedException();

	}

	private async ValueTask CacheCleanup()
	{
		if(!_config.AppData.UseDiskCache)
		{
			return;
		}

		var currentDate = DateTimeOffset.UtcNow;
		if(currentDate < (_lastCacheValidationTime + _minTimeUntilNextCacheCleanup))
		{
			return;
		}

		if(!await _semaphoreSlim.WaitAsync(TimeSpan.FromSeconds(45), _lifetimeService.CancellationToken).ConfigureAwait(false))
		{
			return;
		}

		_lastCacheValidationTime = currentDate;
		if(_minTimeUntilNextCacheCleanup == default)
		{
			_minTimeUntilNextCacheCleanup = TimeSpan.FromMinutes(15);
		}
		else
		{
			_minTimeUntilNextCacheCleanup *= 1.5d;
		}
		await Task.Run(() =>
		{
			try
			{
				foreach(var file in Directory.EnumerateFiles(_config.AppData.CacheFolder))
				{
					try
					{
						var details = new FileInfo(file);
						if(!int.TryParse(Path.GetFileNameWithoutExtension(details.Name), out var nameHash))
						{
							continue;
						}

						var entry = _config.CacheEntries.FirstOrDefault(x => x.KeyHash == nameHash);
						if(entry == null)
						{
							if(details.Extension is ".cdat")
							{
								File.Delete(file);
								_logger.Information("Deleted '{0}' at '{1}', did not find in tracked entries", details.Name, details.Directory?.FullName);
							}
							continue;
						}
						if(currentDate > (details.CreationTime + entry.ExpirationTime))
						{
							File.Delete(file);
							_config.CacheEntries.Remove(entry);
							_logger.Information("Deleted '{0}' at '{1}'", details.Name, details.Directory?.FullName);
						}
					}
					catch(Exception ex)
					{
						_logger.Error(ex, "Failed to delete untracked cached file.");
					}
				}
			}
			catch(Exception ex)
			{
				_logger.Error(ex, "Could not enumerate files under provided folder. Perhaps the folder entered does not exist or incorrect directory permission.");
			}
		}, _lifetimeService.CancellationToken).ConfigureAwait(false);
		_semaphoreSlim.Release();
	}
}