using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Timers;
using Microsoft.Extensions.Configuration;
using RealmAspNet.RealmEye.Proxy;

namespace RealmAspNet.RealmEye
{
	public static partial class Constants
	{
		public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, " +
		                                "like Gecko) Chrome/87.0.4280.88 Safari/537.36";
		public const string RealmEyeBaseUrl = "https://www.realmeye.com";

		public static HttpClient BaseClient;
		public static HttpClient OcrClient;

		public static IDictionary<int, ItemData> IdToItem;
		public static IDictionary<string, ItemData> NameToItem;

		public static IConfiguration Configuration;

		public static ProxyManager ProxyManager;

		public static Random Rand = new();

		public static bool UseProxy;
		public static bool UseOcr;
		
		public static void InitConstants()
		{
			BaseClient = new HttpClient(new HttpClientHandler
			{
				AllowAutoRedirect = true,
				// TODO temporary patch for ssl error on linux, need to fix soon.
				ServerCertificateCustomValidationCallback = (_, _, _, _) => true 
			});
			BaseClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

			var ocrKey = Configuration["ocr_key"] ?? string.Empty;
			UseOcr = ocrKey != string.Empty;
			Console.WriteLine($"[Info] {(UseOcr ? "Using OCR" : "Not Using OCR")}.");
			OcrClient = new HttpClient(new HttpClientHandler {AllowAutoRedirect = true});
			OcrClient.DefaultRequestHeaders.Add("apikey", ocrKey);

			var proxyKey = Configuration["proxy_key"] ?? string.Empty;
			UseProxy = proxyKey != string.Empty;
			Console.WriteLine($"[Info] {(UseProxy ? "Using Proxy" : "Not Using Proxy")}.");
			ProxyManager = new ProxyManager(proxyKey);
			// Get all proxies before doing anything.
			var _ = ProxyManager.GetProxies().Result;
			
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