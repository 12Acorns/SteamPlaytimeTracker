using System.Text.Json.Serialization;

namespace SteamPlaytimeTracker.DataTransfer;

internal readonly record struct PlaytimeExportStructureContainer([property: JsonPropertyName("apps")] List<AppPlaytimeStructure> Structure);
