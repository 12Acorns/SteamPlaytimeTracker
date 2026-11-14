using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.Steam.Data.Playtime;

namespace SteamPlaytimeTracker.DbObject;

internal class PlaytimeIntervalDTO(PlaytimeSlice slice)
{
	public DateTimeOffset SessionStart { get; } = slice.SessionStart;
	public TimeSpan SessionLength { get; } = slice.SessionLength;
	public SteamAppEntry AppEntry { get; } = slice.SteamAppEntry;
	public DateTimeOffset SessionEnd => SessionStart + SessionLength;
}
