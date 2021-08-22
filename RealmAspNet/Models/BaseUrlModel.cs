using System.Text.Json.Serialization;

namespace RealmAspNet.Models
{
	public class BaseUrlModel
	{
		[JsonPropertyName("url")]
		public string Url { get; set; }
	}
}