using System.Collections.Generic;

namespace RealmAspNet.RealmEye.Definitions.Player
{
	public class FameHistoryData : RealmEyePlayerResponse
	{
		public IList<TimeFame> Hour { get; set; } = new List<TimeFame>();
		public IList<TimeFame> Week { get; set; } = new List<TimeFame>();
		public IList<TimeFame> AllTime { get; set; } = new List<TimeFame>();
	}

	public struct TimeFame
	{
		public long Time { get; set; }
		public long Fame { get; set; }
	}
}