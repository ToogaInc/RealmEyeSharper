using System.Collections;
using System.Collections.Generic;

namespace RealmEyeSharper.Definitions.Player
{
	public class ExaltationData
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