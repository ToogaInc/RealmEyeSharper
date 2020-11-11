using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RealmSharper.RealmEye;
using RealmSharper.RealmEye.Definitions;

namespace RealmAspNet.Controllers
{
	[Route("api/realmeye")]
	[ApiController]
	public class RealmEyeController : ControllerBase
	{
		private readonly ILogger<RealmEyeController> _logger;

		/// <summary>
		/// Creates a new controller for this API.
		/// </summary>
		/// <param name="logger">The logging object.</param>
		public RealmEyeController(ILogger<RealmEyeController> logger)
		{
			_logger = logger;
		}

		[HttpGet("basics/{name}")]
		public async Task<RealmEyeResponse> GetBasicDataAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapePlayerProfileAsync(name), "GetBasicDataAsync");


		[HttpGet("petyard/{name}")]
		public async Task<RealmEyeResponse> GetPetYardAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapePetYardAsync(name), "GetPetYardAsync");

		[HttpGet("graveyard/{name}/{amt?}")]
		public async Task<RealmEyeResponse> ScrapeGraveyardAsync(string name, int amt = 1)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapeGraveyardAsync(name, amt), "ScrapeGraveyardAsync");

		[HttpGet("graveyardsummary/{name}")]
		public async Task<RealmEyeResponse> GetGraveyardSummaryAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapeGraveyardSummaryAsync(name), "GetGraveyardSummaryAsync");

		[HttpGet("namehistory/{name}")]
		public async Task<RealmEyeResponse> GetNameHistoryAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapeNameHistoryAsync(name), "GetNameHistoryAsync");

		[HttpGet("rankhistory/{name}")]
		public async Task<RealmEyeResponse> GetRankHistoryAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapeRankHistoryAsync(name), "GetRankHistoryAsync");

		[HttpGet("guildhistory/{name}")]
		public async Task<RealmEyeResponse> GetGuildHistoryAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapeGuildHistoryAsync(name), "GetGuildHistoryAsync");

		[HttpGet("exaltations/{name}")]
		public async Task<RealmEyeResponse> GetExaltationsAsync(string name)
			=> await GetRealmSharperResponse(PlayerScraper.ScrapeExaltationsAsync(name), "GetExaltationsAsync");

		/// <summary>
		/// Gets a response from the RealmSharper library.
		/// </summary>
		/// <typeparam name="T">The response type. This must be derived from the RealmEyeResponse class.</typeparam>
		/// <param name="task">The task to perform.</param>
		/// <param name="methodName">The name of the method.</param>
		/// <returns>The response.</returns>
		private async Task<RealmEyeResponse> GetRealmSharperResponse<T>(Task<T> task, string methodName) 
			where T : notnull, RealmEyeResponse
		{
			var sw = new Stopwatch();
			T data = null;
			try
			{
				sw.Start();

				data = await task;
				sw.Stop();

				_logger.Log(LogLevel.Information, $"[{methodName}] Scraped Data for {data.Name} in {sw.Elapsed.Milliseconds} MS.");

				return data.ProfileIsPrivate
					? RealmEyeResponse.GenerateGenericResponse(data)
					: data;
			}
			catch (Exception e)
			{
				sw.Stop();
				_logger.Log(LogLevel.Error, e, $"[{methodName}] Error Occurred When Getting Profile Data. Name: {data?.Name ?? "N/A"}");

				return RealmEyeResponse.GenerateGenericResponse();
			}
		}
	}
}