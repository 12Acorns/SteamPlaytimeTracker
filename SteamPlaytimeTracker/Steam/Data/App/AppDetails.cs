using System.Text.Json.Serialization;

namespace SteamPlaytimeTracker.Steam.Data.App;

internal readonly record struct AppDetailsContainer(
	[property: JsonPropertyName("success")] bool Success,
	[property: JsonPropertyName("data")] AppDetails Details);
internal readonly record struct AppDetails(
	[property: JsonPropertyName("type")] string AppType,
	[property: JsonPropertyName("name")] string Name,
	[property: JsonPropertyName("steam_appid")] uint Id,
	[property: JsonPropertyName("required_age")] int Age,
	[property: JsonPropertyName("is_free")] bool IsFree);