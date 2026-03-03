using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using SteamPlaytimeTracker.DbObject;
using System.Globalization;
using System.Diagnostics;

namespace SteamPlaytimeTracker.Steam.Data.Playtime;

[DebuggerDisplay("{Id} | {SessionStart} | {SessionLength} | {AppId}")]
internal sealed class PlaytimeSlice
{
	public static bool TryCreate(string startDate, string endDate, uint appId, [NotNullWhen(true)] out PlaytimeSlice? slice)
	{
		slice = null;
		if(string.IsNullOrWhiteSpace(startDate) || string.IsNullOrWhiteSpace(endDate))
		{
			return false;
		}
		if(!DateTimeOffset.TryParseExact(
			startDate,
			"yyyy-MM-dd HH:mm:ss",
			CultureInfo.InvariantCulture,
			DateTimeStyles.AssumeLocal, 
			out var startDateOffset))
		{
			return false;
		}
		if(!DateTimeOffset.TryParseExact(
			endDate,
			"yyyy-MM-dd HH:mm:ss",
			CultureInfo.InvariantCulture,
			DateTimeStyles.AssumeLocal, 
			out var endDateOffset))
		{
			return false;
		}
		var dateDelta = endDateOffset - startDateOffset;
		slice = new PlaytimeSlice
		{
			AppId = appId,
			SessionStart = startDateOffset,
			SessionLength = dateDelta
		};
		return true;
	}

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

	public override bool Equals(object? obj)
	{
		if(ReferenceEquals(this, obj))
		{
			return true;
		}
		if(obj is null)
		{
			return false;
		}
		return this == (obj as PlaytimeSlice);
	}
}