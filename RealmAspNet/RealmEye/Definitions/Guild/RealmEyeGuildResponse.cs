namespace RealmAspNet.RealmEye.Definitions.Guild
{
	public class RealmEyeGuildResponse : RealmEyeResponse
	{
		/// <summary>
		/// <para>The guild name that was requested. By default, this name should be the guild name that was requested.</para>
		/// <para>If the guild name does exist, then this field should be replaced with that guild name.</para>
		/// </summary>
		public string Name { get; init; }
	}
}