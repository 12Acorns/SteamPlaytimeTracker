using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Diagnostics;

namespace SteamPlaytimeTracker.Steam.Data.App;

[DebuggerDisplay("{AppData}")]
public sealed class SteamStoreApp
{
	private SteamStoreApp() { }
	public SteamStoreApp(int appId, SteamStoreAppData appData)
	{
		Id = appId;
		AppData = appData;
	}
	public SteamStoreApp(SteamStoreAppData appData) : this((int)appData.StoreData!.AppId, appData) { }
	public static implicit operator SteamStoreApp(SteamStoreAppData appData) => new((int)appData.StoreData!.AppId, appData);

	[Key, DatabaseGenerated(DatabaseGeneratedOption.None)] public int Id { get; set; } 
	public SteamStoreAppData AppData { get; set; }
	[NotMapped] public bool Exists => AppData is not null && AppData.Success && AppData.StoreData is not null;
}
[DebuggerDisplay("{Success} | {StoreData}")]
public sealed class SteamStoreAppData
{
	private SteamStoreAppData() { }

	[JsonConstructor]
	public SteamStoreAppData(bool success, SteamAppStoreDetails storeData) => (Success, StoreData) = (success, storeData);
	[JsonIgnore, Key, DatabaseGenerated(DatabaseGeneratedOption.None)] public int Id { get; set; }
	[JsonPropertyName("success")] public bool Success { get; set; }
	[JsonPropertyName("data")] public SteamAppStoreDetails? StoreData { get; set; } = default!;
}
[DebuggerDisplay("{Name} | {AppId} | {Age} | {IsFree}")]
public sealed class SteamAppStoreDetails
{
	private SteamAppStoreDetails() { }
	[JsonConstructor]
	public SteamAppStoreDetails(string appType, string name, uint appId, int age, bool isFree) =>
		(AppType, Name, AppId, Age, IsFree) = (appType, name, appId, age, isFree);
	[JsonIgnore, Key, DatabaseGenerated(DatabaseGeneratedOption.None)] public int Id { get; set; }
	[JsonPropertyName("type")] public string AppType { get; set; } = string.Empty;
	[JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
	[JsonPropertyName("steam_appid")] public uint AppId { get; set; }
	[JsonPropertyName("required_age")] public int Age { get; set; }
	[JsonPropertyName("is_free")] public bool IsFree { get; set; }
}