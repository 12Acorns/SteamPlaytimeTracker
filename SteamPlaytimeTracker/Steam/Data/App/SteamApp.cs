using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;

namespace SteamPlaytimeTracker.Steam.Data.App;

[DebuggerDisplay("{Name} | {AppId}")]
public sealed record class SteamApp
{
	[Key] public int Id { get; set; }
	[JsonPropertyName("appid")] public required uint AppId { get; set; }
	[JsonPropertyName("name")] public required string Name { get; set; }

	[NotMapped]
	public string ImageUrl => $"https://steamcdn-a.akamaihd.net/steam/apps/{AppId}/library_600x900_2x.jpg";

	public override int GetHashCode() => HashCode.Combine(AppId, Name);
}
