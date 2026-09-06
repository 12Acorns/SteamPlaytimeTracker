using SteamPlaytimeTracker.Steam.Data.Playtime;
using System.Diagnostics.CodeAnalysis;

namespace SteamPlaytimeTracker.Utility.Equality;

internal sealed class PlaytimeSliceEquality : IEqualityComparer<PlaytimeSlice>
{
	public static PlaytimeSliceEquality Instance { get; } = new();

	public bool Equals(PlaytimeSlice? self, PlaytimeSlice? other)
	{
		if(self is null || other is null)
		{
			return false;
		}
		return self == other;
	}

	public int GetHashCode([DisallowNull] PlaytimeSlice obj) => obj.GetHashCode();

	public static bool SequencesEqual(IEnumerable<PlaytimeSlice> first, IEnumerable<PlaytimeSlice> second) =>
		first.OrderBy(x => x.SessionStart).Aggregate(0, HashCode.Combine) == second.OrderBy(x => x.SessionStart).Aggregate(0, HashCode.Combine);
}
