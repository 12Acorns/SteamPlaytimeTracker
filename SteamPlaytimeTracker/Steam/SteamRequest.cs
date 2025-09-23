using Microsoft.Extensions.DependencyInjection;
using OneOf;
using Serilog.Core;
using SteamPlaytimeTracker.Extensions;
using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.Utility.Cache;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SteamPlaytimeTracker.Steam;

internal static class SteamRequest
{
	private const string SteamApiDomainUrl = "https://api.steampowered.com";
	private const string SteamStoreDomainUrl = "https://store.steampowered.com";
	private const string GetAppListUrl = $"{SteamApiDomainUrl}/ISteamApps/GetAppList/v2/";
	private const string SingleAppDetails = $"{SteamStoreDomainUrl}/api/appdetails?appids=";

	private static readonly JsonSerializerOptions _serializerOptions = new()
	{
		UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip
	};
	private static readonly HttpClient _client = new();
	private static readonly Logger _logger = LoggingService.Logger;

	public static async Task<OneOf<AppListResponse, HttpStatusCode, ParseResult>> GetAppListAsync(CancellationToken token = default)
	{
		if(token == default)
		{
			token = ApplicationEndAsyncLifetimeService.Default.CancellationToken;
		}
		try
		{
			var response = await _client.GetAsync(GetAppListUrl, token).ConfigureAwait(false);
			if(!response.IsSuccessStatusCode)
			{
				_logger.Error("Failed to fetch app list from Steam API. Status code: {0}. Responce: {1}", response.StatusCode, response.ToString());
				return response.StatusCode;
			}
			var contentStream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
			_logger.Information("Successfully fetched steam app list data.");
			return await JsonSerializer.DeserializeAsync<AppListResponse>(contentStream, cancellationToken: token).ConfigureAwait(false);
		}
		catch(JsonException jEx)
		{
			_logger.Error(jEx, "ERROR: {0}. Failed to parse app list response from Steam API.", ParseResult.FailedToParse);
			return ParseResult.FailedToParse;
		}
		catch(Exception ex)
		{
			_logger.Error(ex, "ERROR: {0}. Failed to fetch app list from Steam API.", ParseResult.UnkownError);
			return ParseResult.UnkownError;
		}
	}
	public static async ValueTask<OneOf<SteamStoreAppData, ParseResult, HttpStatusCode>> GetAppDetails(uint appId, CancellationToken token = default)
	{
		if(token == default)
		{
			token = ApplicationEndAsyncLifetimeService.Default.CancellationToken;
		}
		var cache = App.ServiceProvider.GetService<ICacheManager>();
		if(cache == null)
		{
			_logger.Error("Failed to get ICacheManager from ServiceProvider.");
			return ParseResult.UnkownError;
		}

		return await cache.GetAsync<OneOf<SteamStoreAppData, ParseResult, HttpStatusCode>>(appId.ToString(), 15, async () =>
		{
			try
			{
				var response = await _client.GetAsync(SingleAppDetails + appId, token).ConfigureAwait(false);
				if(!response.IsSuccessStatusCode)
				{
					_logger.Error("Failed to fetch app details from Steam API. Status code: {0}. Responce: {1}",
						response.StatusCode, response.ToString());
					return response.StatusCode;
				}
				var contentStream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
				_logger.Information("Successfully fetched steam app list data.");

				var jObject = await JsonNode.ParseAsync(contentStream, cancellationToken: token).ConfigureAwait(false);
				var child = jObject![appId.ToString()];
				return child.Deserialize<SteamStoreAppData>(_serializerOptions)!;
			}
			catch(JsonException jEx)
			{
				_logger.Error(jEx, "ERROR: {0}. Failed to parse app details from Steam API.", ParseResult.FailedToParse);
				return ParseResult.FailedToParse;
			}
			catch(Exception ex)
			{
				_logger.Error(ex, "ERROR: {0}. Failed to fetch app details from Steam API.", ParseResult.UnkownError);
				return ParseResult.UnkownError;
			}
		});
	}
}