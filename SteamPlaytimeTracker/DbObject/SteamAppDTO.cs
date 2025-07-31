namespace SteamPlaytimeTracker.DbObject;

internal class SteamAppDTO
{
	public SteamAppDTO(uint appId, string appName) =>
		(AppName, AppId) = (appName, appId);

	public int SteamAppDTOId { get; set; }
	public uint AppId { get; set; }
	public string AppName { get; set; }

	public override int GetHashCode() => HashCode.Combine(AppId, AppName);
}
