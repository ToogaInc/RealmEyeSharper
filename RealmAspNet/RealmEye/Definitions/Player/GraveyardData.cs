using System.Collections.Generic;
using RealmAspNet.RealmEye.Definitions.Common;

namespace RealmAspNet.RealmEye.Definitions.Player
{
	public class GraveyardData : RealmEyePlayerResponse
	{
		public int GraveyardCount { get; set; }
		public IList<GraveyardEntry> Graveyard { get; set; }
	}
}