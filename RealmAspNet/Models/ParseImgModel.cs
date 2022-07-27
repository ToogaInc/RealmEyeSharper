using System.Text.Json.Serialization;

namespace RealmAspNet.Models
{
	public class ParseImgModel : BaseUrlModel
	{
		[JsonPropertyName("simpleParse")] public bool? SimpleParse { get; set; }
	}
}