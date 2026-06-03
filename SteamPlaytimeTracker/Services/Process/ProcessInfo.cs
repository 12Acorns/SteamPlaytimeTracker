namespace SteamPlaytimeTracker.Services.Process;

internal sealed record ProcessInfo(nint WHandle, uint PID, string? ProcessName, string? ApplicationPath);