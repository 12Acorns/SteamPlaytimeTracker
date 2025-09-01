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
}
