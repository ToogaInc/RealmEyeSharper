using System.Collections.Generic;

namespace RealmAspNet.RealmEye.Definitions.Player
{
	public class GraveyardSummaryData : RealmEyePlayerResponse
	{
		public IList<GraveyardSummaryProperty> Properties { get; set; }
		public IList<GraveyardTechnicalProperty> TechnicalProperties { get; set; }
		public IList<MaxedStatsByCharacters> StatsCharacters { get; set; }
	}

	public struct MaxedStatsByCharacters
	{
		public string CharacterType { get; init; }
		// Stats[0] = 0/8; Stats[8] = 8/8
		public int[] Stats { get; init; }
		public int Total { get; init; }
	}

	public struct GraveyardSummaryProperty
	{
		public string Achievement { get; init; }
		public long Total { get; init; }
		public long Max { get; init; }
		public double Average { get; init; }
		public long Min { get; init; }
	}

	public struct GraveyardTechnicalProperty
	{
		public string Achievement { get; init; }
		public string Total { get; init; }
		public string Max { get; init; }
		public string Average { get; init; }
		public string Min { get; init; }
	}
}