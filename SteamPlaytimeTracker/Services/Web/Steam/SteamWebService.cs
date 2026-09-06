using SteamPlaytimeTracker.Services.Lifetime;
using SteamPlaytimeTracker.Utility.Converter;
using SteamPlaytimeTracker.Steam.Data.App;
using SteamPlaytimeTracker.Utility.Cache;
using SteamPlaytimeTracker.Extensions;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Net.Http;
using Polly.Registry;
using System.Net;
using Serilog;
using OneOf;

namespace SteamPlaytimeTracker.Services.Web.Steam;

internal sealed class SteamWebService : ISteamWebService
{
	private static readonly JsonSerializerOptions _serializerOptions = new()
	{
		UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
		Converters =
		{
			new StringToIntJsonConverter()
		}
	};

	private readonly ResiliencePipelineProvider<string> _resiliencePipeline;
	private readonly IHttpClientFactory _clientFactory;
	private readonly ICacheManager _cacheManager;
	private readonly ILogger _logger;

	public SteamWebService(ILogger logger, IHttpClientFactory clientFactory, ICacheManager cacheManager, 
		ResiliencePipelineProvider<string> resiliencePipeline)
	{
		_logger = logger;
		_clientFactory = clientFactory;
		_cacheManager = cacheManager;
		_resiliencePipeline = resiliencePipeline;
	}


	public async ValueTask<OneOf<SteamStoreAppData, ParseResult, HttpStatusCode>> GetAppDetails(uint appId, CancellationToken token = default)
	{
		if(token == default)
		{
			token = ApplicationEndAsyncLifetimeService.Default.CancellationToken;
		}

		var idStr = appId.ToString();
		return await _cacheManager.GetAsync<OneOf<SteamStoreAppData, ParseResult, HttpStatusCode>>(idStr, cacheTimeMinutes: 15, async token =>
		{
			try
			{
				var client = _clientFactory.CreateClient(GlobalData.SteamHttpClientKey);
				var response = await _resiliencePipeline.GetPipeline(GlobalData.SteamHttpPipelineKey)
						.ExecuteAsync(async ct => await client.GetAsync($"appdetails?appids={idStr}", token).ConfigureAwait(false), token).ConfigureAwait(false);

				if(!response.IsSuccessStatusCode)
				{
					_logger.Error("Failed to fetch app details from Steam API. Status code: {0}. Response: {1}. Id: {2}",
						response.StatusCode, response.ToString(), idStr);
					return response.StatusCode;
				}
				var contentStream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
				_logger.Verbose("Successfully fetched steam app store data for app {0}.", idStr);

				var jObject = await JsonNode.ParseAsync(contentStream, cancellationToken: token).ConfigureAwait(false);
				if(jObject == null)
				{
					_logger.Error("Failed to parse app details from Steam API for app {0}. Parse Result: {1}", idStr, ParseResult.FailedToParse);
					return ParseResult.FailedToParse;
				}
				var child = jObject[appId.ToString()];
				return child.Deserialize<SteamStoreAppData>(_serializerOptions)!;
			}
			catch(JsonException jEx)
			{
				_logger.Error(jEx, "Failed to parse app details from Steam API for app {0}. Parse Result: {1}", idStr, ParseResult.FailedToParse);
				return ParseResult.FailedToParse;
			}
			catch(Exception ex) when(ex is not OperationCanceledException)
			{
				_logger.Error(ex, "Failed to fetch app details from Steam API for app {0}. Parse Result: {1}", idStr, ParseResult.UnkownError);
				return ParseResult.UnkownError;
			}
		}, token: token);
	}
}