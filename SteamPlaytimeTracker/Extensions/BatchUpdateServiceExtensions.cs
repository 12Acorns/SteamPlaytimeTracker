using SteamPlaytimeTracker.Services.Batching;

namespace SteamPlaytimeTracker.Extensions;

internal static class BatchUpdateServiceExtensions
{
	extension<T>(IBatchUpdateService<T> batchUpdateService)
	{
		public void EnqueueRange(IEnumerable<T> items)
		{
			foreach(var item in items)
			{
				batchUpdateService.Enqueue(item);
			}
		}
	}
}