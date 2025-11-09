using System.ComponentModel;
using Serilog.Events;

namespace SteamPlaytimeTracker.SelfConfig.Data;

public interface ILogData
{
	[DefaultValue(nameof(LogEventLevel.Information))]
	public LogEventLevel LogLevel { get; set; }
}