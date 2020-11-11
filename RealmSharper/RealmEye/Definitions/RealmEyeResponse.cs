namespace RealmSharper.RealmEye.Definitions
{
	public class RealmEyeResponse
	{
		/// <summary>
		/// The status code. This is the status code that corresponds to the initial connection to the RealmEye profile. In other words, if the profile could not be found, expect a 4xx code. If the profile was found, expect a 2xx code.
		/// </summary>
		public ResultCode ResultCode { get; set; }

		/// <summary>
		/// <para>Whether the profile is private or not. This should be checked before performing any actions.</para>
		/// <para>By default, this value is set to <c>true</c>, which indicates that the profile is private.</para>
		/// </summary>
		public bool ProfileIsPrivate { get; set; } = true;

		/// <summary>
		/// <para>Whether the particular section you are looking up is private. This should be checked before performing any actions.</para>
		/// <para>By default, this value is set to <c>true</c>, which indicates that the section is private.</para>
		/// </summary>
		public bool SectionIsPrivate { get; set; } = true;

		/// <summary>
		/// <para>The name that was requested. By default, this name should be the name that was requested.</para>
		/// <para>If the name does exist, then this field should be replaced with that name.</para>
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Creates a generic RealmEyeResponse object from the derived RealmEyeResponse object. Note that this should only be done if the derived object doesn't have any useful data (i.e. if the profile is private). 
		/// </summary>
		/// <param name="resp">The derived object.</param>
		/// <returns>The RealmEyeResponse object.</returns>
		public static RealmEyeResponse GenerateGenericResponse(RealmEyeResponse resp = null)
			=> new RealmEyeResponse
			{
				ProfileIsPrivate = resp?.ProfileIsPrivate ?? true,
				SectionIsPrivate = resp?.SectionIsPrivate ?? true,
				ResultCode = resp?.ResultCode ?? ResultCode.InternalServerError
			};
	}
}