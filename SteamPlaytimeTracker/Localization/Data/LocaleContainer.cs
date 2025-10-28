using System.Text.Json.Serialization;

namespace SteamPlaytimeTracker.Localization.Data;

internal sealed class LocaleContainer
{
	[JsonPropertyName("locales")]
	public List<LocaleData> Locales { get; set; } = [];
}
internal sealed class TranslationMap
{
	[JsonPropertyName("translations")]
	public Dictionary<string, string> Map { get; set; } = [];

	public static implicit operator Dictionary<string, string>?(TranslationMap? translationMap) => translationMap?.Map;
}