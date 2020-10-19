using System.Collections.Generic;

namespace RealmEyeSharper.Definitions.Player
{
	public class GuildHistoryData : RealmEyeResponse
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