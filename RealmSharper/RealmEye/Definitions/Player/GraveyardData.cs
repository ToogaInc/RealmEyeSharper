using System.Collections.Generic;

namespace RealmSharper.RealmEye.Definitions
{
	public class GraveyardData : RealmEyePlayerResponse
	{
		public int GraveyardCount { get; set; }
		public IList<GraveyardEntry> Graveyard { get; set; }
	}

	public struct GraveyardEntry
	{
		public string DiedOn { get; init; }
		public string Character { get; init; }
		public int Level { get; init; }
		public int BaseFame { get; init; }
		public int TotalFame { get; init; }
		public long Experience { get; init; }
		public string[] Equipment { get; init; }
		public int MaxedStats { get; init; }
		public string KilledBy { get; init; }
		public bool HadBackpack { get; init; }
	}
}