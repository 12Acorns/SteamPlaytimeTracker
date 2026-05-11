namespace SteamPlaytimeTracker.Services.Batching;

public sealed record BatchOptions
{
	public required int MaximumBatchSize { get; init; }
	public required int MaximumRetries { get; init; }
	public required TimeSpan MinimumWaitInterval { get; init; }
	public required TimeSpan MaximumWaitInterval { get; init; }
	public required double BaseGrowthFactor { get; init; }
}