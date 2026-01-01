using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.DbObject;
using System.Net;
using OneOf;

namespace SteamPlaytimeTracker.Services.App;

// TODO: stream apps
// Provide method GetStoreAppDetailsAsync -> ValueTask<OneOf<SteamStoreAppData?, ParseResult, HttpStatusCode>>
// Method will call a seperate service which provides all local apps, this service could be called ILocalSteamAppService
// Method will check if app exists, if not return a null SteamStoreAppData in OneOf, else return the result of the web request to the app
internal interface IAppService
{
	public ValueTask<List<SteamAppEntry>> AllEntries(CancellationToken token = default);
	public ValueTask<SteamAppEntry?> GetEntryAsync(uint appId, CancellationToken token = default);
	public ValueTask<IEnumerable<SteamStoreAppData>> GetLocalAppsAsync(CancellationToken token = default);
	public ValueTask<OneOf<SteamStoreAppData?, ParseResult, HttpStatusCode>> GetStoreAppDetailsAsync(uint appId, CancellationToken token = default);
}
