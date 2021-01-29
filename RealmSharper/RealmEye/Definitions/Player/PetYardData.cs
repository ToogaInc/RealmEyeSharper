using System.Collections.Generic;

namespace RealmSharper.RealmEye.Definitions
{
	public class PetYardData : RealmEyePlayerResponse
	{
		public IList<PetEntry> Pets { get; set; }
	}

	public struct PetEntry
	{
		public string PetSkinName { get; init; }
		public string Name { get; init; }
		public string Rarity { get; init; }
		public string Family { get; init; }
		public int Place { get; init; }
		public IList<PetAbilityData> PetAbilities { get; init; }
		public int MaxLevel { get; init; }
	}

	public struct PetAbilityData
	{
		public bool IsUnlocked { get; init; }
		public string AbilityName { get; init; }
		public int Level { get; init; }
		public bool IsMaxed { get; init; }
	}
}