using System.Collections.Generic;
using System.Threading.Tasks;
using RealmAspNet.RealmEye.Definitions.Common;
using static RealmAspNet.RealmEye.Constants;
using static RealmAspNet.RealmEye.RealmEyeCommon;

namespace RealmAspNet.RealmEye
{
	public static class CharactersScraper
	{
		
		public const string RecentDeathsSegment = "recent-deaths";

		// TODO 
		public static async Task<List<GraveyardEntry>> ScrapeRecentDeathsAsync(int limit = -1)
		{
			if (limit < -1)
				limit = -1;

			// to avoid huge memory consumptions
			if (limit > 300)
				limit = 300;

			var document = await GetDocument($"{RealmEyeBaseUrl}/{RecentDeathsSegment}");

			if (document is null)
				return new List<GraveyardEntry>();

			return new List<GraveyardEntry>();
		}
	}
}