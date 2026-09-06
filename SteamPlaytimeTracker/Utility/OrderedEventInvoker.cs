namespace SteamPlaytimeTracker.Utility;

internal sealed class OrderedEventInvoker<TEventArg> where TEventArg : EventArgs
{
	private readonly SortedDictionary<int, EventHandler<TEventArg>> _executionOrderedEvents = [];

	public void Invoke(object? sender, TEventArg e)
	{
		foreach(var priority in _executionOrderedEvents)
		{
			priority.Value?.Invoke(sender, e);
		}
	}
	public void Subscribe(OrderedEventPriority priority, EventHandler<TEventArg> @event) => Subscribe((int)priority, @event);
	public void UnSubscribe(OrderedEventPriority priority, EventHandler<TEventArg> @event) => UnSubscribe((int)priority, @event);
	public void Subscribe(int priority, EventHandler<TEventArg> @event)
	{
		if(!_executionOrderedEvents.ContainsKey(priority))
		{
			_executionOrderedEvents.Add(priority, null);
		}
		_executionOrderedEvents[priority] += @event;
	}

	public void UnSubscribe(int priority, EventHandler<TEventArg> @event)
	{
		if(!_executionOrderedEvents.ContainsKey(priority))
		{
			throw new NullReferenceException($"No handler found for priority {priority}");
		}
		_executionOrderedEvents[priority] -= @event;
	}
}
internal enum OrderedEventPriority
{
	Default = 100,
	Minimum = int.MaxValue,
	Maximum = int.MinValue, 
}