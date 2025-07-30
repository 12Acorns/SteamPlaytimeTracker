namespace SteamPlaytimeTracker.DbObject;

internal class PlaytimeSliceDTO
{
	public PlaytimeSliceDTO(DateTimeOffset startTime, TimeSpan sessionTime, uint appId) =>
		(StartTimeTicks, StartTimeOffsetMinutes, SessionTimeTicks, AppId) =
		(startTime.Ticks, (short)startTime.TotalOffsetMinutes, sessionTime.Ticks, appId);
	public PlaytimeSliceDTO(long startTimeTicks, short startTimeOffsetMinutes, long sessionTimeTicks, uint appId) =>
		(StartTimeTicks, StartTimeOffsetMinutes, SessionTimeTicks, AppId) =
		(startTimeTicks, startTimeOffsetMinutes, sessionTimeTicks, appId);

	public int PlaytimeSliceDTOId { get; set; }
	public long StartTimeTicks { get; set; }
	public short StartTimeOffsetMinutes { get; set; }
	public long SessionTimeTicks { get; set; }
	public uint AppId { get; set; }
}
