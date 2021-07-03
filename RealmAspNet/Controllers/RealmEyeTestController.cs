using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RealmAspNet.RealmEye;

namespace RealmAspNet.Controllers
{
	[Route("api/realmeye/test")]
	[ApiController]
	public class RealmEyeTestController : Controller
	{
		private readonly ILogger<RealmEyeTestController> _logger;

		/// <summary>
		/// Creates a new controller for this API.
		/// </summary>
		/// <param name="logger">The logging object.</param>
		public RealmEyeTestController(ILogger<RealmEyeTestController> logger)
			=> _logger = logger;

		[HttpGet("idtonamedict")]
		public async Task<Dictionary<string, ItemData>> GetIdToNameDefinitionsAsync()
		{
			var sw = Stopwatch.StartNew(); 
			var (idToName, _) = await ItemDefinitionScraper.GetDefinitions();
			sw.Stop();
			_logger.Log(LogLevel.Information, $"[GetDefinitionsAsync] Requested {idToName.Count} Entries " +
			                                  $"=> {sw.ElapsedMilliseconds} MS");
			return idToName;
		}
		
		[HttpGet("nametoiddict")]
		public async Task<Dictionary<string, ItemData>> GetNameToIdDefinitionsAsync()
		{
			var sw = Stopwatch.StartNew(); 
			var (_, nameToId) = await ItemDefinitionScraper.GetDefinitions();
			sw.Stop();
			_logger.Log(LogLevel.Information, $"[GetDefinitionsAsync] Requested {nameToId.Count} Entries " +
			                                  $"=> {sw.ElapsedMilliseconds} MS");
			return nameToId;
		}
	}
}