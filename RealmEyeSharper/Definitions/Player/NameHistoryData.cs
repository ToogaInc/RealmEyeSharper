using System.Collections.Generic;

namespace RealmEyeSharper.Definitions.Player
{
	public class NameHistoryData
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