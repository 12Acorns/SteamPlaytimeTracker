using System.Runtime.CompilerServices;
using Serilog.Sinks.File;
using Serilog.Core;
using System.Text;
using System.IO;
using Serilog;

namespace SteamPlaytimeTracker;

internal static class LoggingService
{
	public static readonly LoggingLevelSwitch LoggingLevelSwitcher = new();

	public static readonly Logger Logger = new LoggerConfiguration()
			.MinimumLevel.ControlledBy(LoggingLevelSwitcher)
			.WriteTo.Async(x => x.File("logs/SteamPlaytimeTracker.log", rollingInterval: RollingInterval.Day, hooks: SerilogHooks.FileCycleHook))
			.CreateLogger();
	public static string CurrentLogFilePath
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			if(string.IsNullOrWhiteSpace(SerilogHooks.FileCycleHook.CurrentLoggingFilePath))
			{
				return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
			}
			if(File.Exists(SerilogHooks.FileCycleHook.CurrentLoggingFilePath))
			{
				return Directory.GetParent(SerilogHooks.FileCycleHook.CurrentLoggingFilePath)!.FullName;
			}
			if(!Directory.Exists(SerilogHooks.FileCycleHook.CurrentLoggingFilePath))
			{
				return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
			}
			return SerilogHooks.FileCycleHook.CurrentLoggingFilePath;
		}
	}
}
public class SerilogHooks
{
	public static SerilogFileCycleHook FileCycleHook { get; } = new SerilogFileCycleHook();
}

public class SerilogFileCycleHook : FileLifecycleHooks
{
	public string? CurrentLoggingFilePath { get; private set; }

	public override Stream OnFileOpened(string path, Stream underlyingStream, Encoding encoding)
	{
		CurrentLoggingFilePath = path;

		return base.OnFileOpened(path, underlyingStream, encoding);
	}
}