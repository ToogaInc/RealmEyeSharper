using RealmAspNet.RealmEye.Definitions.Player;

namespace RealmAspNet.RealmEye.Definitions.Common
{
	public struct GraveyardEntry
	{
		public string DiedOn { get; init; }
		public CharacterSkinInfo CharacterSkin { get; init; }
		public string Character { get; init; }
		public int Level { get; init; }
		public int BaseFame { get; init; }
		public int TotalFame { get; init; }
		public long Experience { get; init; }
		public GearInfo[] Equipment { get; init; }
		public int MaxedStats { get; init; }
		public string KilledBy { get; init; }
		public bool HadBackpack { get; init; }
	}
}