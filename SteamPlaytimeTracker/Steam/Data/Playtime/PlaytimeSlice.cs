using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SteamPlaytimeTracker.DbObject;

namespace SteamPlaytimeTracker.Steam.Data.Playtime;

internal sealed class PlaytimeSlice
{
	[Key] public int Id { get; set; }
	public DateTimeOffset SessionStart { get; set; } 
	public TimeSpan SessionLength { get; set; }  
	[NotMapped] public uint AppId { get; set; }
	public SteamAppEntry SteamAppEntry { get; set; } = null!;

	[NotMapped]
	public DateTimeOffset SessionEnd => SessionStart + SessionLength;

	public static bool operator ==(PlaytimeSlice? @this, PlaytimeSlice? other) =>
		(@this is not null && other is not null) &&
		@this.SessionStart == other.SessionStart &&
		@this.SessionLength == other.SessionLength;
	public static bool operator !=(PlaytimeSlice? @this, PlaytimeSlice? other) => !(@this == other);
}