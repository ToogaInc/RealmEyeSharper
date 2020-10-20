using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RealmEyeSharper;

namespace ScraperTest
{
	public class Program
	{
		public static async Task Main()
		{
			var data = await PlayerScraper.ScrapeExaltationsAsync("CandyShi");
			Console.WriteLine(JsonSerializer.Serialize(data));
		}
	}
}
