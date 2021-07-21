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
		[Obsolete("Use the query string method.")]
		public async Task<RealmEyeResponse> GetBasicDataAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapePlayerProfileAsync(name), "GetBasicDataAsync", name);

		/// <summary>
		/// Gets the player's basic information (the player's RealmEye "homepage"). This reads from the query string
		/// instead of a direct parameter.
		/// </summary>
		/// <returns>The API response.</returns>
		[HttpGet("basics")]
		public async Task<IActionResult> GetBasicDataAsync()
		{
			var name = GetNameFromQuery(HttpContext.Request.Query);
			if (string.IsNullOrEmpty(name))
				return BadRequest(ProcessJsonSerialization(
					HttpContext.Request.Query,
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound)
				));

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapePlayerProfileAsync(name), "GetBasicDataAsync+", 
				name
			);

			return resp is PlayerData r
				? Ok(ProcessJsonSerialization(HttpContext.Request.Query, r))
				: GetActionResult(resp, ProcessJsonSerialization(HttpContext.Request.Query, resp));
		}

		/// <summary>
		/// Gets the player's pet yard information.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The API response.</returns>
		[HttpGet("petyard/{name}")]
		[Obsolete("Use the query string method.")]
		public async Task<RealmEyeResponse> GetPetYardAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapePetYardAsync(name), "GetPetYardAsync", name);

		/// <summary>
		/// Gets the player's pet yard information. This reads from the query string instead of a direct parameter.
		/// </summary>
		/// <returns>The API response.</returns>
		[HttpGet("petyard")]
		public async Task<IActionResult> GetPetYardAsync()
		{
			var name = GetNameFromQuery(HttpContext.Request.Query);
			if (string.IsNullOrEmpty(name))
				return BadRequest(ProcessJsonSerialization(
					HttpContext.Request.Query,
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound)
				));

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapePetYardAsync(name),
				"GetPetYardAsync+", 
				name
			);

			return resp is PetYardData r
				? Ok(ProcessJsonSerialization(HttpContext.Request.Query, r))
				: GetActionResult(resp, ProcessJsonSerialization(HttpContext.Request.Query, resp));
		}

		/// <summary>
		/// Gets the player's graveyard information.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="amt">The number of entries to fetch.</param>
		/// <returns>The API response.</returns>
		[HttpGet("graveyard/{name}/{amt:int?}")]
		[Obsolete("Use the query string method.")]
		public async Task<RealmEyeResponse> ScrapeGraveyardAsync(string name, int amt = 70)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapeGraveyardAsync(name, amt), "ScrapeGraveyardAsync", name);

		/// <summary>
		/// Gets the player's graveyard information. This reads from the query string instead of a direct parameter.
		/// </summary>
		/// <returns>The API response.</returns>
		[HttpGet("graveyard")]
		public async Task<IActionResult> GetGraveyardAsync()
		{
			var name = GetNameFromQuery(HttpContext.Request.Query);
			if (string.IsNullOrEmpty(name))
				return BadRequest(ProcessJsonSerialization(
					HttpContext.Request.Query,
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound)
				));

			var amt = HttpContext.Request.Query.TryGetValue("amt", out var unparsedNum)
				? int.TryParse(unparsedNum, out var parsedNum)
					? parsedNum
					: 70
				: 70;

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapeGraveyardAsync(name, amt),
				"ScrapeGraveyardAsync+", 
				name
			);

			return resp is GraveyardData r
				? Ok(ProcessJsonSerialization(HttpContext.Request.Query, r))
				: GetActionResult(resp, ProcessJsonSerialization(HttpContext.Request.Query, resp));
		}

		/// <summary>
		/// Gets the player's graveyard summary information.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The API response.</returns>
		[HttpGet("graveyardsummary/{name}")]
		[Obsolete("Use the query string method.")]
		public async Task<RealmEyeResponse> GetGraveyardSummaryAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapeGraveyardSummaryAsync(name),
				"GetGraveyardSummaryAsync", name);

		/// <summary>
		/// Gets the player's graveyard summary information. This reads from the query string instead of a direct
		/// parameter.
		/// </summary>
		/// <returns>The API response.</returns>
		[HttpGet("graveyardsummary")]
		public async Task<IActionResult> GetGraveyardSummaryAsync()
		{
			var name = GetNameFromQuery(HttpContext.Request.Query);
			if (string.IsNullOrEmpty(name))
				return BadRequest(ProcessJsonSerialization(
					HttpContext.Request.Query,
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound)
				));

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapeGraveyardSummaryAsync(name),
				"GetGraveyardSummaryAsync+", 
				name
			);

			return resp is GraveyardSummaryData r
				? Ok(ProcessJsonSerialization(HttpContext.Request.Query, r))
				: GetActionResult(resp, ProcessJsonSerialization(HttpContext.Request.Query, resp));
		}

		/// <summary>
		/// Gets the player's name history information.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The API response.</returns>
		[HttpGet("namehistory/{name}")]
		[Obsolete("Use the query string method.")]
		public async Task<RealmEyeResponse> GetNameHistoryAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapeNameHistoryAsync(name), "GetNameHistoryAsync", name);

		/// <summary>
		/// Gets the player's name history information. This reads from the query string instead of a direct parameter.
		/// </summary>
		/// <returns>The API response.</returns>
		[HttpGet("namehistory")]
		public async Task<IActionResult> GetNameHistoryAsync()
		{
			var name = GetNameFromQuery(HttpContext.Request.Query);
			if (string.IsNullOrEmpty(name))
				return BadRequest(ProcessJsonSerialization(
					HttpContext.Request.Query,
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound)
				));

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapeNameHistoryAsync(name),
				"GetNameHistoryAsync+", 
				name
			);

			return resp is NameHistoryData r
				? Ok(ProcessJsonSerialization(HttpContext.Request.Query, r))
				: GetActionResult(resp, ProcessJsonSerialization(HttpContext.Request.Query, resp));
		}

		/// <summary>
		/// Gets the player's rank history information.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The API response.</returns>
		[HttpGet("rankhistory/{name}")]
		[Obsolete("Use the query string method.")]
		public async Task<RealmEyeResponse> GetRankHistoryAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapeRankHistoryAsync(name), "GetRankHistoryAsync", name);

		/// <summary>
		/// Gets the player's rank history information. This reads from the query string instead of a direct parameter.
		/// </summary>
		/// <returns>The API response.</returns>
		[HttpGet("rankhistory")]
		public async Task<IActionResult> GetRankHistoryAsync()
		{
			var name = GetNameFromQuery(HttpContext.Request.Query);
			if (string.IsNullOrEmpty(name))
				return BadRequest(ProcessJsonSerialization(
					HttpContext.Request.Query,
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound)
				));

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapeRankHistoryAsync(name),
				"GetRankHistoryAsync+", 
				name
			);

			return resp is RankHistoryData r
				? Ok(ProcessJsonSerialization(HttpContext.Request.Query, r))
				: GetActionResult(resp, ProcessJsonSerialization(HttpContext.Request.Query, resp));
		}

		/// <summary>
		/// Gets the player's guild history information.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The API response.</returns>
		[HttpGet("guildhistory/{name}")]
		[Obsolete("Use the query string method.")]
		public async Task<RealmEyeResponse> GetGuildHistoryAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapeGuildHistoryAsync(name), "GetGuildHistoryAsync", name);

		/// <summary>
		/// Gets the player's guild history information. This reads from the query string instead of a direct parameter.
		/// </summary>
		/// <returns>The API response.</returns>
		[HttpGet("guildhistory")]
		public async Task<IActionResult> GetGuildHistoryAsync()
		{
			var name = GetNameFromQuery(HttpContext.Request.Query);
			if (string.IsNullOrEmpty(name))
				return BadRequest(ProcessJsonSerialization(
					HttpContext.Request.Query,
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound)
				));

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapeGuildHistoryAsync(name),
				"GetGuildHistoryAsync+", 
				name
			);

			return resp is GuildHistoryData r
				? Ok(ProcessJsonSerialization(HttpContext.Request.Query, r))
				: GetActionResult(resp, ProcessJsonSerialization(HttpContext.Request.Query, resp));
		}


		/// <summary>
		/// Gets the player's exaltation information.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <returns>The API response.</returns>
		[HttpGet("exaltations/{name}")]
		[Obsolete("Use the query string method.")]
		public async Task<RealmEyeResponse> GetExaltationsAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapeExaltationsAsync(name), "GetExaltationsAsync", name);

		/// <summary>
		/// Gets the player's guild history information. This reads from the query string instead of a direct parameter.
		/// </summary>
		/// <returns>The API response.</returns>
		[HttpGet("exaltations")]
		public async Task<IActionResult> GetExaltationsAsync()
		{
			var name = GetNameFromQuery(HttpContext.Request.Query);
			if (string.IsNullOrEmpty(name))
				return BadRequest(ProcessJsonSerialization(
					HttpContext.Request.Query,
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound)
				));

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapeExaltationsAsync(name),
				"GetExaltationsAsync+", 
				name
			);

			return resp is ExaltationData r
				? Ok(ProcessJsonSerialization(HttpContext.Request.Query, r))
				: GetActionResult(resp, ProcessJsonSerialization(HttpContext.Request.Query, resp));
		}

		/// <summary>
		/// Executes the scraping code and returns the response of the scrape.
		/// </summary>
		/// <typeparam name="T">The response type. This must be derived from the RealmEyeResponse class.</typeparam>
		/// <param name="task">The task to perform.</param>
		/// <param name="method">The name of the method.</param>
		/// <param name="name">The person's name.</param>
		/// <returns>The response.</returns>
		private async Task<RealmEyePlayerResponse> GetRealmSharperResponse<T>(Task<T> task, string method, string name)
			where T : notnull, RealmEyePlayerResponse
		{
			var sw = new Stopwatch();
			try
			{
				sw.Start();

				var data = await task;
				sw.Stop();

				_logger.Log(LogLevel.Information,
					$"[{method}] Scraped Data for {data.Name} in {sw.Elapsed.Milliseconds} MS.");

				// We are doing this instead of just returning "data" because we don't want to send junk data back
				// to the end user.
				return data.ProfileIsPrivate
					? RealmEyePlayerResponse.GenerateGenericResponse(name, data)
					: data;
			}
			catch (Exception e)
			{
				sw.Stop();
				_logger.Log(LogLevel.Error, e,
					$"[{method}] Error Occurred When Getting Profile Data. Name: {name}");

				return RealmEyePlayerResponse.GenerateGenericResponse(name);
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

		/// <summary>
		/// Gets the appropriate return response from a failed RealmEye response.
		/// </summary>
		/// <param name="response">The original response.</param>
		/// <param name="returnVal">The value to return.</param>
		/// <returns>The appropriate IActionResult.</returns>
		private IActionResult GetActionResult(RealmEyeResponse response, string returnVal)
			=> response.ResultCode switch
			{
				ResultCode.NotFound => NotFound(returnVal),
				ResultCode.ServiceUnavailable => StatusCode(503, returnVal),
				ResultCode.InternalServerError => StatusCode(500, returnVal),
				_ => Ok(returnVal)
			};
	}
}