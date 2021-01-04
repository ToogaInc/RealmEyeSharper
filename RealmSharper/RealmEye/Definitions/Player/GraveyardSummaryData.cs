using System.Collections.Generic;

namespace RealmSharper.RealmEye.Definitions.Player
{
	public class GraveyardSummaryData : RealmEyePlayerResponse
	{
		public IList<GraveyardSummaryProperty> Properties { get; set; }
		public IList<GraveyardTechnicalProperty> TechnicalProperties { get; set; }
		public IList<MaxedStatsByCharacters> StatsCharacters { get; set; }
	}

	public struct MaxedStatsByCharacters
	{
		public string CharacterType { get; set; }
		// Stats[0] = 0/8; Stats[8] = 8/8
		public int[] Stats { get; set; }
		public int Total { get; set; }
	}

	public struct GraveyardSummaryProperty
	{
		public string Achievement { get; set; }
		public long Total { get; set; }
		public long Max { get; set; }
		public double Average { get; set; }
		public long Min { get; set; }
	}

	public struct GraveyardTechnicalProperty
	{
		public string Achievement { get; set; }
		public string Total { get; set; }
		public string Max { get; set; }
		public string Average { get; set; }
		public string Min { get; set; }
	}
}