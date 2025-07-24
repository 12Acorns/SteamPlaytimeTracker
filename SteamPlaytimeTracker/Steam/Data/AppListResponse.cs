using System.Text.Json.Serialization;

namespace SteamPlaytimeTracker.Steam.Data;

internal readonly record struct AppListResponse
{
	[JsonPropertyName("applist")]
	public readonly required AppList Apps { get; init; }
}
internal enum AppListResponseStatus
{
	FailedToParse,
	UnkownError
}