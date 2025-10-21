using System.Diagnostics;

namespace SteamPlaytimeTracker.Utility;

internal sealed class QuickTime
{
	private readonly long _timeStamp;
	private readonly List<(TimeSpan ElapsedFromStart, Func<TimeSpan, TimeSpan, TimeSpan, string> LogMessage)> _segments;
	private TimeSpan endTime = TimeSpan.Zero;

	public QuickTime()
	{
		_segments = new(32);
		_timeStamp = Stopwatch.GetTimestamp();
	}

	public void Stop() => endTime = Stopwatch.GetElapsedTime(_timeStamp);
	public void Time(Func<TimeSpan, TimeSpan, TimeSpan, string> LogMessage)
	{
		_segments.Add((Stopwatch.GetElapsedTime(_timeStamp), LogMessage));
	}
	public void LogAllTimes()
	{
		var total = Stopwatch.GetElapsedTime(_timeStamp);
		if(endTime == TimeSpan.Zero)
			endTime = total;

		var previousElapsedTime = TimeSpan.Zero;
		foreach(var (elapsedFromStart, logMessage) in _segments)
		{
			Debug.WriteLine(logMessage(endTime, elapsedFromStart, previousElapsedTime));
			previousElapsedTime = elapsedFromStart;
		}
		Debug.WriteLine($"[QuickTime] Total time: {endTime.TotalMilliseconds:n0}ms");
	}
}
