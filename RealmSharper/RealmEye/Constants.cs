using ScrapySharp.Network;

namespace RealmSharper.RealmEye
{
	internal static class Constants
	{
		public static readonly ScrapingBrowser Browser = new ScrapingBrowser
		{
			AllowAutoRedirect = true,
			AllowMetaRedirect = true,
			UserAgent = new FakeUserAgent("RealmSharper", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36")
		};
		
		public const string RealmEyeBaseUrl = "https://www.realmeye.com";
	}
}