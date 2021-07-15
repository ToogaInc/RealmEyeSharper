using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RealmAspNet.Controllers
{
	[Route("api")]
	[ApiController]
	public class UtilityController : ControllerBase
	{
		private readonly ILogger<UtilityController> _logger;

		/// <summary>
		/// Creates a new controller for this API.
		/// </summary>
		/// <param name="logger">The logging object.</param>
		public UtilityController(ILogger<UtilityController> logger)
		{
			_logger = logger;
		}

		/// <summary>
		/// Checks if the API is online.
		/// </summary>
		/// <returns>Always returns an object with the Online member being true.</returns>
		[HttpGet("online")]
		public IActionResult GetStatus()
		{
			_logger.LogInformation("Tested Online Status");
			return Ok(new { Online = true });
		}
	}
}