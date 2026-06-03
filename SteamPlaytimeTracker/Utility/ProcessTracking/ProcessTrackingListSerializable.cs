using System.Text.Json.Serialization;

namespace SteamPlaytimeTracker.Utility.ProcessTracking;

internal sealed record ProcessTrackingListSerializable([field: JsonPropertyName("info")] ProcessTrackingInfoSerializable[] TrackingInfo);
