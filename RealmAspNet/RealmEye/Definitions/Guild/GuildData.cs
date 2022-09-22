namespace RealmAspNet.RealmEye.Definitions.Guild
{
	public class GuildData : RealmEyeGuildResponse
	{
		public int MemberCount { get; set; }
		public string[] Description { get; set; }
		public long Fame { get; set; }
		public string MostActiveOn { get; set; }
		public int CharacterCount { get; set; }
		public int NumPrivate { get; set; }
		public GuildMember[] Members { get; set; }
	}

	public class GuildMember
	{
		public string Name { get; set; }
		public string GuildRank { get; set; }
		public long Fame { get; set; }
		public int Rank { get; set; }
		public int Characters { get; set; }
		public string LastSeenTime { get; set; }
		public string LastSeenServer { get; set; }
		public long AverageFamePerCharacter { get; set; }
	}
}