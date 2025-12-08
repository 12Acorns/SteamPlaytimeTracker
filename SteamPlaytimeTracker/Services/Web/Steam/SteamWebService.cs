using Microsoft.Extensions.DependencyInjection;
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
	private readonly ILogger _logger;

	public SteamWebService(ILogger logger, IHttpClientFactory clientFactory, ResiliencePipelineProvider<string> resiliencePipeline)
	{
		_logger = logger;
		_clientFactory = clientFactory;
		_resiliencePipeline = resiliencePipeline;
	}


	public async ValueTask<OneOf<SteamStoreAppData, ParseResult, HttpStatusCode>> GetAppDetails(uint appId, CancellationToken token = default)
	{
		if(token == default)
		{
			token = ApplicationEndAsyncLifetimeService.Default.CancellationToken;
		}
		var cache = App.ServiceProvider.GetService<ICacheManager>();
		if(cache == null)
		{
			_logger.Error($"Failed to get {nameof(ICacheManager)} from ServiceProvider.");
			return ParseResult.UnkownError;
		}

		var idStr = appId.ToString();
		return await cache.GetAsync<OneOf<SteamStoreAppData, ParseResult, HttpStatusCode>>(idStr, cacheTime: 15, async () =>
		{
			try
			{
				var client = _clientFactory.CreateClient(GlobalData.SteamHttpClientKey);
				var response = await _resiliencePipeline.GetPipeline(GlobalData.SteamHttpPipelineKey)
						.ExecuteAsync(async ct => await client.GetAsync($"appdetails?appids={idStr}", token).ConfigureAwait(false), token).ConfigureAwait(false);

				if(!response.IsSuccessStatusCode)
				{
					_logger.Error("Failed to fetch app details from Steam API. Status code: {0}. Response: {1}. Id: {2}",
						response.StatusCode, response.ToString(), appId);
					return response.StatusCode;
				}
				var contentStream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
				_logger.Verbose("Successfully fetched steam app store data.");

				var jObject = await JsonNode.ParseAsync(contentStream, cancellationToken: token).ConfigureAwait(false);
				var child = jObject![appId.ToString()];
				return child.Deserialize<SteamStoreAppData>(_serializerOptions)!;
			}
			catch(JsonException jEx)
			{
				_logger.Error(jEx, "Failed to parse app details from Steam API. Parse Result: {0}", ParseResult.FailedToParse);
				return ParseResult.FailedToParse;
			}
			catch(Exception ex)
			{
				_logger.Error(ex, "Failed to fetch app details from Steam API. Parse Result: {0}", ParseResult.UnkownError);
				return ParseResult.UnkownError;
			}
		});
	}
}