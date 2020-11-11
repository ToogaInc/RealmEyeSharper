using System;
using System.Threading.Tasks;
using RealmSharper.RaidUtil;

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


				var arr = await WhoParser.ParseWhoScreenshot(new WhoInput
					{ Url = input });
				Console.WriteLine(string.Join(", ", arr));
				Console.WriteLine($"{arr.Length} People Found!");
				Console.WriteLine("===================");
			}
		}
	}
}
