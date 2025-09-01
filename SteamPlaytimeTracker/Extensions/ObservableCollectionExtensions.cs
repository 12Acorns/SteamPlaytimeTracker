using System.Collections.ObjectModel;

namespace SteamPlaytimeTracker.Extensions;

internal static class ObservableCollectionExtensions
{
	public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items, bool isInAsyncContext = false)
	{
		foreach(var item in items)
		{
			collection.Add(item);
		}
	}
}
