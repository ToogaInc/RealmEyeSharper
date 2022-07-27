using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RealmAspNet.RealmEye;
using RealmAspNet.RealmEye.Definitions;
using RealmAspNet.RealmEye.Definitions.Player;
using static RealmAspNet.Controllers.Helpers;

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
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				WriteIndented = true,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};
			_defaultSerializationOption = new JsonSerializerOptions
			{
				DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};
		}

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
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound),
					_prettifySerializationOption,
					_defaultSerializationOption
				));

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapePlayerProfileAsync(name), "GetBasicDataAsync+",
				name
			);

			if (resp is PlayerData r)
			{
				return Ok(ProcessJsonSerialization(HttpContext.Request.Query, r,
					_prettifySerializationOption,
					_defaultSerializationOption));
			}

			return GetActionResult(resp, ProcessJsonSerialization(
				HttpContext.Request.Query,
				resp,
				_prettifySerializationOption,
				_defaultSerializationOption
			), this);
		}

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
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound),
					_prettifySerializationOption,
					_defaultSerializationOption
				));

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapePetYardAsync(name),
				"GetPetYardAsync+",
				name
			);

			if (resp is PetYardData r)
			{
				return Ok(ProcessJsonSerialization(HttpContext.Request.Query, r,
					_prettifySerializationOption,
					_defaultSerializationOption));
			}

			return GetActionResult(resp, ProcessJsonSerialization(
				HttpContext.Request.Query,
				resp,
				_prettifySerializationOption,
				_defaultSerializationOption
			), this);
		}

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
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound),
					_prettifySerializationOption,
					_defaultSerializationOption
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

			if (resp is GraveyardData r)
			{
				return Ok(ProcessJsonSerialization(HttpContext.Request.Query, r,
					_prettifySerializationOption,
					_defaultSerializationOption));
			}

			return GetActionResult(resp, ProcessJsonSerialization(
				HttpContext.Request.Query,
				resp,
				_prettifySerializationOption,
				_defaultSerializationOption
			), this);
		}

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
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound),
					_prettifySerializationOption,
					_defaultSerializationOption
				));

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapeGraveyardSummaryAsync(name),
				"GetGraveyardSummaryAsync+",
				name
			);

			if (resp is GraveyardSummaryData r)
			{
				return Ok(ProcessJsonSerialization(HttpContext.Request.Query, r,
					_prettifySerializationOption,
					_defaultSerializationOption));
			}

			return GetActionResult(resp, ProcessJsonSerialization(
				HttpContext.Request.Query,
				resp,
				_prettifySerializationOption,
				_defaultSerializationOption
			), this);
		}

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
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound),
					_prettifySerializationOption,
					_defaultSerializationOption
				));

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapeNameHistoryAsync(name),
				"GetNameHistoryAsync+",
				name
			);

			if (resp is NameHistoryData r)
			{
				return Ok(ProcessJsonSerialization(HttpContext.Request.Query, r,
					_prettifySerializationOption,
					_defaultSerializationOption));
			}

			return GetActionResult(resp, ProcessJsonSerialization(
				HttpContext.Request.Query,
				resp,
				_prettifySerializationOption,
				_defaultSerializationOption
			), this);
		}

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
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound),
					_prettifySerializationOption,
					_defaultSerializationOption
				));

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapeRankHistoryAsync(name),
				"GetRankHistoryAsync+",
				name
			);

			if (resp is RankHistoryData r)
			{
				return Ok(ProcessJsonSerialization(HttpContext.Request.Query, r,
					_prettifySerializationOption,
					_defaultSerializationOption));
			}

			return GetActionResult(resp, ProcessJsonSerialization(
				HttpContext.Request.Query,
				resp,
				_prettifySerializationOption,
				_defaultSerializationOption
			), this);
		}

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
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound),
					_prettifySerializationOption,
					_defaultSerializationOption
				));

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapeGuildHistoryAsync(name),
				"GetGuildHistoryAsync+",
				name
			);

			if (resp is GuildHistoryData r)
			{
				return Ok(ProcessJsonSerialization(HttpContext.Request.Query, r,
					_prettifySerializationOption,
					_defaultSerializationOption));
			}

			return GetActionResult(resp, ProcessJsonSerialization(
				HttpContext.Request.Query,
				resp,
				_prettifySerializationOption,
				_defaultSerializationOption
			), this);
		}

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
					RealmEyePlayerResponse.GenerateGenericResponse(ResultCode.NotFound),
					_prettifySerializationOption,
					_defaultSerializationOption
				));

			var resp = await GetRealmSharperResponse(
				PlayerScraper.ScrapeExaltationsAsync(name),
				"GetExaltationsAsync+",
				name
			);

			if (resp is ExaltationData r)
			{
				return Ok(ProcessJsonSerialization(HttpContext.Request.Query, r,
					_prettifySerializationOption,
					_defaultSerializationOption));
			}

			return GetActionResult(resp, ProcessJsonSerialization(
				HttpContext.Request.Query,
				resp,
				_prettifySerializationOption,
				_defaultSerializationOption
			), this);
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
	}
}