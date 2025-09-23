using Config.Net;

namespace SteamPlaytimeTracker.SelfConfig.Data;

public interface ISteamInstallData
{
	[Option(DefaultValue = null, Alias = nameof(SteamInstallationFolder))]
	public string? SteamInstallationFolder { get; set; }
	[Option(DefaultValue = null, Alias = nameof(LastCheckedSteamApps))]
	public long? LastCheckedSteamApps { get; set; }
	[Option(DefaultValue = true, Alias = nameof(CheckForSteamAppsPeriodically))]
	public bool CheckForSteamAppsPeriodically { get; set; }
}
