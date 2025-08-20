using Serilog;
using Serilog.Core;

namespace SteamPlaytimeTracker;

internal static class LoggingService
{
	public static readonly Logger Logger = new LoggerConfiguration()
			.WriteTo.Async(x => x.File("logs/SteamPlaytimeTracker.log", rollingInterval: RollingInterval.Day))
			.CreateLogger();
}
