using SteamPlaytimeTracker.Steam.Data.Playtime;

namespace SteamPlaytimeTracker.Utility;

internal static class PlaytimeUtility
{
	public static List<PlaytimeSlice> CorrectIntervalOverlap(List<PlaytimeSlice> intervals)
	{
		var corrected = intervals.OrderBy(x => x.SessionStart).ToList();
		var count = intervals.Count;
		do
		{
			count = corrected.Count;
			corrected = CorrectIntervalOverlapImpl(corrected);
		} while(count != intervals.Count);
		return intervals;
	}
	private static List<PlaytimeSlice> CorrectIntervalOverlapImpl(List<PlaytimeSlice> intervals)
	{
		if(intervals.Count == 1)
		{
			return intervals;
		}
		var finalSlices = new List<PlaytimeSlice>();
		for(int i = intervals.Count - 1; i > 0; i--)
		{
			var curr = intervals[i];
			var prev = intervals[i - 1];
			if(curr.SessionStart > prev.SessionEnd)
			{
				finalSlices.Add(curr);
			}
			else if(prev.SessionEnd > curr.SessionEnd)
			{
				finalSlices.Add(prev);
			}
			else if(curr.SessionEnd > prev.SessionEnd && curr.SessionStart <= prev.SessionEnd)
			{
				var newSlice = new PlaytimeSlice()
				{
					AppId = curr.AppId,
					SessionStart = prev.SessionStart,
					SessionLength = curr.SessionEnd - prev.SessionStart,
				};
				finalSlices.Add(newSlice);
			}
		}
		if(intervals[1].SessionStart > intervals[0].SessionEnd)
		{
			finalSlices.Add(intervals[0]);
		}
		return finalSlices;
	}
}
