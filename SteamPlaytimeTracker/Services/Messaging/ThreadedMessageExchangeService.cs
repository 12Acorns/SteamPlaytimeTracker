using SteamPlaytimeTracker.Utility.Messaging;
using System.Collections.Concurrent;

namespace SteamPlaytimeTracker.Services.Messaging;

internal sealed class ThreadedMessageExchangeService : IMessageExchangeService
{
	private readonly ConcurrentStack<Message> _messages = new();

	public void AddMessage(Message message) => _messages.Push(message);
	public IReadOnlyCollection<Message> GetMessages() => _messages;

	public int MessageCount => _messages.Count;
	public int ErrorMessages => _messages.Count(m => m.Type is MessageType.Error);
	public int InformationMessages => _messages.Count(m => m.Type is MessageType.Information);
}