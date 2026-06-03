using System.Text.Json.Serialization;

namespace SteamPlaytimeTracker.Utility.ProcessTracking;

internal sealed record ProcessTrackingInfoSerializable(
	[field: JsonPropertyName("process_name")] string ProcessName,
	[field: JsonPropertyName("application_path")] string ApplicationPath,
	[field: JsonPropertyName("tracked")] bool Tracked);