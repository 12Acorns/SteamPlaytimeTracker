using Config.Net;

namespace SteamPlaytimeTracker.SelfConfig.Data;

public interface IStyleData
{
	[Option(DefaultValue = "Segoe UI", Alias = nameof(CurrentFont))]
	public string CurrentFont { get; set; }
	[Option(DefaultValue = "Dark Theme", Alias = nameof(CurrentStyle))]
	public string CurrentStyle { get; set; }
}
