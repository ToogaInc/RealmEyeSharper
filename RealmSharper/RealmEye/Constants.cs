using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Timers;
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

		public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, " +
		                                "like Gecko) Chrome/87.0.4280.88 Safari/537.36";
		public const string RealmEyeBaseUrl = "https://www.realmeye.com";

		public static readonly HttpClient Client;
		public static IDictionary<string, string> IdToItem;
		
		static Constants()
		{
			using var timer = new Timer
			{
				AutoReset = true,
				Interval = TimeSpan.FromHours(1).TotalMilliseconds
			};

			timer.Elapsed += async (sender, args) =>
			{
				try
				{
					IdToItem = await ItemDefinitionScraper.GetDefinitions();
				}
				catch (Exception)
				{
					// ignore it 
				}
			};
			
			timer.Start();
			
			Client = new HttpClient();
			Client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

			IdToItem = ItemDefinitionScraper.GetDefinitions().Result;
		}
	}
}