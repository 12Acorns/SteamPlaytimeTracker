namespace SteamPlaytimeTracker.Services.Batching;

internal interface IBatchUpdateService<T>
{
	public void Enqueue(T item);
	public event Action<IList<T>> BatchReady;
	public void StartProcessing(CancellationToken token);
}