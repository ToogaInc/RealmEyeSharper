using System;
using System.Text.Json;
using System.Threading.Tasks;
using RealmEyeSharper;

namespace ScraperTest
{
	public class Program
	{
		public static async Task Main()
		{
			var data = await PlayerScraper.ScrapeGraveyard("asdsadasdasd", 10);
			Console.WriteLine(JsonSerializer.Serialize(data));
		}
	}
}
