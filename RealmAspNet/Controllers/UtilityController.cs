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

		[HttpGet("online")]
		public object GetStatus()
		{
			_logger.LogInformation("Tested Online Status");
			return new { Online = true };
		}
	}
}