using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RealmEyeSharper.Definitions;
using RealmEyeSharper.Definitions.Player;
using ScrapySharp.Extensions;
using ScrapySharp.Network;

namespace RealmEyeSharper
{
	public static class PlayerScraper
	{
		#region RealmEye Player URLs

		internal const string RealmEyeBaseUrl = "https://www.realmeye.com";
		internal const string PlayerSegment = "player";
		internal const string ExaltationSegment = "exaltations-of";
		internal const string PetYardSegment = "pets-of";
		internal const string GraveyardSegment = "graveyard-of-player";
		internal const string GraveyardSummarySegment = "graveyard-summary-of-player";
		internal const string FameHistorySegment = "fame-history-of-player";
		internal const string RankHistorySegment = "rank-history-of-player";
		internal const string NameHistorySegment = "name-history-of-player";
		internal const string GuildHistorySegment = "guild-history-of-player";

		#endregion

		internal static ScrapingBrowser Browser = new ScrapingBrowser
		{
			AllowAutoRedirect = true,
			AllowMetaRedirect = true
		};

		/// <summary>
		/// Whether the profile is private or not.
		/// </summary>
		/// <param name="page">The WebPage object representing the RealmEye page.</param>
		/// <returns>Whether the profile is private or not.</returns>
		private static bool IsPrivate(WebPage page)
		{
			var mainElement = page.Html.CssSelect(".col-md-12");
			return mainElement.CssSelect(".player-not-found").Count() != 0;
		}

		/// <summary>
		/// <para>Returns the player's introductory RealmEye page.</para>
		/// <para>This includes information like the person's stats, description, and characters.</para>
		/// </summary>
		/// <param name="name">The name of the person to look up.</param>
		/// <returns>The player data.</returns>
		public static async Task<PlayerData> ScrapePlayerProfile(string name)
		{
			var page = await Browser
				.NavigateToPageAsync(new Uri($"{RealmEyeBaseUrl}/{PlayerSegment}/{name}"));

			if (page == null)
				return new PlayerData {ResultCode = ResultCode.ServiceUnavailable};

			if (IsPrivate(page))
				return new PlayerData {ResultCode = ResultCode.NotFound};

			// profile public
			// scrap time
			var returnData = new PlayerData
			{
				Name = page.Html.CssSelect(".entity-name").First().InnerText,
				ResultCode = ResultCode.Success
			};

			var summaryTable = page.Html.CssSelect(".summary").First();
			var numChars = -1;
			// <tr> 
			foreach (var row in summaryTable.SelectNodes("tr"))
			{
				// td[1] is the first column (property name) -- e.g. "Skins"
				// td[2] is the second column (property value) -- e.g. "58 (6349th)
				foreach (var col in row.SelectNodes("td[1]"))
				{
					switch (col.InnerText)
					{
						case "Characters":
							returnData.CharacterCount = int.Parse(col.NextSibling.InnerText);
							numChars = returnData.CharacterCount;
							break;
						case "Skins":
							returnData.Skins = int.Parse(col.NextSibling.InnerText.Split('(')[0]);
							break;
						case "Fame":
							returnData.Fame = int.Parse(col.NextSibling.InnerText.Split('(')[0]);
							break;
						case "Exp":
							returnData.Exp = int.Parse(col.NextSibling.InnerText.Split('(')[0]);
							break;
						case "Rank":
							returnData.Rank = int.Parse(col.NextSibling.InnerText);
							break;
						case "Account fame":
							returnData.AccountFame = int.Parse(col.NextSibling.InnerText.Split('(')[0]);
							break;
						case "Guild":
							returnData.Guild = col.NextSibling.InnerText;
							break;
						case "Guild Rank":
							returnData.GuildRank = col.NextSibling.InnerText;
							break;
						case "First seen":
							returnData.FirstSeen = col.NextSibling.InnerText;
							break;
						case "Last seen":
							returnData.LastSeen = col.NextSibling.InnerText;
							break;
						case "Created":
							returnData.Created = col.NextSibling.InnerText;
							break;
					}
				}
			}

			var mainElement = page.Html.CssSelect(".col-md-12");
			// #d is id = "d" (in html)
			var descriptionTable = mainElement.CssSelect("#d").First();
			var noDesc = descriptionTable.CssSelect(".help");
			if (noDesc.Any())
				returnData.Description = new string[0];
			else
			{
				returnData.Description = new string[3];
				returnData.Description[0] = WebUtility.HtmlDecode(descriptionTable.FirstChild.InnerText);
				returnData.Description[1] = WebUtility.HtmlDecode(descriptionTable.FirstChild.NextSibling.InnerText);
				returnData.Description[2] = WebUtility
					.HtmlDecode(descriptionTable.FirstChild.NextSibling.NextSibling.InnerText);

				returnData.Description = returnData.Description
					.Where(x => x != "")
					.ToArray();
			}

			returnData.Characters = new List<CharacterEntry>();
			// character parsing
			if (numChars == -1)
			{
				returnData.CharacterCount = -1;
				return returnData;
			}

			var charTable = page.Html
				.CssSelect("#e")
				.First()
				// <tbody><tr>
				.SelectNodes("tbody/tr");

			// td[3] => character type
			// td[4] => level
			// td[5] => cqc
			// td[6] => fame
			// td[7] => exp
			// td[8] => place
			// td[9] => equipment
			// td[10] => stats
			foreach (var characterRow in charTable)
			{
				var petIdRaw = characterRow.SelectSingleNode("td[1]").FirstChild;
				var petId = petIdRaw == null
					? -1
					: int.Parse(petIdRaw.Attributes["data-item"].Value);

				var characterType = characterRow.SelectSingleNode("td[3]").InnerText;
				var level = int.Parse(characterRow.SelectSingleNode("td[4]").InnerText);
				var cqc = int.Parse(characterRow.SelectSingleNode("td[5]").InnerText.Split('/')[0]);
				var fame = int.Parse(characterRow.SelectSingleNode("td[6]").InnerText);
				var exp = long.Parse(characterRow.SelectSingleNode("td[7]").InnerText);
				var place = int.Parse(characterRow.SelectSingleNode("td[8]").InnerText);

				var characterEquipment = new List<string>();
				var equips = characterRow
					.SelectSingleNode("td[9]")
					// <span class="item-wrapper">...
					.ChildNodes;
				for (var i = 0; i < 4; i++)
				{
					// equips[i] -> everything inside <span class="item-wrapper">
					var itemContainer = equips[i].ChildNodes[0];
					characterEquipment.Add(itemContainer.ChildNodes.Count == 0
						? "Empty Slot"
						: WebUtility.HtmlDecode(itemContainer.ChildNodes[0].Attributes["title"].Value));
				}

				// <span class = "player-stats" ...
				var stats = characterRow
					.SelectSingleNode("td[10]");
				var maxedStats = int.Parse(stats.InnerText.Split('/')[0]);
				var dataStats = stats.FirstChild
					.Attributes["data-stats"]
					.Value[1..^1]
					.Split(',')
					.Select(int.Parse)
					.ToArray();
				var bonusStats = stats.FirstChild
					.Attributes["data-bonuses"]
					.Value[1..^1]
					.Split(',')
					.Select(int.Parse)
					.ToArray();

				returnData.Characters.Add(new CharacterEntry
				{
					ActivePetId = petId,
					CharacterType = characterType,
					ClassQuestsCompleted = cqc,
					EquipmentData = characterEquipment.ToArray(),
					Experience = exp,
					Fame = fame,
					HasBackpack = equips.Count == 5,
					Level = level,
					Place = place,
					StatsMaxed = maxedStats,
					Stats = new Stats
					{
						Health = dataStats[0] - bonusStats[0],
						Magic = dataStats[1] - bonusStats[1],
						Attack = dataStats[2] - bonusStats[2],
						Defense = dataStats[3] - bonusStats[3],
						Speed = dataStats[4] - bonusStats[4],
						Vitality = dataStats[5] - bonusStats[5],
						Wisdom = dataStats[6] - bonusStats[6],
						Dexterity = dataStats[7] - bonusStats[7]
					}
				});
			}

			return returnData;
		}

		/// <summary>
		/// <para>Returns the player's pet yard information.</para>
		/// <para>This will include information for each pet, including its abilities and rankings.</para>
		/// </summary>
		/// <param name="name">The name of the person to look up.</param>
		/// <returns>The person's pet yard data.</returns>
		public static async Task<PetYardData> ScrapePetYard(string name)
		{
			var page = await Browser
				.NavigateToPageAsync(new Uri($"{RealmEyeBaseUrl}/{PetYardSegment}/{name}"));

			if (page == null)
				return new PetYardData {ResultCode = ResultCode.ServiceUnavailable, IsPublic = false};

			if (IsPrivate(page))
				return new PetYardData {ResultCode = ResultCode.NotFound, IsPublic = false};

			var mainElem = page.Html.CssSelect(".col-md-12").First();
			var petsPrivateTag = mainElem.SelectSingleNode("//div[@class='col-md-12']/h3");
			if (petsPrivateTag != null)
			{
				if (petsPrivateTag.InnerText == "Pets are hidden.")
					return new PetYardData
					{
						ResultCode = ResultCode.Success, 
						IsPublic = false,
						Pets = new List<PetEntry>()
					};

				if (petsPrivateTag.InnerText.Contains("has no pets."))
					return new PetYardData
					{
						ResultCode = ResultCode.Success,
						IsPublic = true,
						Pets = new List<PetEntry>()
					};
			}

			var returnData = new PetYardData
			{
				ResultCode = ResultCode.Success,
				IsPublic = true,
				Pets = new List<PetEntry>()
			};

			var petTable = page.Html
				.CssSelect("#e")
				.First()
				// <tbody><tr>
				.SelectNodes("tbody/tr");

			// td[1] => span class, data-item
			// td[2] => name of pet
			// td[3] => rarity 
			// td[4] => family
			// depends on locked/unlocked
			// td[5] => first ability 
			// td[6] => first ability stats
			// td[7] => second ability
			// td[7] => second ability stats
			// td[8] => third ability 
			foreach (var petRow in petTable)
			{
				var maxLevel = int.Parse(petRow.SelectNodes("td")
					.Last()
					.InnerText);
				var petId = int.Parse(petRow.SelectSingleNode("td[1]")
					.FirstChild
					.Attributes["data-item"]
					.Value);
				var petName = petRow.SelectSingleNode("td[2]")
					.InnerText;
				var rarity = petRow.SelectSingleNode("td[3]")
					.InnerText;
				var family = petRow.SelectSingleNode("td[4]")
					.InnerText;
				var rank = petRow.SelectSingleNode("td[5]").InnerText == string.Empty
					? -1
					: int.Parse(petRow.SelectSingleNode("td[5]")
						.InnerText[..^2]);
				// td[6] start of ability 
				// will always be unlocked
				var petAbility = new List<PetAbilityData>();
				var firstAbilityName = petRow.SelectSingleNode("td[6]")
					.FirstChild
					.InnerText;
				var firstAbilityLevel = int.Parse(petRow.SelectSingleNode("td[7]")
					.FirstChild
					.InnerText);
				petAbility.Add(new PetAbilityData
				{
					AbilityName = firstAbilityName,
					IsMaxed = firstAbilityLevel == maxLevel,
					Level = firstAbilityLevel,
					IsUnlocked = true
				});

				// get 2nd ability + 
				for (var i = 8; i < petRow.SelectNodes("td").Count; i++)
				{
					var ability = petRow.SelectSingleNode($"td[{i}]");
					if (ability.InnerText == string.Empty)
						continue;

					if (!ability.CssSelect(".pet-ability-disabled").Any())
					{
						// ability exists and is unlocked!
						var nameOfAbility = ability.FirstChild.InnerText;
						++i;
						var abilityLvl = int.Parse(petRow.SelectSingleNode($"td[{i}]")
							.FirstChild
							.InnerText);
						petAbility.Add(new PetAbilityData
						{
							AbilityName = nameOfAbility,
							IsMaxed = abilityLvl == maxLevel,
							Level = abilityLvl,
							IsUnlocked = true
						});
					}
					else
					{
						// locked ability
						var nameOfAbility = ability.FirstChild.InnerText;
						petAbility.Add(new PetAbilityData
						{
							AbilityName = nameOfAbility,
							IsMaxed = false,
							Level = -1,
							IsUnlocked = false
						});
					}
				}

				returnData.Pets.Add(new PetEntry
				{
					ActivePetId = petId,
					Family = family,
					MaxLevel = maxLevel,
					Name = petName,
					PetAbilities = petAbility,
					Place = rank,
					Rarity = rarity
				});
			}

			return returnData;
		}
	}
}