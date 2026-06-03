using SteamPlaytimeTracker.Services.Process;

namespace SteamPlaytimeTracker.Utility.ProcessTracking;

internal sealed record ProcessTrackingInfo(ProcessInfo Info, bool Tracked);