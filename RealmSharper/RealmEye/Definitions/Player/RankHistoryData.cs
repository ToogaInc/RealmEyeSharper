using System.Collections.Generic;

namespace RealmSharper.RealmEye.Definitions
{
	public class RankHistoryData : RealmEyePlayerResponse
	{
		public IList<RankHistoryEntry> RankHistory { get; set; }
	}

	public struct RankHistoryEntry
	{
		public int Rank { get; init; }
		public string Achieved { get; init; }
		public string Date { get; init; }
	}
}