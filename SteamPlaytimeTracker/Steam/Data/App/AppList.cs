using System.Text.Json.Serialization;

namespace SteamPlaytimeTracker.Steam.Data.App;

internal sealed class AppList
{
	[JsonPropertyName("apps")]
	public required SteamApp[] SteamApps { get; set; }
}
