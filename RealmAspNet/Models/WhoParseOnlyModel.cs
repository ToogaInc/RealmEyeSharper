using System.Text.Json.Serialization;

namespace RealmAspNet.Models
{
	public class WhoParseOnlyModel : BaseUrlModel
	{
		[JsonPropertyName("scale")] public bool Scale { get; set; }
	}
}