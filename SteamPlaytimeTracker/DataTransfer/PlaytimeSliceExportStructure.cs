using SteamPlaytimeTracker.Steam.Data.Playtime;
using System.Text.Json.Serialization;

namespace SteamPlaytimeTracker.DataTransfer;

internal readonly record struct PlaytimeSliceExportStructure
{
	public PlaytimeSliceExportStructure(PlaytimeSlice slice)
	{
		StartTime = GlobalData.DateToString(slice.SessionStart);
		Duration = slice.SessionLength.ToString(@"dd\.hh\:mm\:ss");
	}

	[JsonPropertyName("start")] public string StartTime { get; init; }
	[JsonPropertyName("duration")] public string Duration { get; init; }
}