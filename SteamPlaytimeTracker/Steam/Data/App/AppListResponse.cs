using System.Text.Json.Serialization;

namespace SteamPlaytimeTracker.Steam.Data.App;

internal readonly record struct AppListResponse
{
	[JsonPropertyName("applist")]
	public readonly required AppList Apps { get; init; }
}
internal enum ParseResult
{
	FailedToParse,
	UnkownError
}