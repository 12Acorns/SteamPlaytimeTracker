namespace SteamPlaytimeTracker.DbObject;
internal class SteamAppEntry
{
	public int SteamAppEntryId { get; set; }

	public  int SteamAppDTOId { get; set; }
	public SteamAppDTO SteamApp { get; set; }

	public List<PlaytimeSliceDTO> PlaytimeSegments { get; set; } = new();
}
