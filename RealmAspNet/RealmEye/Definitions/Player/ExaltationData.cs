using System.Collections.Generic;

namespace RealmAspNet.RealmEye.Definitions.Player
{
	public class ExaltationData : RealmEyePlayerResponse
	{
		public IList<ExaltationEntry> Exaltations { get; set; }
	}

	public struct ExaltationEntry
	{
		public string Class { get; init; }
		public int ExaltationAmount { get; init; }
		public Stats ExaltationStats { get; init; }
	}

	public struct Stats
	{
		public int Health { get; init; }
		public int Magic { get; init; }
		public int Attack { get; init; }
		public int Defense { get; init; }
		public int Speed { get; init; }
		public int Vitality { get; init; }
		public int Wisdom { get; init; }
		public int Dexterity { get; init; }
	}
}