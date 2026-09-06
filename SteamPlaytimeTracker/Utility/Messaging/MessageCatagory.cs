namespace SteamPlaytimeTracker.Utility.Messaging;

internal sealed record MessageCatagory
{
	public MessageCatagory(string tag, MessageCatagory? subCatagory = null)
	{
		Tag = tag;
		SubCatagory = subCatagory;
	}
	private MessageCatagory() { }

	public string Tag { get; init; }
	public MessageCatagory? SubCatagory { get; set; }

	public static implicit operator MessageCatagory(string tag) => new(tag);

	public override string ToString()
	{
		var totalLength = 0;
		var current = this;
		while(true)
		{
			totalLength += current.Tag.Length;
			if(current.SubCatagory != null)
			{
				totalLength += 1;
				current = current.SubCatagory;
				continue;
			}
			break;
		}
		return string.Create(totalLength, this, (buffer, data) =>
		{
			var currentLength = 0;
			var prevLength = currentLength;
			var current = data;
			while(true)
			{
				currentLength += current.Tag.Length;
				current.Tag.CopyTo(buffer[prevLength..currentLength]);
				prevLength = currentLength;
				if(current.SubCatagory != null)
				{
					buffer[currentLength] = '.';
					currentLength += 1;
					prevLength += 1;
					current = current.SubCatagory;
					continue;
				}
				break;
			}
		});
	}
}
