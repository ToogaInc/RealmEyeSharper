using System.Collections.Generic;

namespace RealmAspNet.RealmEye.Definitions.Player
{
	public class NameHistoryData : RealmEyePlayerResponse
	{
		public IList<NameHistoryEntry> NameHistory { get; set; }
	}

	public class NameHistoryEntry
	{
		public string Name { get; init; }
		public string From { get; init; }
		public string To { get; init; }
	}
}