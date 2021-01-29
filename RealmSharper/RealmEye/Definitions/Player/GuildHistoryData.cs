using System.Collections.Generic;

namespace RealmSharper.RealmEye.Definitions
{
	public class GuildHistoryData : RealmEyePlayerResponse
	{
		public IList<GuildHistoryEntry> GuildHistory { get; set; }
	}

	public struct GuildHistoryEntry
	{
		public string GuildName { get; init; }
		public string GuildRank { get; init; }
		public string From { get; init; }
		public string To { get; init; }
	}
}