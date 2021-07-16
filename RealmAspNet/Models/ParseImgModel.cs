
using System.Text.Json.Serialization;

namespace RealmAspNet.Models
{
	public class ParseImgModel
	{
		[JsonPropertyName("url")]
		public string Url { get; set; }

		[JsonPropertyName("simpleParse")]
		public bool? SimpleParse { get; set; }
	}
}