namespace SteamPlaytimeTracker.DbObject;

internal class SteamAppEntry
{
	public int SteamAppEntryId { get; set; }

	public int SteamAppDTOId { get; set; }
	public SteamAppDTO SteamApp { get; set; }

	public List<PlaytimeSliceDTO> PlaytimeSegments { get; set; } = [];

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(SteamApp?.GetHashCode());
		foreach(var segment in PlaytimeSegments)
		{
			hash.Add(segment.GetHashCode());
		}

		return hash.ToHashCode();
	}
}
