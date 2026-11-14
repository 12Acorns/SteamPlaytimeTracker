using System.ComponentModel;

namespace SteamPlaytimeTracker.SelfConfig.Data;

public interface IUpdaterData
{
	[DefaultValue(false)]
	public bool AutoUpdate { get; set; }
}