using SteamPlaytimeTracker.Steam.Data.App;

namespace SteamPlaytimeTracker.DbObject;

public sealed class StoreAppDTO
{
	public StoreAppDTO(SteamStoreApp app)
	{
		if(!app.AppData.Success)
		{
			return;
		}
		Valid = true;
		Type = app.AppData.StoreData.AppType;
		Name = app.AppData.StoreData.AppType;
		MinimumAgeToPlay = app.AppData.StoreData.Age ?? -1;
		IsFree = app.AppData.StoreData.IsFree;
	}

	/// <summary>
	/// If the data fields are valid or not.
	/// </summary>
	public bool Valid { get; } = false;
	/// <summary>
	/// Type of app on steam store
	/// </summary>
	public string? Type { get; }
	/// <summary>
	/// Name of the app
	/// </summary>
	public string? Name { get; set; } = string.Empty;
	/// <summary>
	/// The minimum age required/recommended to play. <see cref="MinimumAgeToPlay"/> is -1 when age cannot be determined/found.
	/// </summary>
	public int MinimumAgeToPlay { get; }
	/// <summary>
	/// If the app is available for free or needs to be paid.<para></para>
	/// <see langword="true"/> when free; otherwise <see langword="fa"/>
	/// </summary>
	public bool IsFree { get; }
}