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
		public string DiedOn { get; set; }
		public string Character { get; set; }
		public int Level { get; set; }
		public int BaseFame { get; set; }
		public int TotalFame { get; set; }
		public long Experience { get; set; }
		public string[] Equipment { get; set; }
		public int MaxedStats { get; set; }
		public string KilledBy { get; set; }
		public bool HadBackpack { get; set; }
	}
}