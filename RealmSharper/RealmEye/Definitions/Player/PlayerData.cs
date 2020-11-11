using System.Collections.Generic;

namespace RealmSharper.RealmEye.Definitions.Player
{
	public class PlayerData : RealmEyeResponse
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
		public int ActivePetId { get; set; }
		public string CharacterType { get; set; }
		public int Level { get; set; }
		public int ClassQuestsCompleted { get; set; }
		public int Fame { get; set; }
		public long Experience { get; set; }
		public int Place { get; set; }
		public string[] EquipmentData { get; set; }
		public bool HasBackpack { get; set; }
		public Stats Stats { get; set; }
		public int StatsMaxed { get; set; }
	}

	public struct Stats
	{
		public int Health { get; set; }
		public int Magic { get; set; }
		public int Attack { get; set; }
		public int Defense { get; set; }
		public int Speed { get; set; }
		public int Vitality { get; set; }
		public int Wisdom { get; set; }
		public int Dexterity { get; set; }
	}
}