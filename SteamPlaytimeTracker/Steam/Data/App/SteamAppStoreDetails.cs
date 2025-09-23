using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SteamPlaytimeTracker.Steam.Data.App;

public sealed class SteamStoreApp
{
	public SteamStoreApp(int appId, SteamStoreAppData appData)
	{
		Id = appId;
		AppData = appData;
	}
	public SteamStoreApp(uint appId, SteamStoreAppData appData)
	{
		Id = (int)appId;
		AppData = appData;
	}
	public SteamStoreApp(SteamStoreAppData appData) : this(appData.StoreData.AppId, appData) { }
	private SteamStoreApp() { }
	public static implicit operator SteamStoreApp(SteamStoreAppData appData) => new(appData.StoreData.AppId, appData);

	[Key, DatabaseGenerated(DatabaseGeneratedOption.None)] public int Id { get; set; } 
	public SteamStoreAppData AppData { get; set; }
}
public sealed class SteamStoreAppData
{
	[JsonConstructor]
	public SteamStoreAppData(bool success, SteamAppStoreDetails storeData) => (Success, StoreData) = (success, storeData);
	private SteamStoreAppData() { }
	[JsonIgnore, Key, DatabaseGenerated(DatabaseGeneratedOption.None)] public int Id { get; set; }
	[JsonPropertyName("success")] public bool Success { get; set; }
	[JsonPropertyName("data")] public SteamAppStoreDetails StoreData { get; set; } = default!;
}
public sealed class SteamAppStoreDetails
{
	public SteamAppStoreDetails(string appType, string name, uint appId, int age, bool isFree) =>
		(AppType, Name, AppId, Age, IsFree) = (appType, name, appId, age, isFree);
	private SteamAppStoreDetails() { }
	[JsonIgnore, Key, DatabaseGenerated(DatabaseGeneratedOption.None)] public int Id { get; set; }
	[JsonPropertyName("type")] public string AppType { get; set; } = string.Empty;
	[JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
	[JsonPropertyName("steam_appid")] public uint AppId { get; set; }
	[JsonPropertyName("required_age")] public int Age { get; set; }
	[JsonPropertyName("is_free")] public bool IsFree { get; set; }
}
	