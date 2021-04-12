using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RealmSharper.RealmEye;
using RealmSharper.RealmEye.Definitions;

namespace TestCode
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var names = new[]
			{
				"Pihvimies", "Runekoikoi", "Aguilej", "SAMpurdy", "Saetta", "MasterSR", "SnakeyTB", "SCIBA",
				"RitterVal", "Viktusz", "Koromo,", "GWDthree", "HypeSaltZy", "Carlcity", "MMORPGprol", "Satanred",
				"Eostra,", "LaserMast", "Kawayyy", "BouzY", "Matit", "Banbi", "Echobob", "awersowaty,", "LEOMORD",
				"Gaymax", "Gauchie", "Machoflash", "Laugedreng", "Jdewy,", "Jasonking", "BreadxFace", "Skinnytwig",
				"Orthrodaxx", "Nikolav,", "Remiwacho", "FerryPits", "WnBi", "VexiSSSS", "Isaac", "Ishkuman", "Sveks,",
				"SUNGUARD", "Novoline", "FreeeeZ", "Wkdynasty", "TheJooSCHI,", "Bruderalk", "DbossX", "LionsSin",
				"Felima", "Argolich", "Bedus", "Bananabuzs,", "Krakensen", "Dodokipje", "Teteuzym", "Cheeseydud",
				"JetOptimus,", "Nagatoxop", "Plau", "Menametake", "Pinksheepl"
			};
			var responses = new List<PlayerData>();
			var allTasks = names.Select(PlayerScraper.ScrapePlayerProfileAsync).ToArray();
			var sw = Stopwatch.StartNew();
			var res = await Task.WhenAll(allTasks);
			sw.Stop();
			Console.WriteLine($"Completed {responses.Count} In {sw.Elapsed.TotalSeconds} Seconds");

			Console.ReadLine();
			foreach (var resp in res)
			{
				var sb = new StringBuilder()
					.Append($"Name: {resp.Name}")
					.Append($"\t\tStatus Code: {resp.ResultCode}")
					.Append($"\t\tPrivate Profile? {resp.ProfileIsPrivate}")
					.Append($"\t\tPrivate Section? {resp.SectionIsPrivate}");
				Console.WriteLine(sb.ToString());
			}
		}
	}
}