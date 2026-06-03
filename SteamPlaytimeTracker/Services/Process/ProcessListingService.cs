using SteamPlaytimeTracker.Utility.Interoping.WinProc;

namespace SteamPlaytimeTracker.Services.Process;

internal sealed class ProcessListingService : IProcessListingService
{
	public IEnumerable<ProcessInfo> GetProcessList() => WindowsProcessUtility.GetVisibleProcesses()
		.Select(x => new ProcessInfo(
			x.Handle,
			WindowsProcessUtility.GetProcessId(x.Handle),
			x.Title,
			WindowsProcessUtility.GetProcessApplicationPathFromWHandle(x.Handle)));

}