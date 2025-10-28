using System.Text.Json.Serialization;

namespace SteamPlaytimeTracker.Localization.Data;

internal sealed class LocaleData
{
	[JsonPropertyName("code")]
	public required string Code { get; set; }
	[JsonPropertyName("display-name")]
	public required string DisplayName { get; set; }
	[JsonPropertyName("translations")]
	public Dictionary<string, string> LocaleTextMap { get; set; } = [];
}