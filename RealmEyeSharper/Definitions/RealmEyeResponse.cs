namespace RealmEyeSharper.Definitions
{
	public abstract class RealmEyeResponse
	{
		/// <summary>
		/// The status code. 
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
		public bool SectionIsPrivate { get; set; } = false;
	}
}