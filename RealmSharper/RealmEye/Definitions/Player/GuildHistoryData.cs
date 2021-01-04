using System.Collections.Generic;

namespace RealmSharper.RealmEye.Definitions.Player
{
	public class GuildHistoryData : RealmEyePlayerResponse
	{
		public IList<GuildHistoryEntry> GuildHistory { get; set; }
	}

	public struct GuildHistoryEntry
	{
		public string GuildName { get; set; }
		public string GuildRank { get; set; }
		public string From { get; set; }
		public string To { get; set; }
	}
}