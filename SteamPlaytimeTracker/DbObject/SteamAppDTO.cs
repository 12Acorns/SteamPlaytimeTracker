namespace SteamPlaytimeTracker.DbObject;

internal class SteamAppDTO
{
	public SteamAppDTO(int appId, string appName) =>
		(AppName, AppId) = (appName, appId);

	public int SteamAppDTOId { get; set; }
	public int AppId { get; set; }
	public string AppName { get; set; }
}
