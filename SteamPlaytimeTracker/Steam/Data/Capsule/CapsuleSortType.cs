namespace SteamPlaytimeTracker.Steam.Data.Capsule;

[Flags]
internal enum CapsuleSortType : byte
{
	Playtime = 0,
	Name = 1,

	Ascending = 2,
}
