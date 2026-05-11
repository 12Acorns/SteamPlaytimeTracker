using SteamPlaytimeTracker.Utility.Messaging;

namespace SteamPlaytimeTracker.Services.Messaging;

interface IMessageExchangeService
{
	public void AddMessage(Message message);
	public IReadOnlyCollection<Message> GetMessages();
}