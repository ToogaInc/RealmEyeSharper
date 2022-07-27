using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RealmAspNet.RealmEye;
using RealmAspNet.RealmEye.Definitions;
using RealmAspNet.RealmEye.Definitions.Guild;
using RealmAspNet.RealmEye.Definitions.Player;
using static RealmAspNet.Controllers.Helpers;

namespace RealmAspNet.Controllers
{
	[Route("api/realmeye/guild")]
	[ApiController]
	public class RealmEyeGuildController : ControllerBase
	{
		private readonly ILogger<RealmEyeGuildController> _logger;
		private readonly JsonSerializerOptions _defaultSerializationOption;
		private readonly JsonSerializerOptions _prettifySerializationOption;

		/// <summary>
		/// Creates a new controller for this API.
		/// </summary>
		/// <param name="logger">The logging object.</param>
		public RealmEyeGuildController(ILogger<RealmEyeGuildController> logger)
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
				GuildScraper.ScrapeGuildProfileAsync(name), "ScrapeGuildProfileAsync+",
				name
			);

			if (resp is GuildData r)
			{
				return Ok(ProcessJsonSerialization(HttpContext.Request.Query, r, _prettifySerializationOption,
					_defaultSerializationOption));
			}

			return GetActionResult(resp, ProcessJsonSerialization(HttpContext.Request.Query, resp,
				_prettifySerializationOption,
				_defaultSerializationOption), this);
		}


		/// <summary>
		/// Executes the scraping code and returns the response of the scrape.
		/// </summary>
		/// <typeparam name="T">The response type. This must be derived from the RealmEyeResponse class.</typeparam>
		/// <param name="task">The task to perform.</param>
		/// <param name="method">The name of the method.</param>
		/// <param name="name">The person's name.</param>
		/// <returns>The response.</returns>
		private async Task<RealmEyeGuildResponse> GetRealmSharperResponse<T>(Task<T> task, string method, string name)
			where T : notnull, RealmEyeGuildResponse
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
				return data;
			}
			catch (Exception e)
			{
				sw.Stop();
				_logger.Log(LogLevel.Error, e,
					$"[{method}] Error Occurred When Getting Guild Data. Name: {name}");

				return RealmEyeGuildResponse.GenerateGenericResponse(name);
			}
		}
	}
}