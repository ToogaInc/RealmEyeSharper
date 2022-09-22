using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using RealmAspNet.RealmEye.Definitions;
using RealmAspNet.RealmEye.Definitions.Guild;
using ScrapySharp.Extensions;
using static RealmAspNet.RealmEye.Constants;
using static RealmAspNet.RealmEye.RealmEyeCommon;

namespace RealmAspNet.RealmEye
{
	public static class GuildScraper
	{
		/// <summary>
		///     Whether the guild profile is available or not.
		/// </summary>
		/// <param name="doc">The HtmlDocument representing the RealmEye page.</param>
		/// <returns>Whether the guild profile is private or not.</returns>
		private static bool IsAvailable(HtmlDocument doc)
		{
			var mainElement = doc.DocumentNode.CssSelect(".col-md-12");
			return mainElement.CssSelect(".col-md-8").Any();
		}

		/// <summary>
		///     <para>Returns the guild's introductory RealmEye page.</para>
		/// </summary>
		/// <param name="guildName">The name of the guild to look up.</param>
		/// <returns>The player data.</returns>
		public static async Task<GuildData> ScrapeGuildProfileAsync(string guildName)
		{
			var document = await GetDocument($"{RealmEyeBaseUrl}/{GuildSegment}/{guildName}");

			if (document is null)
				return new GuildData { ResultCode = ResultCode.ServiceUnavailable, Name = guildName };

			if (IsAvailable(document))
				return new GuildData { ResultCode = ResultCode.NotFound, Name = guildName };

			var returnData = new GuildData
			{
				Name = document.DocumentNode.CssSelect(".entity-name").First().InnerText,
				ResultCode = ResultCode.Success
			};

			var summaryTable = document.DocumentNode.CssSelect(".summary").First();
			foreach (var row in summaryTable.SelectNodes("tr"))
			foreach (var col in row.SelectNodes("td[1]"))
				switch (col.InnerText)
				{
					case "Members":
						returnData.MemberCount = int.Parse(col.NextSibling.InnerText);
						break;
					case "Characters":
						returnData.CharacterCount = int.Parse(col.NextSibling.InnerHtml);
						break;
					case "Fame":
						returnData.Fame = long.Parse(col.NextSibling.InnerText.Split('(')[0]);
						break;
					case "Most active on":
						returnData.MostActiveOn = col.NextSibling.InnerText.Split('(')[0].Trim();
						break;
				}

			// Get guild description
			var desc = new List<string>();
			for (var i = 1; i <= 3; i++)
			{
				var possDesc = document.DocumentNode.SelectNodes($"//div[contains(@class, 'line{i}')]");
				if (possDesc is not null && possDesc.Count != 0 && possDesc[0].InnerText.Length != 0)
					desc.Add(HttpUtility.HtmlDecode(possDesc[0].InnerText));
			}

			returnData.Description = desc.ToArray();

			// Get all members
			var memberList = document.DocumentNode.SelectNodes("//table[@id]/tbody/tr");
			if (memberList is null || memberList.Count == 0)
				return returnData;

			var memberArr = memberList.ToArray();
			returnData.NumPrivate = 0;
			var members = new List<GuildMember>();
			// Several interesting cases to consider. For some reason, RealmEye may sometimes add
			// a blank first column. One obvious case is when someone's membership in a guild 
			// can't be "verified" by RealmEye (for which a question mark appears by the name). The
			// other, more subtle, case is probably when someone's membership can't be "verified" and
			// they left.
			var offset = 0;
			if (memberArr.Any(x => x.InnerHtml.Contains("glyphicon glyphicon-question-sign")))
				offset = 1;
			else if (memberArr.All(x => x.SelectSingleNode("td[1]").InnerHtml.Length == 0))
				offset = 1;

			foreach (var memberRow in memberArr)
			{
				// Name: column 1
				// <div class="star-container"><a href="...">...</a><div class="..."></div></div>
				var nameNode = memberRow.SelectSingleNode($"td[{1 + offset}]")
					// <a href="...">...</a><div class="..."></div>
					.FirstChild;
				// If there are no child nodes, then this means that
				// this profile is private
				if (nameNode.ChildNodes.Count == 0)
				{
					returnData.NumPrivate++;
					continue;
				}

				// Everything inside <div class="star-container">...
				var name = nameNode.InnerText;

				// Rank: column 2
				var guildRank = memberRow.SelectSingleNode($"td[{2 + offset}]")
					.InnerText;

				// Fame: column 3
				// This is guaranteed to be here
				var fame = int.TryParse(memberRow.SelectSingleNode($"td[{3 + offset}]").InnerText,
					out var p)
					? p
					: -1;
				
				// Rank/Stars: column 4
				// Also guaranteed to be here
				var stars = int.TryParse(memberRow.SelectSingleNode($"td[{4 + offset}]").InnerText,
					out var r)
					? r
					: -1;

				// Number of characters: column 5
				var charCt = int.TryParse(memberRow.SelectSingleNode($"td[{5 + offset}]").InnerText,
					out var c)
					? c
					: -1;

				// Last seen: column 6
				var lastSeenTimeNodes = memberRow.SelectSingleNode($"td[{6 + offset}]").ChildNodes;
				var lastSeenTime = lastSeenTimeNodes.Count == 0
					? string.Empty
					: lastSeenTimeNodes[0].InnerText;

				// Last seen server: column 7
				var lastSeenServer = memberRow.SelectSingleNode($"td[{7 + offset}]").InnerText;

				// Avg. fame/char: column 8
				var avgFameChar = long.TryParse(memberRow.SelectSingleNode($"td[{8 + offset}]").InnerText,
					out var a)
					? a
					: -1;
				
				members.Add(new GuildMember
				{
					Name = name,
					GuildRank = guildRank,
					Fame = fame,
					Rank = stars,
					LastSeenServer = lastSeenServer,
					LastSeenTime = lastSeenTime,
					AverageFamePerCharacter = avgFameChar,
					Characters = charCt
				});
			}

			returnData.Members = members.ToArray();
			return returnData;
		}

		#region RealmEye Guild URLs

		private const string GuildSegment = "guild";
		private const string TopCharSegment = "top-characters-of-guild";
		private const string TopPetsSegment = "top-pets-of-guild";
		private const string RecentDeathsSegment = "recent-deaths-in-guild";
		private const string FormerMembersSegment = "fame-history-of-guild";

		#endregion
	}
}