using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using RealmEyeSharper.Definitions;
using RealmEyeSharper.Definitions.Player;
using ScrapySharp.Extensions;
using ScrapySharp.Network;

// TODO: check and make sure name input is valid 

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
		public static async Task<PlayerData> ScrapePlayerProfileAsync(string name)
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
				ResultCode = ResultCode.Success,
				ProfileIsPrivate = false,
				SectionIsPrivate = false
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
		public static async Task<PetYardData> ScrapePetYardAsync(string name)
		{
			var page = await Browser
				.NavigateToPageAsync(new Uri($"{RealmEyeBaseUrl}/{PetYardSegment}/{name}"));

			if (page == null)
				return new PetYardData {ResultCode = ResultCode.ServiceUnavailable};

			if (IsPrivate(page))
				return new PetYardData {ResultCode = ResultCode.NotFound};

			var returnData = new PetYardData
			{
				ResultCode = ResultCode.Success,
				ProfileIsPrivate = false
			};

			var mainElem = page.Html.CssSelect(".col-md-12").First();
			var petsPrivateTag = mainElem.SelectSingleNode("//div[@class='col-md-12']/h3");

			if (petsPrivateTag != null && petsPrivateTag.InnerText == "Pets are hidden.")
				return new PetYardData {ProfileIsPrivate = false};

			returnData.SectionIsPrivate = false;
			returnData.Pets = new List<PetEntry>();

			if (petsPrivateTag != null && petsPrivateTag.InnerText.Contains("has no pets."))
				return returnData; 

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

		/// <summary>
		/// <para>Returns the player's graveyard. This can return up to every page in the graveyard.</para>
		/// <para>Bear in mind that, depending on the limit given, this operation may take a while.</para>
		/// </summary>
		/// <param name="name">The name of the person to look up.</param>
		/// <param name="limit">The maximum number of graveyard entries to look up.</param>
		/// <returns>The person's graveyard.</returns>
		public static async Task<GraveyardData> ScrapeGraveyardAsync(string name, int limit = -1)
		{
			if (limit < -1)
				limit = -1;

			var page = await Browser
				.NavigateToPageAsync(new Uri($"{RealmEyeBaseUrl}/{GraveyardSegment}/{name}"));

			if (page == null)
				return new GraveyardData {ResultCode = ResultCode.ServiceUnavailable};

			if (IsPrivate(page))
				return new GraveyardData {ResultCode = ResultCode.NotFound};

			var colMd = page.Html.CssSelect(".col-md-12").First();
			// this probably isnt the best way
			// to do it.
			var gyInfoPara = colMd.SelectSingleNode("//div[@class='col-md-12']/p/text()");
			var gyInfoHead = colMd.SelectSingleNode("//div[@class='col-md-12']/h3/text()");

			if (gyInfoHead != null
			    && gyInfoHead.InnerText.Contains("is hidden")
			    && gyInfoHead.InnerText.Contains("The graveyard of"))
				return new GraveyardData {ResultCode = ResultCode.Success, ProfileIsPrivate = false};

			if (gyInfoHead != null && gyInfoHead.InnerText == "No data available yet.")
				return new GraveyardData
				{
					ResultCode = ResultCode.Success,
					Graveyard = new List<GraveyardEntry>(),
					ProfileIsPrivate = false,
					SectionIsPrivate = false
				};

			var numGraveyards = 0;
			if (gyInfoPara != null && !gyInfoPara.InnerText.Contains("We haven"))
				numGraveyards = int.Parse(gyInfoPara.InnerText.Split("graves")[0]
					.Split("found")[1]
					.Replace(",", "")
					.Trim());

			var returnData = new GraveyardData
			{
				Graveyard = new List<GraveyardEntry>(),
				ResultCode = ResultCode.Success,
				ProfileIsPrivate = false,
				SectionIsPrivate = false
			};

			// no dead characters
			if (numGraveyards == 0)
				return returnData;

			var lowestPossibleAmt = Math.Floor((double) numGraveyards / 100) * 100;
			if (limit != -1 && limit <= numGraveyards)
				lowestPossibleAmt = Math.Floor((double) limit / 100) * 100;

			returnData.GraveyardCount = numGraveyards;

			// iterate over each page in the damn website :( 
			for (var index = 1; index <= lowestPossibleAmt + 1; index += 100)
			{
				if (index != 1)
					page = await Browser
						.NavigateToPageAsync(new Uri($"{RealmEyeBaseUrl}/{GraveyardSegment}/{name}/{index}"));

				var graveyardTable = page.Html
					.CssSelect(".table-responsive")
					.CssSelect(".table")
					.First()
					// <tbody><tr>
					.SelectNodes("tbody/tr");

				// td[1] => date
				// td[2] => garbage
				// td[3] => class name
				// td[4] => level
				// td[5] => base fame
				// td[6] => total fame 
				// td[7] => exp
				// td[8] => items
				// td[9] => stats
				// td[10] => died to
				foreach (var gyRow in graveyardTable)
				{
					var diedOn = gyRow.SelectSingleNode("td[1]").InnerText;
					var character = gyRow.SelectSingleNode("td[3]").InnerText;
					var level = int.Parse(gyRow.SelectSingleNode("td[4]").InnerText);
					var baseFame = int.Parse(gyRow.SelectSingleNode("td[5]").InnerText);
					var totalFame = int.Parse(gyRow.SelectSingleNode("td[6]").FirstChild.InnerText);
					var exp = long.Parse(gyRow.SelectSingleNode("td[7]").InnerText);

					var characterEquipment = new List<string>();
					var equips = gyRow
						.SelectSingleNode("td[8]")
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

					var hadBackpack = equips.Count == 5;
					var statsMaxed = int.Parse(gyRow.SelectSingleNode("td[9]")
						.FirstChild.InnerText.Split('/')[0]);
					var diedTo = gyRow.SelectSingleNode("td[10]").InnerText;

					returnData.Graveyard.Add(new GraveyardEntry
					{
						DiedOn = diedOn,
						BaseFame = baseFame,
						Character = character,
						Equipment = characterEquipment.ToArray(),
						Experience = exp,
						HadBackpack = hadBackpack,
						KilledBy = diedTo,
						Level = level,
						MaxedStats = statsMaxed,
						TotalFame = totalFame
					});
				}
			}

			if (limit != -1)
				returnData.Graveyard = returnData.Graveyard.Take(limit).ToArray();

			return returnData;
		}

		/// <summary>
		/// <para>Returns the player's graveyard summary.</para>
		/// </summary>
		/// <param name="name">The name of the person to look up.</param>
		/// <returns>The person's graveyard summary.</returns>
		public static async Task<GraveyardSummaryData> ScrapeGraveyardSummaryAsync(string name)
		{
			var page = await Browser
				.NavigateToPageAsync(new Uri($"{RealmEyeBaseUrl}/{GraveyardSummarySegment}/{name}"));

			if (page == null)
				return new GraveyardSummaryData {ResultCode = ResultCode.ServiceUnavailable};

			if (IsPrivate(page))
				return new GraveyardSummaryData {ResultCode = ResultCode.NotFound};

			// this probably isnt the best way
			// to do it.
			var colMd = page.Html.CssSelect(".col-md-12").First();
			var gyInfoHead = colMd.SelectSingleNode("//div[@class='col-md-12']/h3/text()");

			if (gyInfoHead != null
			    && gyInfoHead.InnerText.Contains("is hidden")
			    && gyInfoHead.InnerText.Contains("The graveyard of"))
				return new GraveyardSummaryData {ResultCode = ResultCode.Success, ProfileIsPrivate = false};

			var returnData = new GraveyardSummaryData
			{
				ResultCode = ResultCode.Success,
				ProfileIsPrivate = false,
				SectionIsPrivate = false,
				Properties = new List<GraveyardSummaryProperty>(),
				StatsCharacters = new List<MaxedStatsByCharacters>(),
				TechnicalProperties = new List<GraveyardTechnicalProperty>()
			};

			if (gyInfoHead != null && gyInfoHead.InnerText == "No data available yet.")
				return returnData;

			var firstSummaryTable = page.Html
				.CssSelect("#e")
				.First()
				// <tbody><tr>
				.SelectNodes("tr");

			// td[1] => random
			// td[2] => name
			// td[3] => total
			// td[4] => max
			// td[5] => avg
			// td[6] => min
			var props = firstSummaryTable.Select(row => new GraveyardSummaryProperty
			{
				Achievement = row.SelectSingleNode("td[2]").InnerText,
				Total = long.Parse(row.SelectSingleNode("td[3]").InnerText),
				Max = long.Parse(row.SelectSingleNode("td[4]").InnerText),
				Average = double.Parse(row.SelectSingleNode("td[5]").InnerText),
				Min = long.Parse(row.SelectSingleNode("td[6]").InnerText)
			}).ToList();

			var secondSummaryTable = page.Html
				.CssSelect("#f")
				.First()
				.SelectNodes("tr");

			// td[1] => name
			// td[2] => total
			// td[3] => max
			// td[4] => avg
			// td[5] => min
			var techProps = secondSummaryTable.Select(row => new GraveyardTechnicalProperty
			{
				Achievement = row.SelectSingleNode("td[1]").InnerText,
				Total = row.SelectSingleNode("td[2]").InnerText,
				Max = row.SelectSingleNode("td[3]").InnerText,
				Average = row.SelectSingleNode("td[4]").InnerText,
				Min = row.SelectSingleNode("td[5]").InnerText
			}).ToList();

			var thirdSummaryTable = page.Html
				.CssSelect("#g")
				.First()
				.SelectNodes("tbody/tr");

			// td[1] => class
			// td[2] => 0/8
			// td[3] => 1/8
			// ...
			// td[n] = (n - 2)/8
			// n <= 10
			// td[last] = td[11] = total
			var charInfo = thirdSummaryTable.Select(row => new MaxedStatsByCharacters
			{
				CharacterType = row.SelectSingleNode("td[1]").InnerText,
				Stats = new[]
				{
					row.SelectSingleNode("td[2]").InnerText == string.Empty
						? 0
						: int.Parse(row.SelectSingleNode("td[2]").InnerText),
					row.SelectSingleNode("td[3]").InnerText == string.Empty
						? 0
						: int.Parse(row.SelectSingleNode("td[3]").InnerText),
					row.SelectSingleNode("td[4]").InnerText == string.Empty
						? 0
						: int.Parse(row.SelectSingleNode("td[4]").InnerText),
					row.SelectSingleNode("td[5]").InnerText == string.Empty
						? 0
						: int.Parse(row.SelectSingleNode("td[5]").InnerText),
					row.SelectSingleNode("td[6]").InnerText == string.Empty
						? 0
						: int.Parse(row.SelectSingleNode("td[6]").InnerText),
					row.SelectSingleNode("td[7]").InnerText == string.Empty
						? 0
						: int.Parse(row.SelectSingleNode("td[7]").InnerText),
					row.SelectSingleNode("td[8]").InnerText == string.Empty
						? 0
						: int.Parse(row.SelectSingleNode("td[8]").InnerText),
					row.SelectSingleNode("td[9]").InnerText == string.Empty
						? 0
						: int.Parse(row.SelectSingleNode("td[9]").InnerText),
					row.SelectSingleNode("td[10]").InnerText == string.Empty
						? 0
						: int.Parse(row.SelectSingleNode("td[10]").InnerText),
				},
				Total = int.Parse(row.SelectSingleNode("td[11]").InnerText)
			}).ToList();

			returnData.Properties = props;
			returnData.StatsCharacters = charInfo;
			returnData.TechnicalProperties = techProps;

			return returnData;
		}

		/// <summary>
		/// <para>Returns the player's name history.</para>
		/// </summary>
		/// <param name="name">The name of the person to look up.</param>
		/// <returns>The person's name history.</returns>
		public static async Task<NameHistoryData> ScrapeNameHistoryAsync(string name)
		{
			var page = await Browser
				.NavigateToPageAsync(new Uri($"{RealmEyeBaseUrl}/{NameHistorySegment}/{name}"));

			if (page == null)
				return new NameHistoryData {ResultCode = ResultCode.ServiceUnavailable};

			if (IsPrivate(page))
				return new NameHistoryData {ResultCode = ResultCode.NotFound};

			var returnData = new NameHistoryData
			{
				ResultCode = ResultCode.Success,
				ProfileIsPrivate = false
			};

			var colMd = page.Html.CssSelect(".col-md-12").First();
			var hiddenTxtHeader = colMd.SelectSingleNode("//div[@class='col-md-12']/h3/text()");
			if (hiddenTxtHeader != null && hiddenTxtHeader.InnerText.Contains("Name history is hidden"))
				return returnData;

			returnData.SectionIsPrivate = false;
			returnData.NameHistory = new List<NameHistoryEntry>();

			var nameHistExists = colMd.SelectNodes("//div[@class='col-md-12']/p/text()");
			if (nameHistExists.Count == 2 && nameHistExists.Last().InnerText.Contains("No name changes detected."))
				return returnData;

			var nameHistoryColl = page.Html
				.CssSelect(".table-responsive")
				.CssSelect(".table")
				.First()
				// <tbody><tr>
				.SelectNodes("tbody/tr");

			// td[1] => name
			// td[2] => from
			// td[3] => to
			foreach (var nameHistoryEntry in nameHistoryColl)
			{
				returnData.NameHistory.Add(new NameHistoryEntry
				{
					Name = nameHistoryEntry.SelectSingleNode("td[1]").InnerText,
					From = nameHistoryEntry.SelectSingleNode("td[2]").InnerText,
					To = nameHistoryEntry.SelectSingleNode("td[3]").InnerText
				});
			}

			return returnData;
		}

		/// <summary>
		/// <para>Returns the player's rank history.</para>
		/// </summary>
		/// <param name="name">The name of the person to look up.</param>
		/// <returns>The person's rank history.</returns>
		public static async Task<RankHistoryData> ScrapeRankHistoryAsync(string name)
		{
			var page = await Browser
				.NavigateToPageAsync(new Uri($"{RealmEyeBaseUrl}/{RankHistorySegment}/{name}"));

			if (page == null)
				return new RankHistoryData {ResultCode = ResultCode.ServiceUnavailable};

			if (IsPrivate(page))
				return new RankHistoryData {ResultCode = ResultCode.NotFound};

			var returnData = new RankHistoryData
			{
				ProfileIsPrivate = false,
				ResultCode = ResultCode.Success
			};

			var colMd = page.Html.CssSelect(".col-md-12").First();

			var hiddenTxtHeader = colMd.SelectSingleNode("//div[@class='col-md-12']/h3/text()");
			if (hiddenTxtHeader != null && hiddenTxtHeader.InnerText.Contains("Rank history is hidden"))
				return returnData;

			returnData.SectionIsPrivate = false;
			returnData.RankHistory = new List<RankHistoryEntry>();

			var rankHistoryColl = page.Html
				.CssSelect(".table-responsive")
				.CssSelect(".table")
				.First()
				// <tbody><tr>
				.SelectNodes("tbody/tr");

			// td[1] => rank
			// td[2] => achieved
			foreach (var rankHistEntry in rankHistoryColl)
			{
				var rank = int.Parse(rankHistEntry.SelectSingleNode("td[1]").FirstChild.InnerText);
				var since = rankHistEntry.SelectSingleNode("td[2]").InnerText;
				var date = rankHistEntry.SelectSingleNode("td[2]").FirstChild.Attributes["title"].Value;
				returnData.RankHistory.Add(new RankHistoryEntry
				{
					Achieved = since,
					Date = date,
					Rank = rank
				});
			}

			return returnData;
		}

		/// <summary>
		/// <para>Returns the player's guild history.</para>
		/// </summary>
		/// <param name="name">The name of the person to look up.</param>
		/// <returns>The person's guild history.</returns>
		public static async Task<GuildHistoryData> ScrapeGuildHistoryAsync(string name)
		{
			var page = await Browser
				.NavigateToPageAsync(new Uri($"{RealmEyeBaseUrl}/{GuildHistorySegment}/{name}"));

			if (page == null)
				return new GuildHistoryData {ResultCode = ResultCode.ServiceUnavailable};

			if (IsPrivate(page))
				return new GuildHistoryData {ResultCode = ResultCode.NotFound};

			var returnData = new GuildHistoryData
			{
				ProfileIsPrivate = false,
				ResultCode = ResultCode.Success
			};

			var colMd = page.Html.CssSelect(".col-md-12").First();
			var hiddenTxtHeader = colMd.SelectSingleNode("//div[@class='col-md-12']/h3/text()");
			if (hiddenTxtHeader != null && hiddenTxtHeader.InnerText.Contains("Guild history is hidden"))
				return returnData;

			returnData.SectionIsPrivate = false;
			returnData.GuildHistory = new List<GuildHistoryEntry>();

			var guildHistExists = colMd.SelectNodes("//div[@class='col-md-12']/h3/text()");
			if (guildHistExists != null
			    && guildHistExists.Count == 2
			    && guildHistExists.Last().InnerText.Contains("No guild changes detected."))
				return returnData;

			var guildHistoryColl = page.Html
				.CssSelect(".table-responsive")
				.CssSelect(".table")
				.First()
				// <tbody><tr>
				.SelectNodes("tbody/tr");

			// td[1] => guild name
			// td[2] => rank
			// td[3] => from
			// td[4] => to
			foreach (var guildHistoryRow in guildHistoryColl)
			{
				returnData.GuildHistory.Add(new GuildHistoryEntry
				{
					GuildName = guildHistoryRow.SelectSingleNode("td[1]").FirstChild.InnerText,
					GuildRank = guildHistoryRow.SelectSingleNode("td[2]").InnerText,
					From = guildHistoryRow.SelectSingleNode("td[3]").InnerText,
					To = guildHistoryRow.SelectSingleNode("td[4]").InnerText
				});
			}

			return returnData;
		}

		/// <summary>
		/// <para>Returns the player's exaltation data.</para>
		/// </summary>
		/// <param name="name">The name of the person to look up..</param>
		/// <returns>The person's exaltations.</returns>
		public static async Task<ExaltationData> ScrapeExaltationsAsync(string name)
		{
			var page = await Browser
				.NavigateToPageAsync(new Uri($"{RealmEyeBaseUrl}/{ExaltationSegment}/{name}"));

			if (page == null)
				return new ExaltationData {ResultCode = ResultCode.ServiceUnavailable};

			if (IsPrivate(page))
				return new ExaltationData {ResultCode = ResultCode.NotFound};

			var returnData = new ExaltationData
			{
				ProfileIsPrivate = false,
				ResultCode = ResultCode.Success
			};

			var colMd = page.Html.CssSelect(".col-md-12").First();
			var hiddenTxtHeader = colMd.SelectSingleNode("//div[@class='col-md-12']/h3/text()");

			if (hiddenTxtHeader != null && hiddenTxtHeader.InnerText.Contains("Exaltations are hidden"))
				return returnData;

			returnData.SectionIsPrivate = false;
			returnData.Exaltations = new List<ExaltationEntry>();

			if (hiddenTxtHeader != null && hiddenTxtHeader.InnerText.Contains("No exaltations"))
				return returnData;

			var exaltationTable = page.Html
				.CssSelect(".table-responsive")
				.CssSelect(".table")
				.First()
				// <tbody><tr>
				.SelectNodes("tbody/tr");

			// td[2] = class
			// td[3] = exaltation amount (8 stats * 5 possible exaltation per stat = 40 total)
			// td[4] = max hp
			// td[5] = max mp
			// td[6] = att
			// td[7] = def
			// td[8] = spd
			// td[9] = dex
			// td[10] = vit
			// td[11] = wis
			foreach (var row in exaltationTable)
			{
				var className = row.SelectSingleNode("td[2]").InnerText;
				var totalExaltations = int.Parse(row.SelectSingleNode("td[3]")
					.FirstChild.InnerText);

				// individual stats
				var hpUnparsed = row.SelectSingleNode("td[4]").ChildNodes;
				var hpExaltations = hpUnparsed.Count == 0
					? 0
					: int.Parse(hpUnparsed.First().InnerText
						.Replace("+", string.Empty));

				var mpUnparsed = row.SelectSingleNode("td[5]").ChildNodes;
				var mpExaltations = mpUnparsed.Count == 0
					? 0
					: int.Parse(mpUnparsed.First().InnerText
						.Replace("+", string.Empty));

				var attUnparsed = row.SelectSingleNode("td[6]").ChildNodes;
				var attExaltations = attUnparsed.Count == 0
					? 0
					: int.Parse(attUnparsed[0].InnerText
						.Replace("+", string.Empty));

				var defUnparsed = row.SelectSingleNode("td[7]").ChildNodes;
				var defExaltations = defUnparsed.Count == 0
					? 0
					: int.Parse(defUnparsed[0].InnerText
						.Replace("+", string.Empty));

				var spdUnparsed = row.SelectSingleNode("td[8]").ChildNodes;
				var spdExaltations = spdUnparsed.Count == 0
					? 0
					: int.Parse(spdUnparsed[0].InnerText
						.Replace("+", string.Empty));

				var dexUnparsed = row.SelectSingleNode("td[9]").ChildNodes;
				var dexExaltations = dexUnparsed.Count == 0
					? 0
					: int.Parse(dexUnparsed[0].InnerText
						.Replace("+", string.Empty));

				var vitUnparsed = row.SelectSingleNode("td[10]").ChildNodes;
				var vitExaltations = vitUnparsed.Count == 0
					? 0
					: int.Parse(vitUnparsed[0].InnerText
						.Replace("+", string.Empty));

				var wisUnparsed = row.SelectSingleNode("td[11]").ChildNodes;
				var wisExaltations = wisUnparsed.Count == 0
					? 0
					: int.Parse(wisUnparsed[0].InnerText
						.Replace("+", string.Empty));

				returnData.Exaltations.Add(new ExaltationEntry
				{
					Class = className,
					ExaltationAmount = totalExaltations,
					ExaltationStats = new Stats
					{
						Attack = attExaltations,
						Dexterity = dexExaltations,
						Defense = defExaltations,
						Health = hpExaltations / 5,
						Magic = mpExaltations / 5,
						Speed = spdExaltations,
						Vitality = vitExaltations,
						Wisdom = wisExaltations
					}
				});
			}

			return returnData;
		}
	}
}