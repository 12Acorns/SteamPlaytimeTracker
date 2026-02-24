namespace SteamPlaytimeTracker.Utility.Messaging;

internal readonly record struct Message(MessageType Type, string? Catagory, string Content);