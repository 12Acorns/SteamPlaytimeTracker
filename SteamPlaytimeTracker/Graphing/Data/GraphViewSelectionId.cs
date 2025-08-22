using System.Diagnostics;

namespace SteamPlaytimeTracker.Graphing.Data;

[DebuggerDisplay("Id = {Id}")]
internal record struct GraphViewSelectionId(uint Id)
{
	public const uint YearPlaytimeId = 0;
	public const uint MonthPlaytimeId = 1;
	public const uint DayPlaytimeId = 2;

	public static GraphViewSelectionId YearPlaytime { get; } = new(YearPlaytimeId);
	public static GraphViewSelectionId MonthPlaytime { get; } = new(MonthPlaytimeId);
	public static GraphViewSelectionId DayPlaytime { get; } = new(DayPlaytimeId);

	public static implicit operator uint(GraphViewSelectionId selectionId) => selectionId.Id;
	public static implicit operator GraphViewSelectionId(uint id) => new(id);
}
