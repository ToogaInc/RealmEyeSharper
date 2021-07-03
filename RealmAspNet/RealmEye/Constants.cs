using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Timers;

namespace RealmAspNet.RealmEye
{
	internal static class Constants
	{
		public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, " +
		                                "like Gecko) Chrome/87.0.4280.88 Safari/537.36";
		public const string RealmEyeBaseUrl = "https://www.realmeye.com";

		public static readonly HttpClient Client;
		public static IDictionary<string, ItemData> IdToItem;
		public static IDictionary<string, ItemData> NameToItem;
		
		static Constants()
		{
			using var timer = new Timer
			{
				AutoReset = true,
				Interval = TimeSpan.FromHours(1).TotalMilliseconds
			};

			timer.Elapsed += async (_, _) =>
			{
				try
				{
					(IdToItem, NameToItem) = await ItemDefinitionScraper.GetDefinitions();
				}
				catch (Exception)
				{
					// ignore it 
				}
			};
			
			timer.Start();

			Client = new HttpClient(new HttpClientHandler
			{
				AllowAutoRedirect = true
			});
			Client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

			(IdToItem, NameToItem) = ItemDefinitionScraper.GetDefinitions().Result;
		}
	}
}