using SteamPlaytimeTracker.DbObject;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

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