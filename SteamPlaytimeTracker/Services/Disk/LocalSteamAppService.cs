using OutParsing;
using Serilog;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.IO;
using SteamPlaytimeTracker.Utility;
using SteamPlaytimeTracker.Utility.Cache;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;

namespace SteamPlaytimeTracker.Services.Disk;

internal sealed class LocalSteamAppService : ILocalSteamAppService
{
	private const int LocalCacheDurationMinutes = 10;

	private readonly ICacheManager _cacheManager;
	private readonly ILogger _logger;

	public LocalSteamAppService(ILogger logger, ICacheManager cacheManager)
	{
		_cacheManager = cacheManager;
		_logger = logger;
	}

	public ValueTask<HashSet<uint>> GetLocalAppIds(CancellationToken token = default) => _cacheManager.GetAsync("LocalAppIds", LocalCacheDurationMinutes,
		async () =>
	{
		if(ApplicationPath.TryGetPath(GlobalData.MainTimeSliceCheckLookupName, out var primarySearchFile) && File.Exists(primarySearchFile))
		{
			return await GetLocalAppIdsPrimary(primarySearchFile, token).ToHashSetAsync(cancellationToken: token).ConfigureAwait(false);
		}
		return [];
	});
	private IAsyncEnumerable<uint> GetLocalAppIdsPrimary(string searchFile, CancellationToken token)
	{
		return IOUtility.HandleTmpFileLifetimeAsyncEnumerable(searchFile, tmpFile => GetIds(tmpFile, token), cancellationToken: token);
		async IAsyncEnumerable<uint> GetIds(string tmpFile, [EnumeratorCancellation] CancellationToken ct)
		{
			var seen = new ConcurrentDictionary<uint, byte>();
			await foreach(var line in IOUtility.ReadLinesAsync(tmpFile, cancellationToken: token).ConfigureAwait(false))
			{
				if(token.IsCancellationRequested)
				{
					_logger.Debug("Cancellation requested, stopping reading local apps.");
					break;
				}
				if(string.IsNullOrWhiteSpace(line))
				{
					continue;
				}
				if(!OutParser.TryParse(line, "[{date}] AppID {appId} adding PID {pidId} as a tracked process {appPath}",
					out string date, out uint appId, out int pidId, out string appPath))
				{
					continue;
				}
				if(seen.TryAdd(appId, 0))
				{
					yield return appId;
				}
			}
			yield break;
		}
	}
}
