namespace RealmAspNet.RealmEye.Definitions.Player
{
	public class RealmEyePlayerResponse : RealmEyeResponse
	{
		/// <summary>
		/// <para>Whether the profile is private or not. This should be checked before performing any actions.</para>
		/// <para>By default, this value is set to <c>true</c>, which indicates that the profile is private.</para>
		/// </summary>
		public bool ProfileIsPrivate { get; init; } = true;

		/// <summary>
		/// <para>Whether the particular section you are looking up is private. This should be checked before performing any actions.</para>
		/// <para>By default, this value is set to <c>true</c>, which indicates that the section is private.</para>
		/// </summary>
		public bool SectionIsPrivate { get; set; } = true;

		/// <summary>
		/// <para>The name that was requested. By default, this name should be the name that was requested.</para>
		/// <para>If the name does exist, then this field should be replaced with that name.</para>
		/// </summary>
		public string Name { get; init; }

		/// <summary>
		/// Creates a generic RealmEyeResponse object from the derived RealmEyeResponse object. Note that this should
		/// only be done if the derived object doesn't have any useful data (i.e. if the profile is private). 
		/// </summary>
		/// <param name="name">The requested name.</param>
		/// <param name="resp">The derived object.</param>
		/// <returns>The RealmEyeResponse object.</returns>
		public static RealmEyePlayerResponse GenerateGenericResponse(string name, RealmEyePlayerResponse resp = null)
			=> new()
			{
				Name = name,
				ProfileIsPrivate = resp?.ProfileIsPrivate ?? true,
				SectionIsPrivate = resp?.SectionIsPrivate ?? true,
				ResultCode = resp?.ResultCode ?? ResultCode.InternalServerError
			};
		
		/// <summary>
		/// Creates a generic RealmEyeResponse object given the status code. This sets everything else to true.
		/// </summary>
		/// <param name="statusCode">The status code object.</param>
		/// <returns>The RealmEyeResponse object.</returns>
		public static RealmEyePlayerResponse GenerateGenericResponse(ResultCode statusCode)
			=> new()
			{
				Name = string.Empty,
				ProfileIsPrivate = true,
				SectionIsPrivate = true,
				ResultCode = statusCode
			};
	}
}