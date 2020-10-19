namespace RealmEyeSharper.Definitions
{
	public enum ResultCode
	{
		/// <summary>
		/// Represents a successful query. 
		/// </summary>
		Success = 200,

		/// <summary>
		/// Represents a failed query due to the query not being found.
		/// </summary>
		NotFound = 400,

		/// <summary>
		/// Represents a failed query due to an internal error.
		/// </summary>
		InternalServerError = 500, 

		/// <summary>
		/// Represents a failed query due to the service being unavailable.
		/// </summary>
		ServiceUnavailable = 503
	}
}