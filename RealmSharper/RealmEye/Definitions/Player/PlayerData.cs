using System.Collections.Generic;

namespace RealmSharper.RealmEye.Definitions
{
	public class PlayerData : RealmEyePlayerResponse
	{
		public int CharacterCount { get; set; }
		public int Skins { get; set; }
		public int Fame { get; set; }
		public int Exp { get; set; }
		public int Rank { get; set; }
		public int AccountFame { get; set; }
		public string Guild { get; set; }
		public string GuildRank { get; set; }
		public string FirstSeen { get; set; }
		public string Created { get; set; }
		public string LastSeen { get; set; }
		public string[] Description { get; set; }
		public IList<CharacterEntry> Characters { get; set; }
	}

	public struct CharacterEntry
	{
		public string Pet { get; init; }
		public string CharacterType { get; init; }
		public int Level { get; init; }
		public int ClassQuestsCompleted { get; init; }
		public int Fame { get; init; }
		public long Experience { get; init; }
		public int Place { get; init; }
		public string[] EquipmentData { get; init; }
		public bool HasBackpack { get; init; }
		public IDictionary<string, int> Stats { get; init; }
		public int StatsMaxed { get; init; }
	}
}