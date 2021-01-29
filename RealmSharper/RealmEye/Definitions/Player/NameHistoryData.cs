using System.Collections.Generic;

namespace RealmSharper.RealmEye.Definitions
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