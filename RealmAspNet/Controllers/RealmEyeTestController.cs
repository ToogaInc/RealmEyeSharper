using System.Collections.Generic;
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

		[HttpGet("idDict")]
		public IDictionary<int, ItemData> GetIdToObjDefinitionsAsync()
		{
			_logger.Log(LogLevel.Information, $"[GetIdToObj] Requested {Constants.NameToItem.Count} Entries");
			return Constants.IdToItem;
		}
		
		[HttpGet("nameDict")]
		public IDictionary<string, ItemData> GetNameToObjDefinitionsAsync()
		{
			_logger.Log(LogLevel.Information, $"[GetNameToObj] Requested {Constants.NameToItem.Count} Entries");
			return Constants.NameToItem;
		}
	}
}