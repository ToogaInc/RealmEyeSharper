using System.Text.Json.Serialization;

namespace RealmAspNet.Models
{
	public class WhoParseOnlyModel
	{
		[JsonPropertyName("url")]
		public string Url { get; set; }

		[JsonPropertyName("scale")]
		public bool Scale { get; set; }
	}
}