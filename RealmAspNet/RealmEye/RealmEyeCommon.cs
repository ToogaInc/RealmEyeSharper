#nullable enable
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PlainHttp;
using RealmAspNet.RealmEye.Definitions.Player;
using RealmAspNet.RealmEye.Proxy;
using static RealmAspNet.RealmEye.Constants;

namespace RealmAspNet.RealmEye
{
	/// <summary>
	///     A set of common methods used in various RealmEye scraping methods.
	/// </summary>
	public static class RealmEyeCommon
	{
		/// <summary>
		///     A static constructor to set the factory to the custom one we defined.
		/// </summary>
		static RealmEyeCommon()
		{
			if (!UseProxy) return;
			HttpRequest.HttpClientFactory = new ProxyHttpClientFactory();
		}

		/// <summary>
		///     Gets the HtmlDocument from the corresponding URL.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <returns>The HtmlDocument object.</returns>
		public static async Task<HtmlDocument?> GetDocument(string url)
		{
			if (UseProxy)
			{
				// Stopwatch sw = Stopwatch.StartNew();
				var limit = 4;
				while (limit-- > 0)
				{
					var attempts = 0;
					var proxy = await Constants.ProxyManager.GetNextProxy();
					// Console.WriteLine("- Proxy GET Time: " + sw.Elapsed.TotalMilliseconds + "ms");
					// sw.Restart();
					while (attempts < 2)
					{
						IHttpRequest client = new HttpRequest(url)
						{
							Proxy = proxy,
							Headers = new Dictionary<string, string>
							{
								{ "User-Agent", UserAgents[Rand.Next(UserAgents.Count)] }
							},
							Method = HttpMethod.Get
						};

						// Console.WriteLine("-- HTTP Setup Time: " + sw.Elapsed.TotalMilliseconds + "ms");
						// sw.Restart();

						IHttpResponse page;
						try
						{
							page = await client.SendAsync();
							// Console.WriteLine("--- HTTP GET Time: " + sw.Elapsed.TotalMilliseconds + "ms");
							// sw.Restart();
						}
						catch (Exception e)
						{
							await Console.Error.WriteLineAsync($"[PlayerScraper] Exception: {e.Message}");
							continue;
						}

						if (!page.Message.IsSuccessStatusCode)
						{
							attempts++;
							continue;
						}

						Constants.ProxyManager.AddProxy(proxy);
						var doc = new HtmlDocument();
						doc.LoadHtml(page.Body);
						// Console.WriteLine("---- Finish Time: " + sw.Elapsed.TotalMilliseconds + "ms");
						// sw.Restart();
						return doc;
					}

					await Constants.ProxyManager.RemoveProxy(proxy);
					(HttpRequest.HttpClientFactory as ProxyHttpClientFactory)?.DeleteProxiedClient(proxy);
				}

				return null;
			}

			try
			{
				using var page = await BaseClient.GetAsync(url);
				var doc = new HtmlDocument();
				doc.LoadHtml(await page.Content.ReadAsStringAsync());
				return doc;
			}
			catch (Exception e)
			{
				await Console.Error.WriteLineAsync(e.ToString());
				return null;
			}
		}


		/// <summary>
		///     Gets the character cosmetics information.
		/// </summary>
		/// <param name="characterDisplay">
		///     The node containing the character cosmetics information (like what
		///     dyes/clothing the character is wearing).
		/// </param>
		/// <returns>The character display information.</returns>
		public static CharacterSkinInfo GetCharacterDisplayInfo(HtmlNode? characterDisplay)
		{
			var characterDisplayInfo = new CharacterSkinInfo
			{
				AccessoryDyeId = -1,
				AccessoryDyeName = string.Empty,
				ClothingDyeId = -1,
				ClothingDyeName = string.Empty
			};

			if (characterDisplay is null)
				return characterDisplayInfo;

			// The big one
			var accessoryDye = characterDisplay.FirstChild
				.Attributes["data-accessory-dye-id"]
				.Value;
			var isAccessoryDyeIdParsed = int.TryParse(accessoryDye, out var parsedAccessoryDye);

			characterDisplayInfo.AccessoryDyeId = isAccessoryDyeIdParsed ? parsedAccessoryDye : -1;
			characterDisplayInfo.AccessoryDyeName = isAccessoryDyeIdParsed
				? IdToItem.TryGetValue(parsedAccessoryDye, out var resAccObj)
					? resAccObj.Name
					: string.Empty
				: string.Empty;

			// The small one
			var clothingDye = characterDisplay.FirstChild
				.Attributes["data-clothing-dye-id"]
				.Value;
			var isClothingDyeIdParsed = int.TryParse(clothingDye, out var parsedClothingDye);

			characterDisplayInfo.ClothingDyeId = isClothingDyeIdParsed ? parsedClothingDye : -1;
			characterDisplayInfo.ClothingDyeName = isClothingDyeIdParsed
				? IdToItem.TryGetValue(parsedClothingDye, out var resCloObj)
					? resCloObj.Name
					: string.Empty
				: string.Empty;

			// Skin id
			var skinId = characterDisplay.FirstChild
				.Attributes["data-skin"]
				.Value;
			var isSkinIdParsed = int.TryParse(skinId, out var parsedSkinDye);
			characterDisplayInfo.SkinId = isSkinIdParsed
				? parsedSkinDye
				: -1;

			return characterDisplayInfo;
		}

		/// <summary>
		///     Gets the character's equipment.
		/// </summary>
		/// <param name="equips">The node containing the equipment information for the character.</param>
		/// <returns>The list of equipment the character is wearing.</returns>
		public static List<GearInfo> GetEquipment(HtmlNodeCollection? equips)
		{
			var gearInfo = new List<GearInfo>();
			if (equips is null) return gearInfo;

			for (var i = 0; i < 4; i++)
			{
				// equips[i] -> everything inside <span class="item-wrapper">
				var itemContainer = equips[i].ChildNodes[0];
				if (itemContainer.ChildNodes.Count == 0)
				{
					gearInfo.Add(new GearInfo
					{
						Tier = string.Empty,
						Id = -1,
						Name = "Empty Slot"
					});
					continue;
				}

				var itemName = WebUtility.HtmlDecode(itemContainer.ChildNodes[0].Attributes["title"].Value);
				var splitName = itemName.Split(" ");
				var parsedName = string.Join(" ", splitName[..^1]);
				gearInfo.Add(new GearInfo
				{
					Tier = splitName[^1],
					Name = parsedName,
					Id = NameToItem.TryGetValue(parsedName, out var val)
						? val.Id
						: -1
				});
			}

			return gearInfo;
		}
	}
}