using System.Linq;
using System.Threading.Tasks;
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
		public static async Task<GuildData> ScrapeGuildDataAsync(string guildName)
		{
			var document = await GetDocument($"{RealmEyeBaseUrl}/{GuildSegment}/{guildName}");
			if (document is null)
				return new GuildData {ResultCode = ResultCode.ServiceUnavailable, Name = guildName};

			var returnData = new GuildData
			{
				Name = document.DocumentNode.CssSelect(".entity-name").First().InnerText,
				ResultCode = ResultCode.Success
			};
			
			
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