using System;
using System.Threading.Tasks;

namespace ScraperTest
{
	public class Program
	{
		public static async Task Main()
		{
			while (true)
			{
				var input = Console.ReadLine();
				if (string.IsNullOrEmpty(input))
					continue;
				
				if (input.ToLower() == "exit")
					break;


				await RealmSharper.RealmEye.PlayerScraper.ScrapePlayerProfileAsync(input);
			}
		}
	}
}
