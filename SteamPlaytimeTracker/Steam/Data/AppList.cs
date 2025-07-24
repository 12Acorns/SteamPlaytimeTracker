using System.Text.Json.Serialization;

namespace SteamPlaytimeTracker.Steam.Data;

internal sealed class AppList
{
	[JsonPropertyName("apps")]
	public required AppData[] SteamApps { get; set; }
}
