using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Diagnostics;

namespace SteamPlaytimeTracker.Steam.Data.App;

[DebuggerDisplay("{Id} | {Name} | {AppId}")]
public sealed class SteamApp
{
	[Key] 
	public int Id { get; set; }
	[JsonPropertyName("steam_appid")] public required uint AppId { get; set; }
	[JsonPropertyName("name")] public required string Name { get; set; }

	[NotMapped]
	public string ImageUrl => $"https://steamcdn-a.akamaihd.net/steam/apps/{AppId}/library_600x900_2x.jpg";

	public override int GetHashCode() => HashCode.Combine(AppId, Name);
}
