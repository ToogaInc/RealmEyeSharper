using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RealmAspNet.RealmEye;
using RealmAspNet.RealmEye.Definitions;
using RealmAspNet.RealmEye.Definitions.Player;

namespace RealmAspNet.Controllers
{
	[Route("api/realmeye/player")]
	[ApiController]
	public class RealmEyePlayerController : ControllerBase
	{
		private readonly ILogger<RealmEyePlayerController> _logger;
		private readonly JsonSerializerOptions _defaultSerializationOption;
		private readonly JsonSerializerOptions _prettifySerializationOption;

		/// <summary>
		/// Creates a new controller for this API.
		/// </summary>
		/// <param name="logger">The logging object.</param>
		public RealmEyePlayerController(ILogger<RealmEyePlayerController> logger)
		{
			_logger = logger;
			_prettifySerializationOption = new JsonSerializerOptions
			{
				IgnoreNullValues = true, 
				WriteIndented = true, 
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};
			_defaultSerializationOption = new JsonSerializerOptions
			{
				IgnoreNullValues = true, 
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};
		}

		/// <summary>
		/// Gets the player's basic information (the player's RealmEye "homepage").
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The API response.</returns>
		[HttpGet("basics/{name}")]
		public async Task<RealmEyeResponse> GetBasicDataAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapePlayerProfileAsync(name), "GetBasicDataAsync");

		/// <summary>
		/// Gets the player's basic information (the player's RealmEye "homepage"). This reads from the query string
		/// instead of a direct parameter.
		/// </summary>
		/// <returns>The API response.</returns>
		[HttpGet("basics")]
		public async Task<string> GetBasicDataAsync()
		{
			var name = GetNameFromQuery(HttpContext.Request.Query);
			if (string.IsNullOrEmpty(name))
				return ProcessJsonSerialization(
					HttpContext.Request.Query,
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound)
				);

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapePlayerProfileAsync(name), "GetBasicDataAsync+"
			);

			return resp is PlayerData r
				? ProcessJsonSerialization(HttpContext.Request.Query, r)
				: ProcessJsonSerialization(HttpContext.Request.Query, resp);
		}

		/// <summary>
		/// Gets the player's pet yard information.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The API response.</returns>
		[HttpGet("petyard/{name}")]
		public async Task<RealmEyeResponse> GetPetYardAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapePetYardAsync(name), "GetPetYardAsync");

		/// <summary>
		/// Gets the player's pet yard information. This reads from the query string instead of a direct parameter.
		/// </summary>
		/// <returns>The API response.</returns>
		[HttpGet("petyard")]
		public async Task<string> GetPetYardAsync()
		{
			var name = GetNameFromQuery(HttpContext.Request.Query);
			if (string.IsNullOrEmpty(name))
				return ProcessJsonSerialization(
					HttpContext.Request.Query,
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound)
				);

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapePetYardAsync(name),
				"GetPetYardAsync+"
			);

			return resp is PetYardData r
				? ProcessJsonSerialization(HttpContext.Request.Query, r)
				: ProcessJsonSerialization(HttpContext.Request.Query, resp);
		}

		/// <summary>
		/// Gets the player's graveyard information.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="amt">The number of entries to fetch.</param>
		/// <returns>The API response.</returns>
		[HttpGet("graveyard/{name}/{amt:int?}")]
		public async Task<RealmEyeResponse> ScrapeGraveyardAsync(string name, int amt = 70)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapeGraveyardAsync(name, amt), "ScrapeGraveyardAsync");

		/// <summary>
		/// Gets the player's graveyard information. This reads from the query string instead of a direct parameter.
		/// </summary>
		/// <returns>The API response.</returns>
		[HttpGet("graveyard")]
		public async Task<string> GetGraveyardAsync()
		{
			var name = GetNameFromQuery(HttpContext.Request.Query);
			if (string.IsNullOrEmpty(name))
				return ProcessJsonSerialization(
					HttpContext.Request.Query,
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound)
				);

			var amt = HttpContext.Request.Query.TryGetValue("amt", out var unparsedNum)
				? int.TryParse(unparsedNum, out var parsedNum)
					? parsedNum
					: 70
				: 70;

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapeGraveyardAsync(name, amt),
				"ScrapeGraveyardAsync+"
			);

			return resp is GraveyardData r
				? ProcessJsonSerialization(HttpContext.Request.Query, r)
				: ProcessJsonSerialization(HttpContext.Request.Query, resp);
		}

		/// <summary>
		/// Gets the player's graveyard summary information.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The API response.</returns>
		[HttpGet("graveyardsummary/{name}")]
		public async Task<RealmEyeResponse> GetGraveyardSummaryAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapeGraveyardSummaryAsync(name),
				"GetGraveyardSummaryAsync");

		/// <summary>
		/// Gets the player's graveyard summary information. This reads from the query string instead of a direct
		/// parameter.
		/// </summary>
		/// <returns>The API response.</returns>
		[HttpGet("graveyardsummary")]
		public async Task<string> GetGraveyardSummaryAsync()
		{
			var name = GetNameFromQuery(HttpContext.Request.Query);
			if (string.IsNullOrEmpty(name))
				return ProcessJsonSerialization(
					HttpContext.Request.Query,
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound)
				);

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapeGraveyardSummaryAsync(name),
				"GetGraveyardSummaryAsync+"
			);

			return resp is GraveyardSummaryData r
				? ProcessJsonSerialization(HttpContext.Request.Query, r)
				: ProcessJsonSerialization(HttpContext.Request.Query, resp);
		}

		/// <summary>
		/// Gets the player's name history information.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The API response.</returns>
		[HttpGet("namehistory/{name}")]
		public async Task<RealmEyeResponse> GetNameHistoryAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapeNameHistoryAsync(name), "GetNameHistoryAsync");

		/// <summary>
		/// Gets the player's name history information. This reads from the query string instead of a direct parameter.
		/// </summary>
		/// <returns>The API response.</returns>
		[HttpGet("namehistory")]
		public async Task<string> GetNameHistoryAsync()
		{
			var name = GetNameFromQuery(HttpContext.Request.Query);
			if (string.IsNullOrEmpty(name))
				return ProcessJsonSerialization(
					HttpContext.Request.Query,
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound)
				);

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapeNameHistoryAsync(name),
				"GetNameHistoryAsync+"
			);

			return resp is NameHistoryData r
				? ProcessJsonSerialization(HttpContext.Request.Query, r)
				: ProcessJsonSerialization(HttpContext.Request.Query, resp);
		}

		/// <summary>
		/// Gets the player's rank history information.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The API response.</returns>
		[HttpGet("rankhistory/{name}")]
		public async Task<RealmEyeResponse> GetRankHistoryAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapeRankHistoryAsync(name), "GetRankHistoryAsync");

		/// <summary>
		/// Gets the player's rank history information. This reads from the query string instead of a direct parameter.
		/// </summary>
		/// <returns>The API response.</returns>
		[HttpGet("rankhistory")]
		public async Task<string> GetRankHistoryAsync()
		{
			var name = GetNameFromQuery(HttpContext.Request.Query);
			if (string.IsNullOrEmpty(name))
				return ProcessJsonSerialization(
					HttpContext.Request.Query,
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound)
				);

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapeRankHistoryAsync(name),
				"GetRankHistoryAsync+"
			);

			return resp is RankHistoryData r
				? ProcessJsonSerialization(HttpContext.Request.Query, r)
				: ProcessJsonSerialization(HttpContext.Request.Query, resp);
		}

		/// <summary>
		/// Gets the player's guild history information.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The API response.</returns>
		[HttpGet("guildhistory/{name}")]
		public async Task<RealmEyeResponse> GetGuildHistoryAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapeGuildHistoryAsync(name), "GetGuildHistoryAsync");

		/// <summary>
		/// Gets the player's guild history information. This reads from the query string instead of a direct parameter.
		/// </summary>
		/// <returns>The API response.</returns>
		[HttpGet("guildhistory")]
		public async Task<string> GetGuildHistoryAsync()
		{
			var name = GetNameFromQuery(HttpContext.Request.Query);
			if (string.IsNullOrEmpty(name))
				return ProcessJsonSerialization(
					HttpContext.Request.Query,
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound)
				);

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapeGuildHistoryAsync(name),
				"GetGuildHistoryAsync+"
			);

			return resp is GuildHistoryData r
				? ProcessJsonSerialization(HttpContext.Request.Query, r)
				: ProcessJsonSerialization(HttpContext.Request.Query, resp);
		}


		/// <summary>
		/// Gets the player's exaltation information.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The API response.</returns>
		[HttpGet("exaltations/{name}")]
		public async Task<RealmEyeResponse> GetExaltationsAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapeExaltationsAsync(name), "GetExaltationsAsync");

		/// <summary>
		/// Gets the player's guild history information. This reads from the query string instead of a direct parameter.
		/// </summary>
		/// <returns>The API response.</returns>
		[HttpGet("exaltations")]
		public async Task<string> GetExaltationsAsync()
		{
			var name = GetNameFromQuery(HttpContext.Request.Query);
			if (string.IsNullOrEmpty(name))
				return ProcessJsonSerialization(
					HttpContext.Request.Query,
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound)
				);

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapeExaltationsAsync(name),
				"GetExaltationsAsync+"
			);

			return resp is ExaltationData r
				? ProcessJsonSerialization(HttpContext.Request.Query, r)
				: ProcessJsonSerialization(HttpContext.Request.Query, resp);
		}

		/// <summary>
		/// Executes the scraping code and returns the response of the scrape.
		/// </summary>
		/// <typeparam name="T">The response type. This must be derived from the RealmEyeResponse class.</typeparam>
		/// <param name="task">The task to perform.</param>
		/// <param name="methodName">The name of the method.</param>
		/// <returns>The response.</returns>
		private async Task<RealmEyePlayerResponse> GetRealmSharperResponse<T>(Task<T> task, string methodName)
			where T : notnull, RealmEyePlayerResponse
		{
			var sw = new Stopwatch();
			T data = null;
			try
			{
				sw.Start();

				data = await task;
				sw.Stop();

				_logger.Log(LogLevel.Information,
					$"[{methodName}] Scraped Data for {data.Name} in {sw.Elapsed.Milliseconds} MS.");

				return data.ProfileIsPrivate
					? RealmEyePlayerResponse.GenerateGenericResponse(data)
					: data;
			}
			catch (Exception e)
			{
				sw.Stop();
				_logger.Log(LogLevel.Error, e,
					$"[{methodName}] Error Occurred When Getting Profile Data. Name: {data?.Name ?? "N/A"}");

				return RealmEyePlayerResponse.GenerateGenericResponse();
			}
		}

		/// <summary>
		/// Gets the name from the query collection.
		/// </summary>
		/// <param name="collection">The query collection.</param>
		/// <returns>The name, if any. An empty string otherwise.</returns>
		private static string GetNameFromQuery(IQueryCollection collection)
			=> collection.TryGetValue("name", out var name) ? name : string.Empty;

		/// <summary>
		/// Serializes the object into a string. In particular, this will check to see if we need to prettify the
		/// output string.
		/// </summary>
		/// <param name="queryCollection">The query collection.</param>
		/// <param name="resp">The response object.</param>
		/// <returns>The resultant string.</returns>
		private string ProcessJsonSerialization<T>(IQueryCollection queryCollection, T resp)
			where T : notnull, RealmEyePlayerResponse
		{
			var res = queryCollection.ContainsKey("prettify")
				? JsonSerializer.Serialize(resp, _prettifySerializationOption)
				: JsonSerializer.Serialize(resp, _defaultSerializationOption);
			// This is needed since the serializer will encode some characters.
			return Regex.Unescape(res);
		}
	}
}