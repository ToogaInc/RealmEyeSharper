using System.Collections.Generic;
using Newtonsoft.Json;

namespace RealmAspNet.Definitions
{
	public class OcrSpaceResponse
	{
		[JsonProperty("ParsedResults")] public List<ParsedResult> ParsedResults { get; set; }
		[JsonProperty("OCRExitCode")] public long OcrExitCode { get; set; }

		[JsonProperty("IsErroredOnProcessing")]
		public bool IsErroredOnProcessing { get; set; }

		[JsonProperty("ProcessingTimeInMilliseconds")]
		public long ProcessingTimeInMilliseconds { get; set; }
	}

	public class ParsedResult
	{
		[JsonProperty("TextOverlay")] public TextOverlay TextOverlay { get; set; }
		[JsonProperty("TextOrientation")] public long TextOrientation { get; set; }
		[JsonProperty("FileParseExitCode")] public long FileParseExitCode { get; set; }
		[JsonProperty("ParsedText")] public string ParsedText { get; set; }
		[JsonProperty("ErrorMessage")] public string ErrorMessage { get; set; }
		[JsonProperty("ErrorDetails")] public string ErrorDetails { get; set; }
	}

	public class TextOverlay
	{
		[JsonProperty("Lines")] public List<Line> Lines { get; set; }
		[JsonProperty("HasOverlay")] public bool HasOverlay { get; set; }
		[JsonProperty("Message")] public string Message { get; set; }
	}

	public class Line
	{
		[JsonProperty("LineText")] public string LineText { get; set; }
		[JsonProperty("Words")] public List<Word> Words { get; set; }
		[JsonProperty("MaxHeight")] public double MaxHeight { get; set; }
		[JsonProperty("MinTop")] public double MinTop { get; set; }
	}

	public class Word
	{
		[JsonProperty("WordText")] public string WordText { get; set; }
		[JsonProperty("Left")] public double Left { get; set; }
		[JsonProperty("Top")] public double Top { get; set; }
		[JsonProperty("Height")] public double Height { get; set; }
		[JsonProperty("Width")] public double Width { get; set; }
	}
}