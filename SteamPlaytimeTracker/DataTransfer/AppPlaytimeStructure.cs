using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.Steam.Data.Playtime;
using System.Text.Json.Serialization;

namespace SteamPlaytimeTracker.DataTransfer;

internal readonly record struct AppPlaytimeStructure
{
	public AppPlaytimeStructure(SteamAppEntry entry)
	{
		if(entry.SteamApp == null)
		{
			throw new ArgumentException("The provided SteamAppEntry does not contain a valid SteamApp.", nameof(entry));
		}
		AppId = (int)entry.SteamApp.AppId;
		PlaytimeSlices = entry.PlaytimeSlices.Select(ps => new PlaytimeSliceExportStructure(ps)).ToList();
	}
	public AppPlaytimeStructure(IGrouping<int, PlaytimeSlice> group)
	{
		AppId = group.Key;
		PlaytimeSlices = group.Select(ps => new PlaytimeSliceExportStructure(ps)).ToList();
	}

	[JsonPropertyName("steam-id")] public int AppId { get; init; }
	[JsonPropertyName("playtime-segments")] public List<PlaytimeSliceExportStructure> PlaytimeSlices { get; init; }
}
