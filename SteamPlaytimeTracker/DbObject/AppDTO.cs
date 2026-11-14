using SteamPlaytimeTracker.Steam.Data.App;

namespace SteamPlaytimeTracker.DbObject;

internal sealed class AppDTO
{
	public AppDTO(SteamAppEntry entry)
	{
		ArgumentNullException.ThrowIfNull(entry.SteamApp);
		AppId = entry.SteamApp.AppId;
		Name = entry.SteamApp.Name;
		StoreDetails = new(entry.StoreDetails);
		Intervals = entry.PlaytimeSlices.Select(interval => new PlaytimeIntervalDTO(interval)).ToArray();
		IntervalCount = entry.PlaytimeSlices.Count;
	}

	public uint AppId { get; }
	public string Name { get; }
	public StoreAppDTO StoreDetails { get; }
	public IEnumerable<PlaytimeIntervalDTO> Intervals { get; }
	public int IntervalCount { get; }
	public string CapsuleImageUrl => $"https://steamcdn-a.akamaihd.net/steam/apps/{AppId}/library_600x900_2x.jpg";
}