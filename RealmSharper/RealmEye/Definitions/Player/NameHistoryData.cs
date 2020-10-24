using System.Collections.Generic;

namespace RealmSharper.RealmEye.Definitions.Player
{
	public class NameHistoryData : RealmEyeResponse
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