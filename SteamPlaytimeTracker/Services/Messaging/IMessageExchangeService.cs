using SteamPlaytimeTracker.Utility.Messaging;
using System.Collections.Concurrent;

namespace SteamPlaytimeTracker.Services.Messaging;

interface IMessageExchangeService
{
	public void AddMessage(Message message);
	public IReadOnlyCollection<Message> GetMessages();
}
internal sealed class ThreadedMessageExchangeService : IMessageExchangeService
{
	private readonly ConcurrentStack<Message> _messages = new();

	public void AddMessage(Message message) => _messages.Push(message);
	public IReadOnlyCollection<Message> GetMessages() => _messages;

	public int MessageCount => _messages.Count;
	public int ErrorMessages => _messages.Count(m => m.Type == MessageType.Error);
	public int InformationMessages => _messages.Count(m => m.Type is MessageType.Information);
}