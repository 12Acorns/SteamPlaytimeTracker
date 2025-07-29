using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.Steam.Data.Playtime;

namespace SteamPlaytimeTracker.DbObject.Conversions;

internal static class DTOConversionExtensions
{
	public static SteamApp FromDTO(this SteamAppDTO appDTO) => new SteamApp(appDTO.AppId, appDTO.AppName);
	public static SteamAppDTO ToDTO(this SteamApp app) => new SteamAppDTO(app.Id, app.Name);

	public static PlaytimeSlice FromDTO(this PlaytimeSliceDTO sliceDTO) => new PlaytimeSlice(
		new DateTimeOffset(sliceDTO.StartTimeTicks, 
			TimeSpan.FromMinutes(sliceDTO.StartTimeOffsetMinutes)),
		new TimeSpan(sliceDTO.SessionTimeTicks), sliceDTO.AppId);
	public static PlaytimeSliceDTO ToDTO(this PlaytimeSlice slice) => new PlaytimeSliceDTO(slice.SessionStart, slice.SessionLength, slice.AppId);
}
