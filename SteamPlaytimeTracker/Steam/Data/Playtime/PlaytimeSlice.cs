namespace SteamPlaytimeTracker.Steam.Data.Playtime;

internal readonly record struct PlaytimeSlice(DateTimeOffset SessionStart, TimeSpan SessionLength, uint AppId)
{
	public readonly DateTimeOffset SessionEnd => SessionStart + SessionLength;
}