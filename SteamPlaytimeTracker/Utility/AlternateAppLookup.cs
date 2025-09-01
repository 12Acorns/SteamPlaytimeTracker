using SteamPlaytimeTracker.DbObject;
using SteamPlaytimeTracker.Steam.Data.App;
using System.Diagnostics.CodeAnalysis;

namespace SteamPlaytimeTracker.Utility;

internal sealed class AlternateAppLookup : IEqualityComparer<SteamAppEntry>, IAlternateEqualityComparer<SteamApp, SteamAppEntry>
{
	internal static AlternateAppLookup Instance { get; } = new();

	public SteamAppEntry Create(SteamApp alternate)
	{
		throw new NotImplementedException();
	}

	public bool Equals(SteamApp alternate, SteamAppEntry other)
	{
		if(other == null)
		{
			return false;
		}
		return alternate.AppId == other.SteamApp.AppId;
	}
	public bool Equals(SteamAppEntry? x, SteamAppEntry? y)
	{
		if(x is null || y is null)
		{
			return false;
		}
		return x.SteamApp.AppId == y.SteamApp.AppId;
	}

	public int GetHashCode(SteamApp alternate) => (int)alternate.AppId;
	public int GetHashCode([DisallowNull] SteamAppEntry obj) => (int)obj.SteamApp.AppId;
}
