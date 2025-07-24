using Serilog;

namespace SteamPlaytimeTracker;
internal static class LoggingService
{
	public static readonly ILogger Logger = new LoggerConfiguration()
			.WriteTo.Async(x => x.File("logs/SteamPlaytimeTracker.log", rollingInterval: RollingInterval.Day))
			.CreateLogger();
}
