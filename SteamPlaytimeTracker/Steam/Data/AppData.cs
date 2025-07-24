using System.Text.Json.Serialization;

namespace SteamPlaytimeTracker.Steam.Data;

internal readonly record struct AppData(
	[property: JsonPropertyName("appid")] int AppId,
	[property: JsonPropertyName("name")] string Name);
