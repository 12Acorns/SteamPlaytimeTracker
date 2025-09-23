using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.Steam.Data.App;
using System.Diagnostics.CodeAnalysis;

namespace SteamPlaytimeTracker.Utility;

internal sealed class AlternateAppLookup : IEqualityComparer<SteamAppEntry>, IAlternateEqualityComparer<SteamStoreAppData, SteamAppEntry>
{
	internal static AlternateAppLookup Instance { get; } = new();

	public SteamAppEntry Create(SteamStoreAppData alternate)
	{
		throw new NotImplementedException();
	}

	public bool Equals(SteamStoreAppData alternate, SteamAppEntry other)
	{
		if(other == null)
		{
			return false;
		}
		return alternate.StoreData.AppId == other.SteamApp.AppId;
	}
	public bool Equals(SteamAppEntry? x, SteamAppEntry? y)
	{
		if(x is null || y is null)
		{
			return false;
		}
		return x.SteamApp.AppId == y.SteamApp.AppId;
	}

	public int GetHashCode(SteamStoreAppData alternate) => (int)alternate.StoreData.AppId;
	public int GetHashCode([DisallowNull] SteamAppEntry obj) => (int)obj.SteamApp.AppId;
}
