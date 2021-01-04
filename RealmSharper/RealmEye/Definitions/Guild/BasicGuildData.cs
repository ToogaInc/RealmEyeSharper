using System.Collections.Generic;

namespace RealmSharper.RealmEye.Definitions.Guild
{
	public class BasicGuildData
	{
		public int Members { get; set; }
		public int Characters { get; set; }
		public long Fame { get; set; }
		public long FameRank { get; set; }
		public long Exp { get; set; }
		public long ExpRank { get; set; }
		public string MostActiveOn { get; set; }
		public long MostActiveOnRank { get; set; }
		public string[] Description { get; set; }
		public IList<MemberEntry> MemberEntries { get; set; }
		public int NumberOfPrivateMembers { get; set; }
	}

	public struct MemberEntry
	{
		public string Name { get; set; }
		public string GuildRank { get; set; }
		public long Fame { get; set; }
		public long Exp { get; set; }
		public int Rank { get; set; }
		public int Characters { get; set; }
		public string LastSeen { get; set; }
		public string MainServer { get; set; }
		public int AverageFamePerCharacter { get; set; }
		public long AverageExpPerCharacter { get; set; }
	}
}