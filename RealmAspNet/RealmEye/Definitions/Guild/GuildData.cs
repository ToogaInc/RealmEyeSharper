namespace RealmAspNet.RealmEye.Definitions.Guild
{
	public class GuildData : RealmEyeGuildResponse
	{
		public string GuildName { get; set; }
		public int MemberCount { get; set; }
		public long Fame { get; set; }
		public long Exp { get; set; }
		public string MostActiveOn { get; set; }
		public uint NumPrivate { get; set; }
	}

	public class GuildMember
	{
		public string Name { get; set; }
		public string GuildRank { get; set; }
		public long Fame { get; set; }
		public long Exp { get; set; }
		public int Rank { get; set; }
		public int Characters { get; set; }
		public string LastSeen { get; set; }
		public long AverageFamePerCharacter { get; set; }
		public long AverageExpPerCharacter { get; set; }
	}
}