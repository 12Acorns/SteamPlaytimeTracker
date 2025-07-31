using System.Text.Json.Serialization;

namespace SteamPlaytimeTracker.Steam.Data.App;

public readonly record struct SteamApp(
	[property: JsonPropertyName("appid")] uint Id,
	[property: JsonPropertyName("name")] string Name);
