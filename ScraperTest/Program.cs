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

				var args = input.Split(" ");
				if (args.Length == 0)
					continue;

				if (args[0] == "r")
				{
					var d = await RealmSharper.RealmEye.PlayerScraper.ScrapePlayerProfileAsync(args[1]);
					Console.WriteLine(d.Name);
				}
			}
		}
	}
}
