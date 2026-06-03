namespace SteamPlaytimeTracker.Services.Process;

internal interface IProcessListingService
{
	public IEnumerable<ProcessInfo> GetProcessList();
}
