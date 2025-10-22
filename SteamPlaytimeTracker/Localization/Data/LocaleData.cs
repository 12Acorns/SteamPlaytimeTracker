using System.Text.Json.Serialization;

namespace SteamPlaytimeTracker.Localization.Data;

internal sealed class LocaleData
{
	[JsonPropertyName("code")]
	public required string Code { get; init; }
	[JsonPropertyName("display-name")]
	public required string DisplayName { get; init; }
	public Dictionary<string, string> LocaleTextMap { get; } = [];
}