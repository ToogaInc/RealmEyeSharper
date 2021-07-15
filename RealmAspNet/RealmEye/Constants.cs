using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.Configuration;
using RealmAspNet.RealmEye.Proxy;

namespace RealmAspNet.RealmEye
{
	internal static class Constants
	{
		public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, " +
		                                "like Gecko) Chrome/87.0.4280.88 Safari/537.36";
		public const string RealmEyeBaseUrl = "https://www.realmeye.com";

		public static HttpClient BaseClient;

		public static HttpClient ProxyClient;
		public static IDictionary<int, ItemData> IdToItem;
		public static IDictionary<string, ItemData> NameToItem;

		public static IConfiguration Configuration;

		public static ProxyManager ProxyManager;
		
		public static void InitConstants()
		{
			ProxyClient = new HttpClient(new HttpClientHandler
			{
				AllowAutoRedirect = true
			});
			ProxyClient.DefaultRequestHeaders.Add("Authorization", $"Token {Configuration["proxy_key"]}");
			
			ProxyManager = new ProxyManager();
			var ct = ProxyManager.GetProxies().Result;
			Console.WriteLine(ct);
			BaseClient = new HttpClient(new HttpClientHandler
			{
				AllowAutoRedirect = true
			});

			BaseClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
			(IdToItem, NameToItem) = ItemDefinitionScraper.GetDefinitions().Result;
			
			
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
		}
	}
}