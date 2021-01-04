using System.Collections.Generic;

namespace RealmSharper.RealmEye.Definitions.Player
{
	public class ExaltationData : RealmEyePlayerResponse
	{
		public IList<ExaltationEntry> Exaltations { get; set; }
	}

	public struct ExaltationEntry
	{
		public string Class { get; set; }
		public int ExaltationAmount { get; set; }
		public Stats ExaltationStats { get; set; }
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