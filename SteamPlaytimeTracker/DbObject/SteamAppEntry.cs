using System.ComponentModel.DataAnnotations.Schema;
using SteamPlaytimeTracker.Steam.Data.Playtime;
using System.ComponentModel.DataAnnotations;
using SteamPlaytimeTracker.Steam.Data.App;
using System.Diagnostics;

namespace SteamPlaytimeTracker.DbObject;

[DebuggerDisplay("{Id} | {SteamApp} || {PlaytimeSlices.Count}")]
internal sealed class SteamAppEntry
{
	[Key] public int Id { get; set; }
	public required SteamStoreApp StoreDetails { get; set; }
	[NotMapped] public SteamApp? SteamApp
	{
		get
		{
			if(!StoreDetails.Exists)
			{
				return default;
			}
			return new() { AppId = StoreDetails.AppData.StoreData!.AppId, Name = StoreDetails.AppData.StoreData.Name ?? "N/A" };
		}
	}
	public List<PlaytimeSlice> PlaytimeSlices { get; set; } = [];
	[NotMapped] public long TotalPlaytime
	{
		get
		{
			if(PlaytimeSlices.Count == 0)
			{
				return 0;
			}
			if(field == long.MinValue)
			{
				field = PlaytimeSlices.Sum(x => x.SessionLength.Ticks);
			}
			return field;
		}
		private set;
	} = long.MinValue;
	[NotMapped] public double TotalPlaytimeHours => TimeSpan.FromTicks(TotalPlaytime).TotalHours;

	public override int GetHashCode()
	{
		var hash = new HashCode();
		hash.Add(StoreDetails.Id);
		foreach(var segment in PlaytimeSlices.OrderBy(x => x.SessionStart))
		{
			hash.Add(segment.GetHashCode());
		}
		return hash.ToHashCode();
	}
}
