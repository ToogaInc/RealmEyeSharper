using System;
using System.Collections.Generic;
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
		/// <returns>A dictionary consisting of the item ID as the key and the item name as the value.</returns>
		public static async Task<Dictionary<string, string>> GetDefinitions()
		{
			using var httpMessage = new HttpRequestMessage
			{
				RequestUri = new Uri("https://www.realmeye.com/s/y3/js/definition.js"),
				Method = HttpMethod.Get
			};

			using var httpResponse = await Client.SendAsync(httpMessage);
			var content = await httpResponse.Content.ReadAsStringAsync();
			// remove "items={" and the end "}" 
			// then split
			var items = content[7..^1]
				.Split(":[")
				.SelectMany(x => x.Split("],").ToArray())
				.ToArray();
			var dict = new Dictionary<string, string>();
			for (var i = 0; i < items.Length; i += 2)
			{
				var k = items[i].Replace("\"", string.Empty);
				var v = items[i + 1].Split("\",")[0][1..];
				dict.Add(k, v);
			}
				
			return dict;
		}
	}
}