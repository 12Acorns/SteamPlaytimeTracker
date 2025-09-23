using System.ComponentModel.DataAnnotations.Schema;
using SteamPlaytimeTracker.Steam.Data.Playtime;
using System.ComponentModel.DataAnnotations;
using SteamPlaytimeTracker.Steam.Data.App;
using System.Diagnostics;

namespace SteamPlaytimeTracker.DbObject;

[DebuggerDisplay("{SteamApp} || {PlaytimeSlices.Count}")]
internal sealed class SteamAppEntry
{
	[Key]
	public int Id { get; set; }
	public SteamStoreApp? StoreDetails { get; set; }
	[NotMapped] public SteamApp? SteamApp
	{
		get
		{
			if(StoreDetails is null)
			{
				return default;
			}
			return new() { Id = StoreDetails.Id, AppId = StoreDetails.AppData.StoreData.AppId, Name = StoreDetails.AppData.StoreData.Name ?? "N/A" };
		}
	}
	//public required SteamApp SteamApp { get; set; }
	public List<PlaytimeSlice> PlaytimeSlices { get; set; } = [];

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(SteamApp.GetHashCode());
		foreach(var segment in PlaytimeSlices)
		{
			hash.Add(segment.GetHashCode());
		}
		return hash.ToHashCode();
	}
}
