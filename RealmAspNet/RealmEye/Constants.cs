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

		public static HttpClient OCRClient;

		public static IDictionary<int, ItemData> IdToItem;
		public static IDictionary<string, ItemData> NameToItem;

		public static IConfiguration Configuration;

		public static ProxyManager ProxyManager;
		
		public static void InitConstants()
		{
			BaseClient = new HttpClient(new HttpClientHandler
			{
				AllowAutoRedirect = true
			});

			ProxyClient = new HttpClient(new HttpClientHandler
			{
				AllowAutoRedirect = true
			});

			OCRClient = new HttpClient(new HttpClientHandler
			{
				AllowAutoRedirect = true
			});
			

			BaseClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
			ProxyClient.DefaultRequestHeaders.Add("Authorization", $"Token {Configuration["proxy_key"]}");
			OCRClient.DefaultRequestHeaders.Add("apikey", $"{Configuration["ocr_key"]}");


			ProxyManager = new ProxyManager(Configuration["proxy_key"]);
			var ct = ProxyManager.GetProxies().Result;
			Console.WriteLine(ct);
			


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