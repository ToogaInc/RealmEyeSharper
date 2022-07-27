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
		///     Creates a new controller for this API.
		/// </summary>
		/// <param name="logger">The logging object.</param>
		public RealmEyeTestController(ILogger<RealmEyeTestController> logger)
		{
			_logger = logger;
		}

		/// <summary>
		///     Scrapes RealmEye's `definitions.js` for item data.
		/// </summary>
		/// <returns>An array of objects where the item ID is the key and the item information is the value.</returns>
		[HttpGet("idDict")]
		public IActionResult GetIdToObjDefinitionsAsync()
		{
			_logger.Log(LogLevel.Information, $"[GetIdToObj] Requested {Constants.NameToItem.Count} Entries");
			return Ok(Constants.IdToItem);
		}

		/// <summary>
		///     Scrapes RealmEye's `definitions.js` for item data.
		/// </summary>
		/// <returns>An array of objects where the item name is the key and the item information is the value.</returns>
		[HttpGet("nameDict")]
		public IActionResult GetNameToObjDefinitionsAsync()
		{
			_logger.Log(LogLevel.Information, $"[GetNameToObj] Requested {Constants.NameToItem.Count} Entries");
			return Ok(Constants.NameToItem);
		}
	}
}