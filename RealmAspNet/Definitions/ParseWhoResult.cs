namespace RealmAspNet.Definitions
{
	public class ParseWhoResult
	{
		public long ImageDownloadTime { get; set; }
		public long ImageProcessingTime { get; set; }
		public long OcrRecognitionTime { get; set; }
		public string[] WhoResult { get; set; }
		public string RawOcrResult { get; set; }
		public int Count { get; set; }

		public string Code { get; set; }
		public string Issues { get; set; }
	}
}