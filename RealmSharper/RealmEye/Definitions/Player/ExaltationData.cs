using System.Collections.Generic;

namespace RealmSharper.RealmEye.Definitions.Player
{
	public class ExaltationData : RealmEyeResponse
	{
		public IList<ExaltationEntry> Exaltations { get; set; }
	}

	public struct ExaltationEntry
	{
		public string Class { get; set; }
		public int ExaltationAmount { get; set; }
		public Stats ExaltationStats { get; set; }
	}
}