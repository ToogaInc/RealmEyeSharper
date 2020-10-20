using System.Collections.Generic;

namespace RealmEyeSharper.Definitions.Player
{
	public class RankHistoryData : RealmEyeResponse
	{
		public IList<RankHistoryEntry> RankHistory { get; set; }
	}

	public struct RankHistoryEntry
	{
		public int Rank { get; set; }
		public string Achieved { get; set; }
		public string Date { get; set; }
	}
}