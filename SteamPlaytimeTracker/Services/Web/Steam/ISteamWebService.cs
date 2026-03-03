using SteamPlaytimeTracker.Steam.Data.App;
using System.Net;
using OneOf;

namespace SteamPlaytimeTracker.Services.Web.Steam;
internal interface ISteamWebService
{
	public ValueTask<OneOf<SteamStoreAppData, ParseResult, HttpStatusCode>> GetAppDetails(uint appId, CancellationToken token = default);
}
