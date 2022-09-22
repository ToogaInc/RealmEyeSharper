using System.Collections.Generic;

namespace RealmAspNet.RealmEye.Definitions.Player
{
	public class PlayerData : RealmEyePlayerResponse
	{
		public int CharacterCount { get; set; }
		public int Skins { get; set; }
		public long Fame { get; set; }
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
		public PetInfo Pet { get; init; }
		public CharacterSkinInfo CharacterSkin { get; init; }
		public string CharacterType { get; init; }
		public int Level { get; init; }
		public long Fame { get; init; }
		public int Place { get; init; }
		public GearInfo[] EquipmentData { get; init; }
		public bool HasBackpack { get; init; }
		public IDictionary<string, int> Stats { get; init; }
		public int StatsMaxed { get; init; }
	}

	public struct GearInfo
	{
		public string Name { get; init; }
		public string Tier { get; init; }
		public int Id { get; init; }
	}

	public struct PetInfo
	{
		public string Name { get; init; }
		public int Id { get; init; }
	}

	public struct CharacterSkinInfo
	{
		// Small
		public int ClothingDyeId { get; set; }
		public string ClothingDyeName { get; set; }

		// Big
		public int AccessoryDyeId { get; set; }
		public string AccessoryDyeName { get; set; }

		// Skin
		public int SkinId { get; set; }
	}
}