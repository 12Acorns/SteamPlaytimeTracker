using OneOf;
using Serilog;
using SteamPlaytimeTracker.Steam.Data;
using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace SteamPlaytimeTracker.Steam;

internal static class SteamRequest
{
	private const string AppListApiUrl = "https://api.steampowered.com/ISteamApps/GetAppList/v2/";

	private static readonly HttpClient _client = new();
	private static readonly ILogger _logger = LoggingService.Logger;

	public static async Task<OneOf<AppListResponse, HttpStatusCode, AppListResponseStatus>> GetAppListAsync()
	{
		try
		{
			var response = await _client.GetAsync(AppListApiUrl).ConfigureAwait(false);
			if(!response.IsSuccessStatusCode)
			{
				_logger.Error("Failed to fetch app list from Steam API. Status code: {StatusCode}", response.StatusCode);
				return response.StatusCode;
			}

			var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
			return JsonSerializer.Deserialize<AppListResponse>(content);
		}
		catch(InvalidOperationException iEx)
		{
			_logger.Error(iEx, "Invalid Json structure");
			return AppListResponseStatus.FailedToParse;
		}
		catch(JsonException jEx)
		{
			_logger.Error(jEx, "Failed to parse app list response from Steam API.");
			return AppListResponseStatus.FailedToParse;
		}
		catch(Exception ex)
		{
			_logger.Error(ex, "Failed to fetch app list from Steam API.");
			return AppListResponseStatus.UnkownError;
		}
	}
}
