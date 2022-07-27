namespace RealmAspNet.RealmEye.Definitions.Guild
{
	public class RealmEyeGuildResponse : RealmEyeResponse
	{
		/// <summary>
		/// <para>The guild name that was requested. By default, this name should be the guild name that was requested.</para>
		/// <para>If the guild name does exist, then this field should be replaced with that guild name.</para>
		/// </summary>
		public string Name { get; init; }
		
		/// <summary>
		/// Creates a generic RealmEyeResponse object from the derived RealmEyeResponse object. Note that this should
		/// only be done if the derived object doesn't have any useful data (i.e. if the profile is private). 
		/// </summary>
		/// <param name="name">The requested name.</param>
		/// <param name="resp">The derived object.</param>
		/// <returns>The RealmEyeResponse object.</returns>
		public static RealmEyeGuildResponse GenerateGenericResponse(string name, RealmEyeGuildResponse resp = null)
			=> new()
			{
				Name = name,
				ResultCode = resp?.ResultCode ?? ResultCode.InternalServerError
			};
	}
}