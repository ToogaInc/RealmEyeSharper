using System;
using Newtonsoft.Json;

namespace RealmAspNet.RealmEye.Proxy
{
	public class WebshareResponse
	{
		[JsonProperty("count")]
		public long Count { get; set; }

		[JsonProperty("next")]
		public object Next { get; set; }

		[JsonProperty("previous")]
		public object Previous { get; set; }

		[JsonProperty("results")]
		public Result[] Results { get; set; }
	}

	public class Result
	{
		[JsonProperty("username")]
		public string Username { get; set; }

		[JsonProperty("password")]
		public string Password { get; set; }

		[JsonProperty("proxy_address")]
		public string ProxyAddress { get; set; }

		[JsonProperty("ports")]
		public Ports Ports { get; set; }

		[JsonProperty("valid")]
		public bool Valid { get; set; }

		[JsonProperty("last_verification")]
		public DateTimeOffset LastVerification { get; set; }

		[JsonProperty("country_code")]
		public string CountryCode { get; set; }

		[JsonProperty("country_code_confidence")]
		public long CountryCodeConfidence { get; set; }

		[JsonProperty("city_name")]
		public string CityName { get; set; }
	}

	public class Ports
	{
		[JsonProperty("http")]
		public long Http { get; set; }

		[JsonProperty("socks5")]
		public long Socks5 { get; set; }
	}
}
