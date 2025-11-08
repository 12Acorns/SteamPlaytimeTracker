using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SteamPlaytimeTracker.DbObject;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Net.WebSockets;
using System.Globalization;

namespace SteamPlaytimeTracker.Steam.Data.Playtime;

[DebuggerDisplay("{Id} | {SessionStart} | {SessionLength} | {AppId}")]
internal sealed class PlaytimeSlice
{
	[Key] public int Id { get; set; }
	[JsonPropertyName("start")] public DateTimeOffset SessionStart { get; set; } 
	[JsonPropertyName("duration")] public TimeSpan SessionLength { get; set; }  
	[NotMapped] public uint AppId { get; set; }
	[JsonIgnore] public SteamAppEntry SteamAppEntry { get; set; } = null!;

	[NotMapped]
	public DateTimeOffset SessionEnd => SessionStart + SessionLength;

	public static bool operator ==(PlaytimeSlice? @this, PlaytimeSlice? other) =>
		(@this is not null && other is not null) &&
		@this.SessionStart == other.SessionStart &&
		@this.SessionLength == other.SessionLength;
	public static bool operator !=(PlaytimeSlice? @this, PlaytimeSlice? other) => !(@this == other);

	public override int GetHashCode() => HashCode.Combine(SessionStart, SessionLength);
}