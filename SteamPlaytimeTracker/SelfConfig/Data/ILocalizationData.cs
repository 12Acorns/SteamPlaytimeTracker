using System.ComponentModel;

namespace SteamPlaytimeTracker.SelfConfig.Data;

public interface ILocalizationData
{
	[DefaultValue("en-GB")]
	public string LanguageCode { get; set; }
}