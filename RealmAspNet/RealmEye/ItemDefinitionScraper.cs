using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static RealmAspNet.RealmEye.Constants;

namespace RealmAspNet.RealmEye
{
	public static class ItemDefinitionScraper
	{
		/// <summary>
		/// Gets the latest definitions from RealmEye. The definitions here map the item IDs to a specified item,
		/// which can be used to map items like pet skins. 
		/// </summary>
		/// <returns>A tuple containing two dictionaries. The first dictionary consists of the item ID as the key and
		/// the item object as the value. The second dictionary consists of the item name as the key and the item object
		/// as the value.</returns>
		public static async Task<(Dictionary<int, ItemData> idToObj, Dictionary<string, ItemData> nameToObj)>
			GetDefinitions()
		{
			Console.WriteLine("Called!");
			using var httpMessage = new HttpRequestMessage
			{
				RequestUri = new Uri("https://www.realmeye.com/s/y3/js/definition.js"),
				Method = HttpMethod.Get
			};

			using var httpResponse = await Client.SendAsync(httpMessage);
			var content = await httpResponse.Content.ReadAsStringAsync();
			// remove "items={" and the end "]};" 
			// then split
			var items = content[7..^3]
				.Split(":[")
				.SelectMany(x => x.Split("],").ToArray())
				.ToArray();
			var idToInfoDict = new Dictionary<int, ItemData>();
			for (var i = 0; i < items.Length; i += 2)
			{
				// item[i] = "item"
				// key = item
				var idStr = items[i].Replace("\"", string.Empty);
				var id = int.TryParse(
					idStr,
					NumberStyles.Integer | NumberStyles.AllowExponent,
					NumberFormatInfo.CurrentInfo,
					out var parsedId
				) ? parsedId : -2;

				if (id == -2)
				{
					await Console.Error.WriteLineAsync($"[Error] {idStr} is invalid ID. Skipping.");
					continue;
				}
				
				// splitVal = ["Item name	0,-1,0,0,0,0]
				var splitVal = items[i + 1].Split("\",");
				// val = Item name
				var name = splitVal[0][1..];
				var rest = splitVal[1].Split(",")
					.Where(x => int.TryParse(x, NumberStyles.Integer | NumberStyles.AllowExponent,
						NumberFormatInfo.CurrentInfo, out _))
					.Select(x => int.Parse(x, NumberStyles.Integer | NumberStyles.AllowExponent))
					.ToArray();

				var xCoord = rest.Length > 5
					? rest[2]
					: rest[0];
				var yCoord = rest.Length > 5
					? rest[3]
					: rest[1];
				idToInfoDict.Add(id, new ItemData
				{
					Id = id,
					Name = name,
					X = xCoord,
					Y = yCoord
				});
			}

			var nameToIdDict = new Dictionary<string, ItemData>();
			foreach (var (_, obj) in idToInfoDict)
			{
				if (nameToIdDict.ContainsKey(obj.Name)) continue;
				nameToIdDict.Add(obj.Name, obj);
			}

			return (idToInfoDict, nameToIdDict);
		}
	}
}