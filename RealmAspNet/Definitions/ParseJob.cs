using System.Collections.Generic;
using RealmAspNet.RealmEye.Definitions.Player;

namespace RealmAspNet.Definitions
{
	public class ParseJob
	{
		public double TotalElapsedSec { get; set; }
		public double ConcurrElapsedSec { get; set; }
		public double ParseWhoElapsedSec { get; set; }
		public int CompletedCount { get; set; }
		public int FailedCount { get; set; }
		public List<string> Input { get; set; }
		public List<string> Completed { get; set; }
		public List<string> Failed { get; set; }
		public List<string> DefaultNames { get; set; }
		public List<PlayerData> Output { get; set; }
	}
}