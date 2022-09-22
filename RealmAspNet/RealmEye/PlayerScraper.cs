#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using RealmAspNet.RealmEye.Definitions;
using RealmAspNet.RealmEye.Definitions.Common;
using RealmAspNet.RealmEye.Definitions.Player;
using ScrapySharp.Extensions;
using static RealmAspNet.RealmEye.Constants;
using static RealmAspNet.RealmEye.RealmEyeCommon;

namespace RealmAspNet.RealmEye
{
	public static class PlayerScraper
	{
		private static readonly Regex SquareBracketRegex = new(@"\[(.*?)\]", RegexOptions.Compiled);

		/// <summary>
		///     Whether the profile is private or not.
		/// </summary>
		/// <param name="doc">The HtmlDocument representing the RealmEye page.</param>
		/// <returns>Whether the profile is private or not.</returns>
		private static bool IsPrivate(HtmlDocument doc)
		{
			var mainElement = doc.DocumentNode.CssSelect(".col-md-12");
			return mainElement.CssSelect(".player-not-found").Any();
		}

		/// <summary>
		///     <para>Returns the player's introductory RealmEye page.</para>
		///     <para>This includes information like the person's stats, description, and characters.</para>
		/// </summary>
		/// <param name="name">The name of the person to look up.</param>
		/// <returns>The player data.</returns>
		public static async Task<PlayerData> ScrapePlayerProfileAsync(string name)
		{
			// Stopwatch sw = Stopwatch.StartNew();
			var document = await GetDocument($"{RealmEyeBaseUrl}/{PlayerSegment}/{name}");
			// Console.WriteLine("Document GET Time: " + sw.Elapsed.TotalMilliseconds + "ms");
			// sw.Restart();

			if (document is null)
				return new PlayerData { ResultCode = ResultCode.ServiceUnavailable, Name = name };

			if (IsPrivate(document))
				return new PlayerData { ResultCode = ResultCode.NotFound, Name = name };

			// profile public
			// scrap time
			var returnData = new PlayerData
			{
				Name = document.DocumentNode.CssSelect(".entity-name").First().InnerText,
				ResultCode = ResultCode.Success,
				ProfileIsPrivate = false,
				SectionIsPrivate = false
			};

			var summaryTable = document.DocumentNode.CssSelect(".summary").First();
			var numChars = -1;
			// <tr> 
			foreach (var row in summaryTable.SelectNodes("tr"))
				// td[1] is the first column (property name) -- e.g. "Skins"
				// td[2] is the second column (property value) -- e.g. "58 (6349th)
			foreach (var col in row.SelectNodes("td[1]"))
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
						returnData.Fame = long.Parse(col.NextSibling.InnerText.Split('(')[0]);
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

			var finalDesc = new List<string>();
			for (var i = 1; i <= 3; i++)
			{
				var possDesc = document.DocumentNode.SelectNodes($"//div[contains(@class, 'line{i}')]");
				if (possDesc != null && possDesc.Count != 0 && possDesc[0].InnerText.Length != 0)
					finalDesc.Add(HttpUtility.HtmlDecode(possDesc[0].InnerText));
			}

			returnData.Description = finalDesc.ToArray();

			// Console.WriteLine("Document Summary Time: " + sw.Elapsed.TotalMilliseconds + "ms");
			// sw.Restart();

			returnData.Characters = new List<CharacterEntry>();
			// character parsing
			if (numChars == -1)
			{
				returnData.CharacterCount = -1;
				return returnData;
			}

			// this is the only table with an id
			var charTable = document.DocumentNode.SelectNodes("//table[@id]/tbody/tr");

			// td[3] => character type
			// td[4] => level
			// td[5] => cqc
			// td[6] => fame
			// td[7] => exp
			// td[8] => place
			// td[9] => equipment
			// td[10] => stats
			if (charTable == null)
				return returnData;

			var tableOffset = 0;
			foreach (var characterRow in charTable)
			{
				// pet: column 1
				// Here, we need to explicitly check and make sure the pet column does exist.
				var petIdRaw = characterRow.SelectSingleNode("td[1]").FirstChild;
				var petId = string.Empty;
				if (petIdRaw is not null && tableOffset != 1)
				{
					var attr = petIdRaw.GetAttributeValue("data-item", null);
					// If this is null, then this means that we must offset by one since the first column is the
					// character display. Then, it follows that there is NOT a pet column and we no longer need to
					// worry about pets.
					if (attr is null)
						tableOffset = 1;
					else
						petId = attr;
				}

				var isParsedPetId = int.TryParse(petId, out var parsedPetId);

				// character display: column 2
				var characterDisplay = characterRow.SelectSingleNode($"td[{2 - tableOffset}]");
				var characterDisplayInfo = GetCharacterDisplayInfo(characterDisplay);

				// character class type: column 3
				var characterType = characterRow.SelectSingleNode($"td[{3 - tableOffset}]").InnerText;

				// level of character
				var level = int.TryParse(characterRow.SelectSingleNode($"td[{4 - tableOffset}]").InnerText,
					out var lvl)
					? lvl
					: -1;
				
				// alive fame
				var fame = long.TryParse(characterRow.SelectSingleNode($"td[{5 - tableOffset}]").InnerText,
					out var f)
					? f
					: -1;

				// rank
				var place = int.TryParse(characterRow.SelectSingleNode($"td[{6 - tableOffset}]").InnerText,
					out var p)
					? p
					: -1;

				// equipment
				var equips = characterRow
					.SelectSingleNode($"td[{7 - tableOffset}]")
					// <span class="item-wrapper">...
					.ChildNodes;
				var characterEquipment = GetEquipment(equips);

				// player stats 
				// <span class = "player-stats" ...
				var stats = characterRow
					.SelectSingleNode($"td[{8 - tableOffset}]");

				var maxedStats = stats.InnerText != null && stats.InnerText.Contains('/')
					? int.TryParse(stats.InnerText.Split('/')[0], out var ms)
						? ms
						: -1
					: -1;

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

				var charStats = new Dictionary<string, int>();
				var possibleStats = new[]
				{
					"Health",
					"Magic",
					"Attack",
					"Defense",
					"Speed",
					"Vitality",
					"Wisdom",
					"Dexterity"
				};

				var lengthOfStats = Math.Min(dataStats.Length, bonusStats.Length);
				var indexOfStats = 0;
				for (; indexOfStats < lengthOfStats; ++indexOfStats)
					charStats.Add(possibleStats[indexOfStats], dataStats[indexOfStats] - bonusStats[indexOfStats]);

				for (; indexOfStats < 8; ++indexOfStats)
					charStats.Add(possibleStats[indexOfStats], -1);

				returnData.Characters.Add(new CharacterEntry
				{
					Pet = new PetInfo
					{
						Name = isParsedPetId
							? IdToItem.TryGetValue(parsedPetId, out var a) ? a.Name : $"PET_ID: {petId}"
							: string.Empty,
						Id = isParsedPetId ? parsedPetId : -1
					},
					CharacterSkin = characterDisplayInfo,
					CharacterType = characterType,
					EquipmentData = characterEquipment.ToArray(),
					Fame = fame,
					HasBackpack = equips.Count == 5,
					Level = level,
					Place = place,
					StatsMaxed = maxedStats,
					Stats = charStats
				});
			}

			// Console.WriteLine("Document Char Time: " + sw.Elapsed.TotalMilliseconds + "ms");
			// sw.Stop();

			return returnData;
		}

		/// <summary>
		///     <para>Returns the player's pet yard information.</para>
		///     <para>This will include information for each pet, including its abilities and rankings.</para>
		/// </summary>
		/// <param name="name">The name of the person to look up.</param>
		/// <returns>The person's pet yard data.</returns>
		public static async Task<PetYardData> ScrapePetYardAsync(string name)
		{
			var document = await GetDocument($"{RealmEyeBaseUrl}/{PetYardSegment}/{name}");

			if (document is null)
				return new PetYardData { ResultCode = ResultCode.ServiceUnavailable, Name = name };

			if (IsPrivate(document))
				return new PetYardData { ResultCode = ResultCode.NotFound, Name = name };

			var returnData = new PetYardData
			{
				ResultCode = ResultCode.Success,
				ProfileIsPrivate = false,
				Name = name
			};

			var mainElem = document.DocumentNode.CssSelect(".col-md-12").First();
			var petsPrivateTag = mainElem.SelectSingleNode("//div[@class='col-md-12']/h3");

			if (petsPrivateTag is { InnerText: "Pets are hidden." })
				return new PetYardData { ProfileIsPrivate = false };

			returnData.SectionIsPrivate = false;
			returnData.Pets = new List<PetEntry>();

			if (petsPrivateTag != null && petsPrivateTag.InnerText.Contains("has no pets."))
				return returnData;

			var petTable = document.DocumentNode.SelectNodes("//table[@id]/tbody/tr");

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
					Id = petId,
					PetSkinName = IdToItem.TryGetValue(petId, out var data)
						? data.Name
						: $"PET_ID: {petId}",
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
		///     <para>Returns the player's graveyard. This can return up to every page in the graveyard.</para>
		///     <para>Bear in mind that, depending on the limit given, this operation may take a while.</para>
		/// </summary>
		/// <param name="name">The name of the person to look up.</param>
		/// <param name="limit">The maximum number of graveyard entries to look up.</param>
		/// <returns>The person's graveyard.</returns>
		public static async Task<GraveyardData> ScrapeGraveyardAsync(string name, int limit = -1)
		{
			if (limit < -1)
				limit = -1;

			// to avoid huge memory consumptions
			if (limit > 100)
				limit = 100;

			var document = await GetDocument($"{RealmEyeBaseUrl}/{GraveyardSegment}/{name}");

			if (document is null)
				return new GraveyardData { ResultCode = ResultCode.ServiceUnavailable, Name = name };

			if (IsPrivate(document))
				return new GraveyardData { ResultCode = ResultCode.NotFound, Name = name };

			var colMd = document.DocumentNode.CssSelect(".col-md-12").First();
			// this probably isnt the best way
			// to do it.
			var gyInfoPara = colMd.SelectSingleNode("//div[@class='col-md-12']/p/text()");
			var gyInfoHead = colMd.SelectSingleNode("//div[@class='col-md-12']/h3/text()");

			if (gyInfoHead != null
			    && gyInfoHead.InnerText.Contains("is hidden")
			    && gyInfoHead.InnerText.Contains("The graveyard of"))
				return new GraveyardData { ResultCode = ResultCode.Success, ProfileIsPrivate = false };

			if (gyInfoHead is { InnerText: "No data available yet." })
				return new GraveyardData
				{
					ResultCode = ResultCode.Success,
					Graveyard = new List<GraveyardEntry>(),
					ProfileIsPrivate = false,
					SectionIsPrivate = false,
					Name = name
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
				SectionIsPrivate = false,
				Name = name
			};

			// no dead characters
			if (numGraveyards == 0)
				return returnData;

			var lowestPossibleAmt = Math.Floor((double)numGraveyards / 100) * 100;
			if (limit != -1 && limit <= numGraveyards)
				lowestPossibleAmt = Math.Floor((double)limit / 100) * 100;

			returnData.GraveyardCount = numGraveyards;

			// iterate over each page in the damn website :( 
			for (var index = 1; index <= lowestPossibleAmt + 1; index += 100)
			{
				if (index != 1)
				{
					document = await GetDocument($"{RealmEyeBaseUrl}/{GraveyardSegment}/{name}/{index}");
					if (document is null) break;
				}

				var graveyardTable = document.DocumentNode
					.CssSelect(".table-responsive")
					.CssSelect(".table")
					.First()
					// <tbody><tr>
					.SelectNodes("tbody/tr");

				// td[1] => date
				// td[2] => character dye info
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

					// character display: column 2
					var characterDisplay = gyRow.SelectSingleNode("td[2]");
					var characterDisplayInfo = GetCharacterDisplayInfo(characterDisplay);

					var character = gyRow.SelectSingleNode("td[3]").InnerText;
					var level = int.Parse(gyRow.SelectSingleNode("td[4]").InnerText);
					var baseFame = int.Parse(gyRow.SelectSingleNode("td[5]").InnerText);
					var totalFame = int.Parse(gyRow.SelectSingleNode("td[6]").FirstChild.InnerText);
					var exp = long.Parse(gyRow.SelectSingleNode("td[7]").InnerText);

					var equips = gyRow
						.SelectSingleNode("td[8]")
						// <span class="item-wrapper">...
						.ChildNodes;
					var characterEquipment = GetEquipment(equips);

					var hadBackpack = equips.Count == 5;
					var statsMaxed = int.Parse(gyRow.SelectSingleNode("td[9]")
						.FirstChild.InnerText.Split('/')[0]);
					var diedTo = gyRow.SelectSingleNode("td[10]").InnerText;

					returnData.Graveyard.Add(new GraveyardEntry
					{
						DiedOn = diedOn,
						CharacterSkin = characterDisplayInfo,
						BaseFame = baseFame,
						Character = character,
						Equipment = characterEquipment.ToArray(),
						Experience = exp,
						HadBackpack = hadBackpack,
						KilledBy = HttpUtility.HtmlDecode(diedTo),
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
		///     <para>Returns the player's graveyard summary.</para>
		/// </summary>
		/// <param name="name">The name of the person to look up.</param>
		/// <returns>The person's graveyard summary.</returns>
		public static async Task<GraveyardSummaryData> ScrapeGraveyardSummaryAsync(string name)
		{
			var document = await GetDocument($"{RealmEyeBaseUrl}/{GraveyardSummarySegment}/{name}");

			if (document is null)
				return new GraveyardSummaryData { ResultCode = ResultCode.ServiceUnavailable, Name = name };

			if (IsPrivate(document))
				return new GraveyardSummaryData { ResultCode = ResultCode.NotFound, Name = name };

			// this probably isnt the best way
			// to do it.
			var colMd = document.DocumentNode.CssSelect(".col-md-12").First();
			var gyInfoHead = colMd.SelectSingleNode("//div[@class='col-md-12']/h3/text()");

			if (gyInfoHead != null
			    && gyInfoHead.InnerText.Contains("is hidden")
			    && gyInfoHead.InnerText.Contains("The graveyard of"))
				return new GraveyardSummaryData
					{ ResultCode = ResultCode.Success, ProfileIsPrivate = false, Name = name };

			var returnData = new GraveyardSummaryData
			{
				ResultCode = ResultCode.Success,
				ProfileIsPrivate = false,
				SectionIsPrivate = false,
				Properties = new List<GraveyardSummaryProperty>(),
				StatsCharacters = new List<MaxedStatsByCharacters>(),
				TechnicalProperties = new List<GraveyardTechnicalProperty>(),
				Name = name
			};

			if (gyInfoHead is { InnerText: "No data available yet." })
				return returnData;

			var allPossibleTables = document.DocumentNode
				.CssSelect(".table-responsive")
				.CssSelect(".table")
				.ToArray();

			if (allPossibleTables.Length < 3)
				return returnData;

			var firstSummaryTable = allPossibleTables[0]
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
				Achievement = HttpUtility.HtmlDecode(row.SelectSingleNode("td[2]").InnerText),
				Total = long.Parse(row.SelectSingleNode("td[3]").InnerText),
				Max = long.Parse(row.SelectSingleNode("td[4]").InnerText),
				Average = double.Parse(row.SelectSingleNode("td[5]").InnerText),
				Min = long.Parse(row.SelectSingleNode("td[6]").InnerText)
			}).ToList();

			var secondSummaryTable = allPossibleTables[1]
				.SelectNodes("tr");

			// td[1] => name
			// td[2] => total
			// td[3] => max
			// td[4] => avg
			// td[5] => min
			var techProps = secondSummaryTable.Select(row => new GraveyardTechnicalProperty
			{
				Achievement = HttpUtility.HtmlDecode(row.SelectSingleNode("td[1]").InnerText),
				Total = row.SelectSingleNode("td[2]").InnerText,
				Max = row.SelectSingleNode("td[3]").InnerText,
				Average = row.SelectSingleNode("td[4]").InnerText,
				Min = row.SelectSingleNode("td[5]").InnerText
			}).ToList();

			var thirdSummaryTable = allPossibleTables[2]
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
						: int.Parse(row.SelectSingleNode("td[10]").InnerText)
				},
				Total = int.Parse(row.SelectSingleNode("td[11]").InnerText)
			}).ToList();

			returnData.Properties = props;
			returnData.StatsCharacters = charInfo;
			returnData.TechnicalProperties = techProps;

			return returnData;
		}

		/// <summary>
		///     <para>Returns the player's name history.</para>
		/// </summary>
		/// <param name="name">The name of the person to look up.</param>
		/// <returns>The person's name history.</returns>
		public static async Task<NameHistoryData> ScrapeNameHistoryAsync(string name)
		{
			var document = await GetDocument($"{RealmEyeBaseUrl}/{NameHistorySegment}/{name}");

			if (document is null)
				return new NameHistoryData { ResultCode = ResultCode.ServiceUnavailable, Name = name };

			if (IsPrivate(document))
				return new NameHistoryData { ResultCode = ResultCode.NotFound, Name = name };

			var returnData = new NameHistoryData
			{
				ResultCode = ResultCode.Success,
				ProfileIsPrivate = false,
				Name = name
			};

			var colMd = document.DocumentNode.CssSelect(".col-md-12").First();
			var hiddenTxtHeader = colMd.SelectSingleNode("//div[@class='col-md-12']/h3/text()");
			if (hiddenTxtHeader != null && hiddenTxtHeader.InnerText.Contains("Name history is hidden"))
				return returnData;

			returnData.SectionIsPrivate = false;
			returnData.NameHistory = new List<NameHistoryEntry>();

			var nameHistExists = colMd.SelectNodes("//div[@class='col-md-12']/p/text()");
			if (nameHistExists.Count == 2 && nameHistExists.Last().InnerText.Contains("No name changes detected."))
				return returnData;

			var nameHistoryColl = document.DocumentNode
				.CssSelect(".table-responsive")
				.CssSelect(".table")
				.First()
				// <tbody><tr>
				.SelectNodes("tbody/tr");

			// td[1] => name
			// td[2] => from
			// td[3] => to
			foreach (var nameHistoryEntry in nameHistoryColl)
				returnData.NameHistory.Add(new NameHistoryEntry
				{
					Name = nameHistoryEntry.SelectSingleNode("td[1]").InnerText,
					From = nameHistoryEntry.SelectSingleNode("td[2]").InnerText,
					To = nameHistoryEntry.SelectSingleNode("td[3]").InnerText
				});

			return returnData;
		}

		/// <summary>
		///     <para>Returns the player's fame history. This is a WIP.</para>
		/// </summary>
		/// <param name="name">The name of the person to look up.</param>
		/// <returns>The person's fame history.</returns>
		public static async Task<FameHistoryData> ScrapeFameHistoryAsync(string name)
		{
			var document = await GetDocument($"{RealmEyeBaseUrl}/{GuildHistorySegment}/{name}");

			if (document is null)
				return new FameHistoryData { ResultCode = ResultCode.ServiceUnavailable, Name = name };

			if (IsPrivate(document))
				return new FameHistoryData { ResultCode = ResultCode.NotFound, Name = name };

			var returnData = new FameHistoryData
			{
				ProfileIsPrivate = false,
				ResultCode = ResultCode.Success,
				Name = name
			};

			var colMd = document.DocumentNode.CssSelect(".col-md-12").First();
			var hiddenTxtHeader = colMd.SelectSingleNode("//div[@class='col-md-12']/h3/text()");
			if (hiddenTxtHeader != null && hiddenTxtHeader.InnerText.Contains("Fame history is hidden"))
				return returnData;

			returnData.SectionIsPrivate = false;
			var fameNode = document.DocumentNode
				.Descendants("script")
				.FirstOrDefault(x => x.InnerText.Contains("initializeSearch")
				                     && x.InnerText.Contains("initializeGraphs"));

			if (fameNode is null)
				return returnData;

			var arraysStr = fameNode.InnerText
				.Split("initializeGraphs(")[1]
				.Split(", \"")[0];

			// https://stackoverflow.com/questions/740642/c-sharp-regex-split-everything-inside-square-brackets
			// For regex 
			// Count of 1 = only daily
			// Count of 2 = daily + weekly
			// Count of 3 = all graphs
			var count = Regex.Matches(arraysStr, "]]").Count;
			Debug.WriteLine(count);
			var unparsedArr = arraysStr.Split("]]", StringSplitOptions.RemoveEmptyEntries);

			var dateTime = DateTime.Now;
			var offset = 0;
			// All graphs
			if (count >= 3)
			{
				var allGraphsArr = unparsedArr[^1] + "]";
				allGraphsArr = allGraphsArr.Replace(", [[", "[");
				var res = SquareBracketRegex.Match(allGraphsArr);
				while (res.Success)
				{
					var timeFame = res.Groups[1].Value
						.Split(",")
						.Select(x => long.Parse(x.Trim()))
						.ToArray();
					var date = new DateTime(dateTime.Ticks - 1000 * timeFame[0]);
					returnData.AllTime.Add(new TimeFame { Fame = timeFame[1], Time = date.Ticks });
					res = res.NextMatch();
				}

				offset++;
			}

			// week
			if (count >= 2)
			{
				var allGraphsArr = unparsedArr[^(1 + offset)] + "]";
				allGraphsArr = allGraphsArr.Replace(", [[", "[");
				var res = SquareBracketRegex.Match(allGraphsArr);
				while (res.Success)
				{
					var timeFame = res.Groups[1].Value
						.Split(",")
						.Select(x => long.Parse(x.Trim()))
						.ToArray();
					var date = new DateTime(dateTime.Ticks - 1000 * timeFame[0]);
					returnData.Week.Add(new TimeFame { Fame = timeFame[1], Time = date.Ticks });
					res = res.NextMatch();
				}

				offset++;
			}

			// day
			if (count >= 1)
			{
				var allGraphsArr = "[" + unparsedArr[^(1 + offset)]
					.Split("initializeGraphs([[")[1] + "]";
				var res = SquareBracketRegex.Match(allGraphsArr);
				while (res.Success)
				{
					var timeFame = res.Groups[1].Value
						.Split(",")
						.Select(x => long.Parse(x.Trim()))
						.ToArray();
					var date = new DateTime(dateTime.Ticks - 1000 * timeFame[0]);
					returnData.Hour.Add(new TimeFame { Fame = timeFame[1], Time = date.Ticks });
					res = res.NextMatch();
				}
			}

			return returnData;
		}

		/// <summary>
		///     <para>Returns the player's rank history.</para>
		/// </summary>
		/// <param name="name">The name of the person to look up.</param>
		/// <returns>The person's rank history.</returns>
		public static async Task<RankHistoryData> ScrapeRankHistoryAsync(string name)
		{
			var document = await GetDocument($"{RealmEyeBaseUrl}/{RankHistorySegment}/{name}");

			if (document is null)
				return new RankHistoryData { ResultCode = ResultCode.ServiceUnavailable, Name = name };

			if (IsPrivate(document))
				return new RankHistoryData { ResultCode = ResultCode.NotFound, Name = name };

			var returnData = new RankHistoryData
			{
				ProfileIsPrivate = false,
				ResultCode = ResultCode.Success,
				Name = name
			};

			var colMd = document.DocumentNode.CssSelect(".col-md-12").First();

			var hiddenTxtHeader = colMd.SelectSingleNode("//div[@class='col-md-12']/h3/text()");
			if (hiddenTxtHeader != null && hiddenTxtHeader.InnerText.Contains("Rank history is hidden"))
				return returnData;

			returnData.SectionIsPrivate = false;
			returnData.RankHistory = new List<RankHistoryEntry>();

			var rankHistoryColl = document.DocumentNode
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
		///     <para>Returns the player's guild history.</para>
		/// </summary>
		/// <param name="name">The name of the person to look up.</param>
		/// <returns>The person's guild history.</returns>
		public static async Task<GuildHistoryData> ScrapeGuildHistoryAsync(string name)
		{
			var document = await GetDocument($"{RealmEyeBaseUrl}/{GuildHistorySegment}/{name}");

			if (document is null)
				return new GuildHistoryData { ResultCode = ResultCode.ServiceUnavailable, Name = name };

			if (IsPrivate(document))
				return new GuildHistoryData { ResultCode = ResultCode.NotFound, Name = name };

			var returnData = new GuildHistoryData
			{
				ProfileIsPrivate = false,
				ResultCode = ResultCode.Success,
				Name = name
			};

			var colMd = document.DocumentNode.CssSelect(".col-md-12").First();
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

			var guildHistoryColl = document.DocumentNode
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
				returnData.GuildHistory.Add(new GuildHistoryEntry
				{
					GuildName = WebUtility.HtmlDecode(guildHistoryRow.SelectSingleNode("td[1]").FirstChild.InnerText),
					GuildRank = guildHistoryRow.SelectSingleNode("td[2]").InnerText,
					From = guildHistoryRow.SelectSingleNode("td[3]").InnerText,
					To = guildHistoryRow.SelectSingleNode("td[4]").InnerText
				});

			return returnData;
		}

		/// <summary>
		///     <para>Returns the player's exaltation data.</para>
		/// </summary>
		/// <param name="name">The name of the person to look up..</param>
		/// <returns>The person's exaltations.</returns>
		public static async Task<ExaltationData> ScrapeExaltationsAsync(string name)
		{
			var document = await GetDocument($"{RealmEyeBaseUrl}/{ExaltationSegment}/{name}");

			if (document is null)
				return new ExaltationData { ResultCode = ResultCode.ServiceUnavailable, Name = name };

			if (IsPrivate(document))
				return new ExaltationData { ResultCode = ResultCode.NotFound, Name = name };

			var returnData = new ExaltationData
			{
				ProfileIsPrivate = false,
				ResultCode = ResultCode.Success,
				Name = name
			};

			var colMd = document.DocumentNode.CssSelect(".col-md-12").First();
			var hiddenTxtHeader = colMd.SelectSingleNode("//div[@class='col-md-12']/h3/text()");

			if (hiddenTxtHeader != null && hiddenTxtHeader.InnerText.Contains("Exaltations are hidden"))
				return returnData;

			returnData.SectionIsPrivate = false;
			returnData.Exaltations = new List<ExaltationEntry>();

			if (hiddenTxtHeader != null && hiddenTxtHeader.InnerText.Contains("No exaltations"))
				return returnData;

			var exaltationTable = document.DocumentNode
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

		#region RealmEye Player URLs

		private const string PlayerSegment = "player";
		private const string ExaltationSegment = "exaltations-of";
		private const string PetYardSegment = "pets-of";
		private const string GraveyardSegment = "graveyard-of-player";
		private const string GraveyardSummarySegment = "graveyard-summary-of-player";
		private const string RankHistorySegment = "rank-history-of-player";
		private const string NameHistorySegment = "name-history-of-player";
		private const string GuildHistorySegment = "guild-history-of-player";

		#endregion
	}
}