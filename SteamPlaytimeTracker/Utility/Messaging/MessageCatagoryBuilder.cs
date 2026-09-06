using AppServices.Common.ReleaseDetails;

namespace SteamPlaytimeTracker.Utility.Messaging;

internal sealed class MessageCatagoryBuilder
{
	private readonly MessageCatagory _root;
	private MessageCatagory _tail;

	public MessageCatagoryBuilder(string rootTag) { _tail = _root = new(rootTag); }
	private MessageCatagoryBuilder() { _tail = _root = default!; }

	public MessageCatagoryBuilder Add(string tag)
	{
		_tail = _tail.SubCatagory = new MessageCatagory(tag);
		return this;
	}
	public MessageCatagory Build()
	{
		return _root;
	}

	public static MessageCatagory? Parse(string wholeTag, char seperator = '.')
	{
		if(!wholeTag.Contains(seperator))
		{
			return new MessageCatagory(wholeTag);
		}

		MessageCatagoryBuilder? builder = null;
		var skipNewBuilder = false;
		foreach(var tag in wholeTag.Split(seperator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
		{
			if(!skipNewBuilder)
			{
				builder = new MessageCatagoryBuilder(tag);
				skipNewBuilder = true;
				continue;
			}
			builder!.Add(tag);
		}
		return builder?.Build();
	}
}