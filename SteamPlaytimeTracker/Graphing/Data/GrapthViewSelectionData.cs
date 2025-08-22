namespace SteamPlaytimeTracker.Graphing.Data;

internal readonly record struct GrapthViewSelectionData(
	GraphViewSelectionId Id,
	string Title,
	bool IsSelectedByDefault);