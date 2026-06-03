using System.Runtime.InteropServices;
using System.Text;

namespace SteamPlaytimeTracker.Utility.Interoping.WinProc;

// https://stackoverflow.com/questions/77231403/get-a-list-of-open-application-windows-in-c-windows
// https://stackoverflow.com/questions/19867402/how-can-i-use-enumwindows-to-find-windows-with-a-specific-caption-title
// https://stackoverflow.com/questions/20470389/how-to-get-running-applications-in-windows
// https://forums.codeguru.com/showthread.php?243416-definition-of-struct-HWND
internal partial class WindowsProcessUtility
{
	private const int PROCESS_QUERY_LIMITED_INFORMATION = 0x00001000;
	private const nint WS_EX_TOOLWINDOW = (nint)0x00000080L;
	private const nint WS_EX_APPWINDOW = (nint)0x00040000L;
	private const int GWL_EXSTYLE = -20;

	private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

	[LibraryImport("user32.dll")]
	protected static partial int GetWindowTextLengthA(IntPtr hWnd);
	[LibraryImport("user32.dll", SetLastError = true)] private static partial nint GetAncestor(nint hwind, uint gaFlags);
	[LibraryImport("user32.dll", SetLastError = true)] private static partial nint GetLastActivePopup(nint hwind);
	[LibraryImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool IsWindowVisible(nint hwind);
	[LibraryImport("user32.dll", SetLastError = true)] protected static partial nint GetWindowLongPtrA(nint hwind, int nIndex);
	[LibraryImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
	[LibraryImport("user32.dll", SetLastError = true)]
	private static partial uint GetWindowThreadProcessId(nint hwind, ref uint lpdwProcessId);
	[DllImport("Psapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern uint GetProcessImageFileName(nint hProcess, [Out] StringBuilder lpImageFileName, [MarshalAs(UnmanagedType.U4)] int nSize);
	[LibraryImport("kernel32.dll", SetLastError = true)]
	private static partial nint OpenProcess(int dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

	public static string GetProcessApplicationPathFromWHandle(nint handle) => GetProcessApplicationPath(GetProcessId(handle));
	public static string GetProcessApplicationPath(uint processId)
	{
		var builder = new StringBuilder(256);
		var procHandle = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, (int)processId);
		_ = GetProcessImageFileName(procHandle, builder, builder.Capacity);
		return builder.ToString();
	}
	public static uint GetProcessId(nint handle)
	{
		uint procId = 0;
		GetWindowThreadProcessId(handle, ref procId);
		if(procId == 0)
		{
			return 0;
		}
		return procId;
	}
	/// <summary>
	/// Retrieves the titles of all visible windows that are considered Alt-Tabable processes.
	/// </summary>
	/// <remarks>Only windows that are both visible and considered Alt-Tabable are included. Hidden windows or those
	/// not shown in the Alt-Tab dialog are excluded. Some windows, like settings, will be found regardless of if they're open or not.</remarks>
	/// <returns>A list of strings containing the titles of visible windows. The list is empty if no such windows are found.</returns>
	public static List<(string Title, nint Handle)> GetVisibleProcesses()
	{
		var windows = new List<(string Title, nint Handle)>();
		EnumWindows((handle, lParam) =>
		{
			if(IsAltTabable(handle) && IsWindowVisible(handle))
			{
				var len = GetWindowTextLengthA(handle);
				if(len == 0)
				{
					return true;
				}
				var buffer = new StringBuilder(len + 1);
				_ = GetWindowText(handle, buffer, buffer.Capacity);
				windows.Add((buffer.ToString(), handle));
			}
			return true;
		}, nint.Zero);
		return windows;
	}
	private static bool IsAltTabable(nint procHandle)
	{
		var handleWalk = GetAncestor(procHandle, (uint)GAFlags.GA_ROOTOWNER);
		nint handleTry;
		while((handleTry = GetLastActivePopup(handleWalk)) != handleTry)
		{
			if(IsWindowVisible(handleTry))
			{
				break;
			}
			handleWalk = handleTry;
		}
		var longPtrHandle = GetWindowLongPtrA(handleWalk, GWL_EXSTYLE);
		return (handleWalk == procHandle) && ((longPtrHandle & WS_EX_APPWINDOW) != 0 || (longPtrHandle & WS_EX_TOOLWINDOW) == 0);
	}
}
