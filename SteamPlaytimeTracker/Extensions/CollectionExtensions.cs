namespace SteamPlaytimeTracker.Extensions;

public static class CollectionExtension
{
	extension<TKey, TData>(IDictionary<TKey, TData> collection) where TKey : notnull
	{
		public bool IsNullOrEmpty() => collection is null or { Count: 0 };
	}
	extension<T>(ICollection<T> collection)
	{
		public bool IsNullOrEmpty() => collection is null or { Count: 0 };
	}
	extension<T>(IEnumerable<T> collection)
	{
		public bool IsNullOrEmpty() => collection == null || !collection.Any();
	}
}
