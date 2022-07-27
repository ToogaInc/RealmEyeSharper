using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealmAspNet.RealmEye.Definitions;

namespace RealmAspNet.Controllers
{
	public static class Helpers
	{
		/// <summary>
		///     Gets the name from the query collection.
		/// </summary>
		/// <param name="collection">The query collection.</param>
		/// <returns>The name, if any. An empty string otherwise.</returns>
		public static string GetNameFromQuery(IQueryCollection collection)
		{
			return collection.TryGetValue("name", out var name) ? name : string.Empty;
		}


		/// <summary>
		///     Serializes the object into a string. In particular, this will check to see if we need to prettify the
		///     output string.
		/// </summary>
		/// <param name="queryCollection">The query collection.</param>
		/// <param name="resp">The response object.</param>
		/// <param name="prettySerializer">The JSON serializer for the pretty case.</param>
		/// <param name="defaultSerializer">The JSON serializer for the general case.</param>
		/// <returns>The resultant string.</returns>
		public static string ProcessJsonSerialization<T>(
			IQueryCollection queryCollection,
			T resp,
			JsonSerializerOptions prettySerializer,
			JsonSerializerOptions defaultSerializer
		)
			where T : notnull
		{
			var res = queryCollection.ContainsKey("prettify")
				? JsonSerializer.Serialize(resp, prettySerializer)
				: JsonSerializer.Serialize(resp, defaultSerializer);
			// This is needed since the serializer will encode some characters.
			return Regex.Unescape(res);
		}


		/// <summary>
		///     Gets the appropriate return response from a failed RealmEye response.
		/// </summary>
		/// <param name="response">The original response.</param>
		/// <param name="returnVal">The value to return.</param>
		/// <param name="controllerBase">The controller.</param>
		/// <returns>The appropriate IActionResult.</returns>
		public static IActionResult GetActionResult(RealmEyeResponse response, string returnVal,
			ControllerBase controllerBase)
		{
			return response.ResultCode switch
			{
				ResultCode.NotFound => controllerBase.NotFound(returnVal),
				ResultCode.ServiceUnavailable => controllerBase.StatusCode(503, returnVal),
				ResultCode.InternalServerError => controllerBase.StatusCode(500, returnVal),
				_ => controllerBase.Ok(returnVal)
			};
		}
	}
}