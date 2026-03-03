using SteamPlaytimeTracker.Steam.Data.Playtime;
using OneOf.Monads;

namespace SteamPlaytimeTracker.Services.Playtime;

internal interface IPlaytimeService
{
	public ValueTask<Dictionary<uint, List<PlaytimeSlice>>> GetPlayimeSegments(CancellationToken cancellationToken = default);
	public virtual async ValueTask<Option<List<PlaytimeSlice>>> TryGetSegmentsForApp(uint appId, CancellationToken token = default)
	{
		var allSegmentsTask = await GetPlayimeSegments(token).ConfigureAwait(false);
		if(allSegmentsTask.TryGetValue(appId, out var segments))
		{
			return new Some<List<PlaytimeSlice>>(segments);
		}
		return new None();
	}
}