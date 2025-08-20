using OneOf;
using Serilog.Core;
using SteamPlaytimeTracker.Steam.Data.App;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace SteamPlaytimeTracker.Steam;

internal static class SteamRequest
{
	private const string SteamApiDomainUrl = "https://api.steampowered.com";
	private const string GetAppListUrl = $"{SteamApiDomainUrl}/ISteamApps/GetAppList/v2/";

	private static readonly HttpClient _client = new();
	private static readonly Logger _logger = LoggingService.Logger;

	public static async Task<OneOf<AppListResponse, HttpStatusCode, AppListResponseStatus>> GetAppListAsync(CancellationToken token = default)
	{
		try
		{
			var response = await _client.GetAsync(GetAppListUrl, token).ConfigureAwait(false);
			if(!response.IsSuccessStatusCode)
			{
				_logger.Error("Failed to fetch app list from Steam API. Status code: {StatusCode}", response.StatusCode);
				return response.StatusCode;
			}

			var contentStream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
			_logger.Information("Successfully fetched steam game catalogue data.");
			return await JsonSerializer.DeserializeAsync<AppListResponse>(contentStream, cancellationToken: token).ConfigureAwait(false);
		}
		catch(JsonException jEx)
		{
			_logger.Error(jEx, "Failed to parse app list response from Steam API. Exception: {0}", AppListResponseStatus.FailedToParse);
			return AppListResponseStatus.FailedToParse;
		}
		catch(Exception ex)
		{
			_logger.Error(ex, "Failed to fetch app list from Steam API. Exception: {0}", AppListResponseStatus.UnkownError);
			return AppListResponseStatus.UnkownError;
		}
	}
}