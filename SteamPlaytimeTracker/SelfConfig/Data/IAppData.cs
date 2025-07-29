using Config.Net;

namespace SteamPlaytimeTracker.SelfConfig.Data;

public interface IAppData
{
	public string SteamInstallationFolder { get; set; }
	public long LastCheckedSteamApps { get; set; }
	[Option(DefaultValue ="true")]
	public bool CheckForSteamAppsPeriodically { get; set; }
}