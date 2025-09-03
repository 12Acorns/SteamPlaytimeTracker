using SteamPlaytimeTracker.Steam.Data.Playtime;
using System.ComponentModel.DataAnnotations;
using SteamPlaytimeTracker.Steam.Data.App;
using System.Diagnostics;

namespace SteamPlaytimeTracker.DbObject;

[DebuggerDisplay("{Id} | {SteamApp} | {PlaytimeSlices.Count}")]
internal sealed class SteamAppEntry
{
	[Key]
	public int Id { get; set; }
	public int SteamAppId { get; set; }
	public required SteamApp SteamApp { get; set; }
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
