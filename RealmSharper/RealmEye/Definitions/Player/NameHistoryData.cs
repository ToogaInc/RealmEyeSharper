using System.Collections.Generic;

namespace RealmSharper.RealmEye.Definitions
{
	public class NameHistoryData : RealmEyePlayerResponse
	{
		public IList<NameHistoryEntry> NameHistory { get; set; }
	}

	public class NameHistoryEntry
	{
		public string Name { get; set; }
		public string From { get; set; }
		public string To { get; set; }
	}
}