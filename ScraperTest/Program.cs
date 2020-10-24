using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using RealmSharper;
using RealmSharper.RaidUtil;
using RealmSharper.RealmEye;

namespace ScraperTest
{
	public class Program
	{
		public static async Task Main()
		{
			var arr = await WhoParser.ParseWhoScreenshot(new WhoInput
				{Url = "" });
			Console.WriteLine(string.Join(", ", arr));
			Console.WriteLine($"{arr.Length} People Found!");
		}
	}
}
